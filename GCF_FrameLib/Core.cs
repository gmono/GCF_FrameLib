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
    public class Core
    {
        public Core()
        {

        }

        Dictionary<string, object> objs = new Dictionary<string, object>();
        //GET响应
        void GetRequest(string text)
        {
            //从1开始
            for(int i=1;i<text.Length;++i)
            {
                string tstr = text.Substring(0, text.Length - i);
                if(objs.ContainsKey(tstr)
                {
                    object obj = objs[tstr];//定向 就近原则
                    string fname = text.Substring(text.Length - i, i);
                    string[] fns = fname.Split('?');//问号前的是名
                    fname = fns[0];
                    MethodInfo info = obj.GetType().GetMethod(fname);
                    object jsonobj=JsonConvert.DeserializeObject(fns[1]);
                    
                    info.Invoke(obj,)
                }
            }
        }
        void loadtype(Type t)
        {
            
        }
        void loadmod(Assembly asm)
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
