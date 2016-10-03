using System;
using System.Web;

namespace GCF_FrameLib
{
    public class GCFHandler : IHttpHandler
    {
        /// <summary>
        /// 您将需要在网站的 Web.config 文件中配置此处理程序 
        /// 并向 IIS 注册它，然后才能使用它。有关详细信息，
        /// 请参见下面的链接: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members

        public bool IsReusable
        {
            // 如果无法为其他请求重用托管处理程序，则返回 false。
            // 如果按请求保留某些状态信息，则通常这将为 false。
            get { return true; }
        }
        Core invcore = new Core(); //调用核心
        public void ProcessRequest(HttpContext context)
        {
            //在此处写入您的处理程序实现。
            //这个处理程序处理csapi文件(cs源文件vb的为 vbapi）
            string path = context.Request.Url.AbsolutePath;
            path=path.Substring(1, path.Length - 1);
            var asm = MLoad.LoadCS(AppDomain.CurrentDomain.BaseDirectory+path);
            invcore.loadmod(asm, path);///加载程序集

            var s = context.Request.QueryString.ToString();
            string[] ss = s.Split('&');
            string cls = ss[0];
            string funs = ss[1];
            string[] pars = new string[ss.Length - 2];
            ss.CopyTo(pars, 2);
            object res;
            invcore.stringinvoke(ss[0], ss[1], out res, pars);
            context.Response.Write(res.ToString());
        }

        #endregion
    }
}
