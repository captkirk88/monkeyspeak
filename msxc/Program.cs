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
                help.AppendLine("Monkeyspeak Help ================")
                    .AppendLine("Usage mxsc.exe \"script.ms\" \"script.exe\" [-d]")
                    .AppendLine("\tParameters:")
                    .AppendLine("\t\t1) Monkeyspeak script file uncompiled. (eg. \"in.ms\")")
                    .AppendLine("\t\t2) Monkeyspeak compiled script file. (eg. \"out.exe\")")
                    .AppendLine("\tOptional Parameters:")
                    .AppendLine("\t\t1) -d Enable compiling with exe debug information for debugging in visual studio (default: disabled)")
                    .AppendLine("\t\t2) -e Write error output to error.log file (default: console/terminal)");
            }

            if (args.Length < 2)
                Console.WriteLine(help.ToString());
            else
            {
                for (int i = 0; i <= args.Length - 1; i++)
                    Console.WriteLine("Arg{0}: {1}", i, args[i]);
                Console.WriteLine();

                string filePath = args[0];
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
                            if (args[i].Equals("-d")) debugInformation = true;
                            if (args[i].Equals("-e"))
                            {
                                Console.SetError(new StreamWriter(new FileStream("error.log", FileMode.Create)));
                            }
                        }

                        var exeBuilder = new ExeBuilder(page, args[1], debugInformation);
                        exeBuilder.Build();
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
            }
            return 0;
        }
    }
}