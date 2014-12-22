using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection; 

namespace CompilerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = false;
            parameters.ReferencedAssemblies.Add("Schare.dll");
            parameters.OutputAssembly = "AutoGen.dll";
            parameters.IncludeDebugInformation = true;


            var pro = CodeDomProvider.CreateProvider("CSharp");
            CompilerResults r = pro.CompileAssemblyFromFile(parameters, "TestCompile.cs");
            


            //verify generation
            Schare.Ishare item = Activator.CreateInstance(Assembly.LoadFrom("AutoGen.dll").GetType("CompilerTest.B")) as Schare.Ishare;


            Console.WriteLine(item.Test); 


        }
    }
}
