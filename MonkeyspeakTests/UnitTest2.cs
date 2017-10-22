using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monkeyspeak;
using Monkeyspeak.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyspeakTests
{
    [TestClass]
    public class UnitTest2
    {
        private string testScript = @"
*This is a comment
(0:0) when the script is started,
		(5:150) take variable %i and add 1 to it.
		(5:102) print {%i} to the console.
";

        private class TestReflection
        {
            [Monkeyspeak.TriggerHandler(TriggerCategory.Cause, 1000, "TestTriggerHandlerMethod")]
            public bool TestTriggerHandlerCauseMethod(TriggerReader reader)
            {
                return true;
            }

            [Monkeyspeak.TriggerHandler(TriggerCategory.Effect, 1001, "test print 2 {...}")]
            [Monkeyspeak.TriggerHandler(TriggerCategory.Effect, 1000, "test print {...}")]
            public bool TestTriggerHandlerEffectMethod(TriggerReader reader)
            {
                if (reader.PeekString()) Console.WriteLine(reader.ReadString());
                else Console.WriteLine("Error!!!... no value.");
                return true;
            }
        }

        [TestMethod]
        public void TestCompileToFile()
        {
            var engine = new MonkeyspeakEngine
            {
                Options = { TriggerLimit = int.MaxValue }
            };

            var sb = new StringBuilder(testScript);
            for (int i = 0; i <= 50000; i++)
            {
                sb.AppendLine();
                sb.AppendLine(UnitTest1.tableScript);
                sb.AppendLine();
            }
            Stopwatch watch = Stopwatch.StartNew();
            var oldPage = engine.LoadFromString(sb.ToString());
            oldPage.SetTriggerHandler(TriggerCategory.Cause, 0, UnitTest1.HandleScriptStartCause);
            watch.Stop();
            Console.WriteLine($"Loaded in {watch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Page Trigger Count: {oldPage.Size}");
            watch.Restart();
            oldPage.CompileToFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "test.msx"));
            watch.Stop();
            Console.WriteLine($"Compiled in {watch.ElapsedMilliseconds} ms");
            watch.Restart();
            var page = engine.LoadCompiledFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "test.msx"));
            page.SetTriggerHandler(TriggerCategory.Cause, 0, UnitTest1.HandleScriptStartCause);
            watch.Stop();
            Console.WriteLine($"Loaded compiled in {watch.ElapsedMilliseconds} ms");
            page.LoadAllLibraries();
            page.RemoveLibrary<Monkeyspeak.Libraries.Debug>();

            Console.WriteLine("Page Trigger Count: " + page.Size);
            //page.Execute(0);
            page.Dispose();
        }

        [TestMethod]
        public void DeleteVariable()
        {
            var test = @"
0:0 when the script starts,
    5:100 set variable %hi to {hi}.
    5:107 delete variable %hi.
    1:100 and variable %hi is defined,
        5:102 print {@var %hi does exist} to the console.
    1:101 and variable %hi is not defined,
        5:102 print {@var %hi does not exist} to the console.
";

            var engine = new Monkeyspeak.MonkeyspeakEngine
            {
                Options = { TriggerLimit = int.MaxValue, Debug = true }
            };

            var page = engine.LoadFromString(test);

            page.SetTriggerHandler(TriggerCategory.Cause, 0, UnitTest1.HandleScriptStartCause);

            page.LoadAllLibraries();

            var tasks = new Task[1];
            for (int i = 0; i <= tasks.Length - 1; i++)
                tasks[i] = Task.Run(async () => await page.ExecuteAsync());

            foreach (var var in page.Scope)
            {
                Logger.Debug(var);
            }

            Task.WaitAll(tasks);
            Console.WriteLine("Page Trigger Count: " + page.Size);
            // Result is execution is parallel! Awesome!
        }

        [TestMethod]
        public void TestParallelExecute()
        {
            var ioTestString = @"
(0:0) when the script starts,
    (5:100) set variable %file to {test.txt}.
	(1:200) and the file {%file} exist,
		(5:202) delete file {%file}.
		(5:203) create file {%file}.

(0:0) when the script starts,
		(5:102) print {%file} to the console.
		(5:150) take variable %increment and add 1 to it.
		(5:102) print {Execution increment %increment} to the console.

(0:0) when the script starts,
	(1:200) and the file {%file} exists,
	(1:203) and the file {%file} can be written to,
		(5:200) append {Hello World from Monkeyspeak %VERSION!} to file {%file}.

(0:0) when the script starts,
	(5:150) take variable %test and add 2 to it.
	(5:102) print {%test} to the console.
";

            var engine = new Monkeyspeak.MonkeyspeakEngine
            {
                Options = { TriggerLimit = int.MaxValue }
            };
            engine.Options.Debug = false;
            var page = engine.LoadFromString(UnitTest1.tableScript);

            page.SetTriggerHandler(TriggerCategory.Cause, 0, UnitTest1.HandleScriptStartCause);

            page.LoadAllLibraries();

            var tasks = new Task[5];
            for (int i = 0; i <= tasks.Length - 1; i++)
                tasks[i] = Task.Run(async () => await page.ExecuteAsync(0));

            Console.WriteLine("Page Trigger Count: " + page.Size);
            Task.WaitAll(tasks);
            foreach (var variable in page.Scope)
            {
                Console.WriteLine(variable.ToString());
            }
        }

        public bool HandleAllCauses(Monkeyspeak.TriggerReader reader)
        {
            return true;
        }

        private void DebugAllErrors(Monkeyspeak.Trigger trigger, Exception ex)
        {
            Console.WriteLine("Error with " + trigger);
#if DEBUG
            throw ex;
#endif
        }
    }
}