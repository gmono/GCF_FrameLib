using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCF_FrameLib;
namespace consoletest
{
    [AttrWebApi]
    class webclass
    {
        [WebFunction]
        public string hello(int test)
        {
            return test.ToString();
        }
        string str="默认文本";
        [WebFunction]
        public string getstr()
        {
            return str;
        }
        [WebFunction]
        public void setstr(string s)
        {
            str = s;
        }
    }
}
