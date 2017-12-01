using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monkeyspeak;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    (5:250) create a table as %myTable.
    (5:100) set %hello to {hi}
    (5:252) with table %myTable put {%hello} in it at key {myKey1}.
    (5:252) with table %myTable put {%hello} in it at key {myKey2}.
    (5:252) with table %myTable put {%hello} in it at key {myKey3}.
    (5:252) with table %myTable put {%hello} in it at key {myKey4}.
    (5:252) with table %myTable put {%hello} in it at key {myKey5}.
    (5:252) with table %myTable put {%hello} in it at key {myKey6}.
    (5:252) with table %myTable put {%hello} in it at key {myKey7}.
    (6:250) for each entry in table %myTable put it into %entry,
        (5:102) print {%entry} to the console.
        (5:150) take variable %i and add 1 to it.
        (5:102) print {%i} to the console.
    (6:454) after the loop is done,
        (5:102) print {I'm done!} to the console.
        (1:108) and variable %myTable is table,
            (5:101) set %myTable[myKey1] to 123
            (5:102) print {%myTable[myKey1]} to the console.

(0:0) when the script is started,
    (5:101) set %answer to 0
    (5:101) set %life to 42
    (5:102) print {The answer to LIFE is...} to the console.
    (6:450) while variable %answer is not %life,
        (5:150) take variable %answer and add 1 to it.
        (1:102) and variable %answer equals 21,
            (5:450) exit the current loop.
    (6:454) after the loop is done,
        (5:102) print {We may never know the answer...} to the console.
";

        [TestMethod]
        public void TestCompileToFile()
        {
            Logger.SingleThreaded = true;
            var engine = new MonkeyspeakEngine
            {
                Options = { TriggerLimit = int.MaxValue, Debug = true }
            };

            var sb = new StringBuilder(testScript);
            /*for (int i = 0; i <= 50; i++)
            {
                sb.AppendLine();
                sb.AppendLine(testScript);
                sb.AppendLine();
            }*/
            Stopwatch watch = Stopwatch.StartNew();
            var oldPage = engine.LoadFromString(sb.ToString());

            watch.Stop();
            Console.WriteLine($"Loaded in {watch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Page Trigger Count: {oldPage.Size}");
            watch.Restart();

            oldPage.CompileToFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "test.msx"));

            watch.Stop();
            Console.WriteLine($"Compiled in {watch.ElapsedMilliseconds} ms");
            watch.Restart();

            var page = engine.LoadCompiledFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "test.msx"));
            page.RemoveLibrary<MyLibrary>();
            watch.Stop();
            Console.WriteLine($"Loaded compiled in {watch.ElapsedMilliseconds} ms");

            page.LoadAllLibraries();
            Console.WriteLine("Page Trigger Count: " + page.Size);
            page.Execute();
            page.Dispose();
            oldPage.Dispose();
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

            page.AddTriggerHandler(TriggerCategory.Cause, 0, UnitTest1.HandleScriptStartCause);

            page.LoadAllLibraries();

            page.Execute();
            foreach (var var in page.Scope)
            {
                Logger.Debug(var);
            }

            Console.WriteLine("Page Trigger Count: " + page.Size);
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