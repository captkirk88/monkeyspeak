using Microsoft.CSharp;
using Monkeyspeak;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;

namespace msxc
{
    internal class ExeBuilder
    {
        private string output;
        private Page page;
        private CSharpCodeProvider csharpProvider;
        private CompilerParameters cp;
        private string tempScriptFile;
        private bool debugging;

        public ExeBuilder(Page page, string outputFilePath, bool debug = false, params string[] dllsWithTriggers)
        {
            output = outputFilePath;
            debugging = debug;
            this.page = page;
            page.Engine.Options.Debug = debugging;
            csharpProvider = new CSharpCodeProvider();
            cp = new CompilerParameters
            {
                GenerateExecutable = true,
                GenerateInMemory = false,
                TreatWarningsAsErrors = false,
                OutputAssembly = outputFilePath,
                IncludeDebugInformation = debug,
                MainClass = "Script.Program"
            };
            if (!debug) cp.CompilerOptions = "/optimize";

            cp.ReferencedAssemblies.Add("mscorlib.dll");
            cp.ReferencedAssemblies.Add("System.dll");
            cp.ReferencedAssemblies.Add("System.Core.dll");
            cp.ReferencedAssemblies.Add("System.Data.dll");
            cp.ReferencedAssemblies.Add("System.Xml.dll");
            cp.ReferencedAssemblies.Add("Monkeyspeak.dll");

            foreach (var dllWithTrigger in dllsWithTriggers)
                cp.ReferencedAssemblies.Add(dllWithTrigger);

            tempScriptFile = "script.msx";
            Console.WriteLine("Compiling script...");
            page.CompileToFile(tempScriptFile);

            if (csharpProvider.Supports(GeneratorSupport.Resources))
            {
                cp.EmbeddedResources.Add(tempScriptFile);
            }
            else
            {
                throw new Exception("Cannot compile with default code provider.");
            }
        }

        public void Build()
        {
            CompilerResults results = csharpProvider.CompileAssemblyFromSource(cp, @"
using System;
using System.Data;
using System.IO;
using System.Reflection;
using Monkeyspeak;
using Monkeyspeak.Logging;

namespace Script {
	class Program {
		static int Main(string[] args){
			MonkeyspeakEngine engine = new MonkeyspeakEngine();
            engine.Options.Debug = " + (debugging ? "true" : "false") + @";
			Page page = null;
			try{
				using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(" + "\"script.msx\"" + @"))
					page = engine.LoadCompiledStream(stream);

				if (page != null){
					page.SetTriggerHandler(TriggerCategory.Cause, 0, rdr=> true);

                    page.LoadAllLibraries();
                    page.RemoveLibrary<Monkeyspeak.Libraries.Debug>();

					page.Execute(0);
					Logger.Debug(Trigger Count: page.Size);

					foreach(var desc in page.GetTriggerDescriptions()) Logger.Debug(desc);
					return 0;
				}else return -1;
			}catch(Exception ex){
                Logger.Debug(ex);
				return -1;
			}
			return 0;
		}
	}
}
");
            // cleanup temp files
            File.Delete(tempScriptFile);

            if (results.Errors.HasErrors)
            {
                StringBuilder errors = new StringBuilder("Compiler Errors :\r\n");
                foreach (CompilerError error in results.Errors)
                {
                    errors.AppendFormat("Line {0},{1}\t: {2}\n", error.Line, error.Column, error.ErrorText);
                }
                throw new Exception(errors.ToString());
            }
            else
            {
                Console.WriteLine("No errors.");
                Console.WriteLine("Completed.");
            }
        }
    }
}