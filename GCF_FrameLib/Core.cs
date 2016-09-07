using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
namespace GCF_FrameLib
{
    /// <summary>
    /// 核心类
    /// 多会话模式：所有core对象中 file只加载一次 加载时不创建对象 并放入文件表 objs中存储的obj为null
    ///             invoke函数中检测包含且值为null时调用loadwebobject函数创建新实例
    /// 独立模式：一个这样的core对象中 file独立加载 不考虑文件表 加载时创建对象 不放入文件表 调用时直接从对象表中搜索
    /// 默认为独立模式
    /// 文件表中的“文件名”不一定是真实文件名 但是直接加载的文件会以完整路径作为文件名
    /// </summary>
    public class Core
    {
         static List<Core> multiobjs = new List<Core>();//多会话模式的对象表 为了对所有对象进行同步
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="ismultisession">是否为多会话模式</param>
        public Core(bool ismultisession=false)
        {
            ismulti = ismultisession;
            if(ismulti)
            {
                //如果是多会话模式就从空类表复制一份到对象所有的对象表
                objs = new Dictionary<string, WebObject>(emptyclasses);
                multiobjs.Add(this);//把自己加入对象表
            }
        }
        private bool ismulti;//是否多会话
        class WebObject
        {
            public Type type=null;//对象的type信息 多会话模式时用到
            public object obj=null;//目标对象
            public Dictionary<string, MethodInfo> methods=null;//方法表 多会话模式下为所有对象共享
            public Dictionary<string, PropertyInfo> props=null;//属性表 多会话模式下为所有对象共享
        }
        static SortedSet<string> loadedfiles = new SortedSet<string>();//这是加载过的文件的集合 目前设计用不到 添加多会话模式时用到
        static Dictionary<string, WebObject> emptyclasses = new Dictionary<string, WebObject>();//加载过的类集合 每个多会话模式对象创建时自动复制一份到自己的对象表
        /// <summary>
        /// 对象表 不管什么模式 每个Core对象总有一个对象表
        /// </summary>
        Dictionary<string, WebObject> objs = new Dictionary<string, WebObject>();
        /// <summary>
        /// 调用函数的接口
        /// </summary>
        /// <param name="classname"> 调用的对象的类名 包括命名空间</param>
        /// <param name="funname">函数名</param>
        /// <param name="pars">参数表 由此函数内部转换为具体类型 注意类型只支持基础数据类型以及实现了Parse静态方法的类型</param>
        bool invoke(string classname,string funname, out object result,params string[] pars)
        {
            if(objs.ContainsKey(classname))
            {
                WebObject obj = objs[classname];
                if(obj.methods.ContainsKey(funname))
                {
                    MethodInfo minfo = obj.methods[funname];
                    ParameterInfo[] parinfo = minfo.GetParameters();//获得参数表信息
                    //下面验证传过来的pars是否符合个数要求
                    if(pars.Length<parinfo.Length)
                    {
                        for(int i=pars.Length;i<parinfo.Length;i++)
                        {
                            ParameterInfo pinfo = parinfo[i];
                            if (!pinfo.HasDefaultValue) { result = null; return false; }//只要有一个没有默认值就返回 意味着参数个数小于参数表参数个数时后面的必须全部有默认值
                        }
                    }
                    //此处开始转换对象
                    object[] mpars=new object[pars.Length];
                    int index= 0;
                    foreach(string s in pars)
                    {
                        var pinfo = parinfo[index];
                        Type ptype = pinfo.ParameterType;
                        if (ptype.Name == "string") mpars[index] = s;//是字符串就直接放入
                        else
                        {
                            MethodInfo parsemet = ptype.GetMethod("Parse");
                            if (parsemet == null) { result = null; return false; }//如果有一个参数解析函数没有就返回 调用不合法
                            object p = parsemet.Invoke(null, new object[] { s });//解析字符串
                            if (p == null) { result = null; return false; }//解析失败就返回 调用不合法
                            //成功 加入参数表
                            mpars[index] = p;
                        }
                    }
                    //此处为多会话模式的关键代码 如果可以调用方法 并且需要创建对象就创建对象
                    if (obj.obj == null) createWebObject(obj);//如果对象没有实例就创建一个实例 理论上如果独立模式采用延迟创建方法这个代码也可以工作
                    result=minfo.Invoke(obj.obj, mpars);//调用方法 返回对象
                    return true; 
                }
            }
            result = null; return false; 
        }
        /// <summary>
        /// 创建一个webobject对象的实例
        /// </summary>
        /// <param name="obj">webobject对象</param>
        private void createWebObject(WebObject obj)
        {
            obj.obj = Activator.CreateInstance(obj.type);
        }
        /// <summary>
        /// 加载webobject
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="iscreate">是否创建实例</param>
        private void loadWebObject(WebObject obj,bool iscreate)
        {
            Type t = obj.type;
            //加载方法
            MethodInfo[] mts = t.GetMethods();
            foreach (var m in mts)
            {
                WebFunction fun;
                fun = m.GetCustomAttribute<WebFunction>();
                if (fun == null) continue;
                obj.methods.Add(m.Name, m);
            }
            //加载属性
            PropertyInfo[] pts = t.GetProperties();
            foreach (var p in pts)
            {
                WebValue val;
                val = p.GetCustomAttribute<WebValue>();
                if (val == null) continue;
                obj.props.Add(p.Name, p);
            }
            if (iscreate) createWebObject(obj);//创建对象实例
        }
        public void loadtype(Type t)
        {
            WebObject obj = new WebObject();
            obj.type = t;//加入类型信息
            if (ismulti)
            {
                loadWebObject(obj, false);//不创建实例
                //多会话模式的处理
                emptyclasses.Add(t.FullName,new WebObject() { type=obj.type});
                //以上为将类型复制一个加入空类型表 以便以后创建的多会话对象复制到自身对象表
                //以下为处理以创建多会话对象的对象表
                foreach(var v in multiobjs)
                {
                    v.objs.Add(t.FullName, new WebObject() { type = obj.type });//加入已创建对象的对象表 其中包括自己（构造函数中添加的this）
                }
            }
            else
            {
                loadWebObject(obj,true);//加载这个webobject  如果这里传入false则是采用延迟创建的策略 其他代码无需修改
                                        //把对象加入对象表
                objs.Add(t.FullName, obj);//
            }

            
        }
        /// <summary>
        /// 加载一个程序集
        /// </summary>
        /// <param name="asm">程序集</param>
        /// <param name="name">程序集的filename</param>
        public void loadmod(Assembly asm,string filename)
        { 
            if (ismulti && loadedfiles.Contains(filename)) return; //已经加载返回 三层防护
            Type[] ts = asm.GetTypes();
            foreach(var type in ts)
            {
                AttrWebApi at = null;
                if((at=type.GetCustomAttribute<AttrWebApi>())!=null)
                {
                    
                    loadtype(type);
                }
            }
            if (ismulti) loadedfiles.Add(filename);//多会话模式加载后加入文件表
        }
        /// <summary>
        /// 加载一个模块
        /// </summary>
        /// <param name="file">模块数据来源</param>
        /// <returns>是否成功</returns>
        public bool Load(Stream file,string filename)
        {
            if (ismulti && loadedfiles.Contains(filename)) return true; //已经加载返回成功 二层防护
            //以上为多会话检测
            try
            {
                byte[] filedata = new byte[file.Length];
                file.Read(filedata, 0, (int)file.Length);
                var v = Assembly.Load(filedata);
                loadmod(v,filename);

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
        public bool loadfile(string file)
        {
            string filename = new FileInfo(file).FullName;
            if (ismulti && loadedfiles.Contains(filename)) return true; //已经加载返回成功 一层防护

            try
            {
                var v = Assembly.LoadFile(file);
                loadmod(v,filename);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}
