using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCF_FrameLib
{
    [AttributeUsage(AttributeTargets.Property,AllowMultiple =false,Inherited =true)]
    class WebValue:Attribute
    {
        public WebValue() { }
    }
}
