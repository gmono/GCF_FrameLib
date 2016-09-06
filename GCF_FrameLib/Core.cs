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
    /// </summary>
    public class Core
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="ismultisession">是否为多会话模式</param>
        public Core(bool ismultisession=false)
        {
            ismulti = ismultisession;
        }
        bool ismulti;//是否多会话
        class WebObject
        {
            public Type type=null;//对象的type信息 多会话模式时用到
            public object obj=null;//目标对象
            public Dictionary<string, MethodInfo> methods=null;//方法表 
            public Dictionary<string, PropertyInfo> props=null;//属性表
        }
        SortedSet<string> loadedfiles = new SortedSet<string>();//这是加载过的文件的集合 目前设计用不到 添加多会话模式时用到
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
        bool invoke(string classname,string funname,params string[] pars)
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
                            if (!pinfo.HasDefaultValue) return false;//只要有一个没有默认值就返回 意味着参数个数小于参数表参数个数时后面的必须全部有默认值
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
                            if (parsemet == null) return false;//如果有一个参数解析函数没有就返回 调用不合法
                            object p = parsemet.Invoke(null, new object[] { s });//解析字符串
                            if (p == null) return false;//解析失败就返回 调用不合法
                            //成功 加入参数表
                            mpars[index] = p;
                        }
                    }
                    minfo.Invoke(obj.obj, mpars);//调用方法
                    return true; 
                }
            }
            return false;
        }
        private void loadWebObject(WebObject obj)
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
            obj.obj = Activator.CreateInstance(t);//创建对象实例
        }
        public void loadtype(Type t)
        {
            WebObject obj = new WebObject();
            obj.type = t;//加入类型信息
            loadWebObject(obj);//加载这个webobject
            //把对象加入对象表
            objs.Add(t.FullName, obj);//
            
        }
        public void loadmod(Assembly asm)
        {
            Type[] ts = asm.GetTypes();
            foreach(var type in ts)
            {
                AttrWebApi at = null;
                if((at=type.GetCustomAttribute<AttrWebApi>())!=null)
                {
                    
                    loadtype(type);
                }
            }

        }
        /// <summary>
        /// 加载一个模块
        /// </summary>
        /// <param name="file">模块数据来源</param>
        /// <returns>是否成功</returns>
        public bool Load(Stream file)
        {
            try
            {
                byte[] filedata = new byte[file.Length];
                file.Read(filedata, 0, (int)file.Length);
                var v = Assembly.Load(filedata);
                loadmod(v);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
        public bool loadfile(string file)
        {
            try
            {
                var v = Assembly.LoadFile(file);
                loadmod(v);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}
