using Monkeyspeak;
using System;
using System.IO;
using System.Text;

namespace msxc
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            StringBuilder help = new StringBuilder();
            {
                help.AppendLine("Monkeyspeak Script Compiler Help ================")
                    .AppendLine("Usage mxsc.exe \"script.ms\" [-e]")
                    .AppendLine("\tParameters:")
                    .AppendLine("\t\tMonkeyspeak script file. (eg. \"in.ms\")")
                    .AppendLine("\tOptional Parameters:")
                    .AppendLine("\t\t-e Write error output to error.log file (default: console/terminal)");
            }
            Console.WriteLine(help.ToString());
            for (int i = 0; i <= args.Length - 1; i++)
                Console.WriteLine("Arg{0}: {1}", i, args[i]);
            Console.WriteLine();
#if DEBUG
            Console.ReadLine();
#endif
            if (args == null || args.Length < 1) return -1;
            string filePath = args[0];
            string output = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".msx");
            var engine = new MonkeyspeakEngine();
            try
            {
                Page page;
                using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    FileInfo fInfo = new FileInfo(filePath);
                    if (fInfo.Extension != "msx")
                    {
                        page = engine.LoadFromStream(stream);
                    }
                    else
                    {
                        page = engine.LoadCompiledStream(stream);
                    }
                }
                if (page != null)
                {
                    bool debugInformation = false;
                    for (int i = 0; i <= args.Length - 1; i++)
                    {
                        if (args[i].Equals("-e"))
                        {
                            Console.SetError(new StreamWriter(new FileStream("error.log", FileMode.Create)));
                        }
                    }

                    //var exeBuilder = new ExeBuilder(page, args[1], debugInformation);
                    //exeBuilder.Build();
                    if (filePath.EndsWith(".msx"))
                    {
                        
                    }
                    else
                        page.CompileToFile(output);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                if (ex.InnerException != null)
                {
                    Console.Error.WriteLine("InnerException: ");
                    Console.Error.WriteLine(ex.InnerException);
                }
                Console.Error.WriteLine();
                Console.WriteLine(help.ToString());
                return -404;
            }
            return 0;
        }
    }
}