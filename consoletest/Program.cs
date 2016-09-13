using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCF_FrameLib;
namespace consoletest
{
    class Program
    {
        static void Main(string[] args)
        {
            //以下为独立模式测试
            Core c = new Core();
            c.loadtype(typeof(webclass));
            object res;
            c.stringinvoke(typeof(webclass).FullName, "hello", out res, new string[] { "50" });
            Console.WriteLine(res);
            //多会话模式测试
            Core[] ts = new Core[5];
            for (int i = 0; i < 5; ++i) ts[i] = new Core(true);
            ts[0].loadmod(System.Reflection.Assembly.GetAssembly(typeof(webclass)), "webapitest");
            ts[1].stringinvoke(typeof(webclass).FullName, "hello", out res, new string[] { "40" });
            Console.WriteLine(res);
            //以下为多会话独立对象测试
            ts[0].stringinvoke(typeof(webclass).FullName, "setstr", out res, new string[] { "30" });
           object res1, res2;
            ts[0].stringinvoke(typeof(webclass).FullName, "getstr", out res1, null);
            ts[4].stringinvoke(typeof(webclass).FullName, "getstr", out res2, null);
            Console.WriteLine("0:{0},4:{1}", res1, res2);
            Console.ReadKey();
        }
    }
}
