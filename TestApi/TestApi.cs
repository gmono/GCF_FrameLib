using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCF_FrameLib;
namespace TestApi
{
    [AttrWebApi(AttrWebApiSecType.None,"/testapi/")]
    public class TestApi
    {
        public TestApi()
        {

        }
        [WebFunction]
        public string Test(int test,string test2)
        {

            return string.Format("{0}{1}", test, test2);
        }
    }
}
