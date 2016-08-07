using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCF_FrameLib
{
    public enum AttrWebApiSecType
    {
        HTTPS,
        GMSW,
        None
    }
    [AttributeUsage(AttributeTargets.Class,AllowMultiple =false,Inherited =false)]
    public class AttrWebApi:Attribute
    {
        
        /// <summary>
        /// 设置类的公开方式
        /// </summary>
        /// <param name="issec">采用那种安全传输 如果采用 则使用者必须使用对应安全连接方式连接</param>
        /// <param name="route">调用路由 指示怎样的前缀可以被定向到此类</param>
        public AttrWebApi(AttrWebApiSecType issec,string route)
        {
            SecType = issec;
            Route = route;
        }
        public AttrWebApiSecType SecType { get; private set; }
        public string Route { get; private set; }
    }
}
