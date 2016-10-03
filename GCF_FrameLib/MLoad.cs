using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
namespace GCF_FrameLib
{
    /// <summary>
    /// 所谓的模块加载器 用于动态编译源文件为程序集
    /// </summary>
    class MLoad
    {
        /// <summary>
        /// 加载CS文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        static public Assembly LoadCS(string path)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters par = new CompilerParameters();
            par.ReferencedAssemblies.Add("System.dll");
            par.ReferencedAssemblies.Add(@"C:\Users\gaozi.DESKTOP-8ESAD0C\Documents\Visual Studio 2015\Projects\GCF_FrameLib\GCF_FrameLib\bin\Debug\GCF_FrameLib.dll");
            par.GenerateExecutable = false;
            par.GenerateInMemory = true;
            CompilerResults result = provider.CompileAssemblyFromFile(par, new string[] { path });

            //var t = new System.IO.FileInfo(path);
            if (result.Errors.HasErrors)
            {
                throw new Exception("编译错误");
            }
            var ret = result.CompiledAssembly;
            return ret;
        }
    }
}
