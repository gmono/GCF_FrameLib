using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCF_FrameLib
{
    [AttributeUsage(AttributeTargets.Method,AllowMultiple =false,Inherited =true)]
    public class WebFunction:Attribute
    {
        public WebFunction() { }
    }
}
