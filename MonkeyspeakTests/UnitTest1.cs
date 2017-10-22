﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monkeyspeak;
using Monkeyspeak.Libraries;
using Monkeyspeak.Utils;
using Monkeyspeak.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyspeakTests
{
    [TestClass]
    public class UnitTest1
    {
        private string testScript = @"
(0:0) when the script is started,
    (5:102) print {Welcome to %MONKEY} to the console.

(0:0) when the script is started,
    (5:102) print {None = %none} to the console.

(0:0) when the script is started,
    (1:666) and false,
        (5:102) print {better not be seen} to the console.

(0:0) when the script is started,
    (1:104) and variable %hello equals {this will be false move on to next condition}
		(5:100) set %hello to {Hello World}.
        (5:101) set %helloNum to 5.69.
        (5:102) print {hello = %hello helloNum = %helloNum} to the console.

(0:0) when the script is started,
        (5:100) set %hello to {Hello World}.
        (5:101) set %num to 5.1212E+003.
        (5:102) print {num = %num} to the console.
        (5:102) print {%hello} to the console.

(0:0) when the script is started,
    *Uncommented version
    (1:104) and variable %hello equals {this will be false move on to next condition}
        (5:102) print {the pen is blue!} to the console
    (1:104) and variable %hello equals {Hello World}
        (5:102) print {the pen is red!} to the console
        (5:102) print {hello = %hello helloNum = %helloNum} to the console
        (5:100) set %hello to {@%helloNum}
        (5:101) set %helloNum to 5.6969.
        (5:102) print {hello = %hello helloNum = %helloNum} to the console
        (5:115) call job 1.
        (5:115) call job 2.

(0:0) when the script is started,
        (5:250) create a table as %myTable.
        (5:252) with table %myTable put {Hello World} in it at key {myKey}.
        (5:102) print {%myTable[myKey]} to the console.
        (6:250) for each entry in table %myTable put it into %entry,
            (5:102) print {%entry} to the console.

(0:0) when the script is started,
		(5:10000) create a debug breakpoint here,
        (5:100) set %testVariable to {Modified!}. -- try to modify constant variable
		(5:102) print {%testVariable} to the console.

(0:10000) when a debug breakpoint is hit,
		(5:102) print {Hit a breakpoint!} to the console.
        (5:105) raise an error. * dirty exit

(0:100) when job 1 is called,
    (5:102) print {job 1 executed} to the console

(0:100) when job 2 is called,
    (5:102) print {job will not execute because infinite loop possibility} to the console
   (5:115) call job 1.
";

        public static string tableScript = @"
(0:0) when the script is started,
    (5:250) create a table as %myTable.
    (5:100) set %hello to {hi}
    (5:101) set %i to 0
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
    (6:450) while variable %answer is not %life,
        (5:150) take variable %answer and add 1 to it.
        (1:102) and variable %answer equals 21,
            (5:450) exit the current loop.
    (6:454) after the loop is done,
        (5:102) print {We may never know the answer...} to the console.
";

        [TestMethod]
        public void AssemblyInfo()
        {
            var asm = Assembly.GetAssembly(typeof(MonkeyspeakEngine));
            Logger.Info(asm.FullName);
            Logger.Info(asm.ImageRuntimeVersion);
            Logger.Info(asm.GetName().Version);
            Logger.Info(int.MaxValue.ToString());
        }

        [TestMethod]
        public void Tables()
        {
            var engine = new MonkeyspeakEngine();
            engine.Options.Debug = true;
            Page page = engine.LoadFromString(tableScript);

            page.Error += DebugAllErrors;
            page.SetTriggerHandler(TriggerCategory.Condition, 666, AlwaysFalseCond);
            page.LoadAllLibraries();
            page.RemoveLibrary<Monkeyspeak.Libraries.Debug>();
            page.SetVariable("%testVariable", "Hello WOrld", true);

            page.SetTriggerHandler(TriggerCategory.Cause, 0, HandleScriptStartCause);

            // Trigger count created by subscribing to TriggerAdded event and putting triggers into a list.
            Console.WriteLine("Trigger Count: " + page.Size);
            Logger.Assert(page.Size > 0, "Page size was 0 = FAIL!");
            page.Execute();
            foreach (var variable in page.Scope)
            {
                Console.WriteLine($"{variable.ToString()} {variable.GetType().Name}");
            }
        }

        [TestMethod]
        public void DemoTest()
        {
            var engine = new MonkeyspeakEngine();
            engine.Options.Debug = true;
            Page page = engine.LoadFromString(testScript);

            page.Error += DebugAllErrors;

            page.LoadAllLibraries();
            //page.LoadDebugLibrary();
            page.RemoveLibrary<Monkeyspeak.Libraries.Debug>();
            page.SetVariable("%testVariable", "Hello WOrld", true);

            page.SetTriggerHandler(TriggerCategory.Cause, 0, HandleScriptStartCause);
            page.SetTriggerHandler(TriggerCategory.Condition, 666, AlwaysFalseCond);

            // Trigger count created by subscribing to TriggerAdded event and putting triggers into a list.
            Console.WriteLine("Trigger Count: " + page.Size);
            Logger.Assert(page.Size > 0, "Page size was 0 = FAIL!");
            page.Execute();
            foreach (var variable in page.Scope)
            {
                Console.WriteLine(variable.ToString());
            }
        }

        [TestMethod]
        public void LexerPrint()
        {
            //using (var stream = new FileStream("testBIG.ms", FileMode.OpenOrCreate))
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(tableScript)))
            using (Lexer lexer = new Lexer(new MonkeyspeakEngine(), new SStreamReader(stream)))
            {
                foreach (var token in lexer.Read())
                {
                    if (token.Type != TokenType.COMMENT)
                        Logger.Info($"{token} = {new string(lexer.Read(token.ValueStartPosition, token.Length))}");
                }
            }
        }

        [TestMethod]
        public void LexerAndParserPrint()
        {
            var engine = new MonkeyspeakEngine();
            //engine.Options.Debug = true;
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(tableScript));
            using (Lexer lexer = new Lexer(engine, new SStreamReader(stream)))
            {
                Parser parser = new Parser(engine);
                //parser.VisitToken = VisitTokens;
                foreach (var triggerList in parser.Parse(lexer))
                {
                    Logger.Info($"New Block starting with {triggerList.First()}");
                    // each triggerList instance is a new (0:###) block.
                    foreach (var trigger in triggerList)
                    {
                        // check trigger's out here.
                        StringBuilder sb = new StringBuilder();
                        sb.Append(trigger);
                        foreach (var expr in trigger.Contents)
                        {
                            sb.Append(' ').Append(expr).Append(" (").Append(expr.GetType().Name).Append(") ");
                        }
                        Logger.Info(sb.ToString());
                    }
                }
                parser.VisitToken = null;
            }
        }

        [TestMethod]
        public void DebugTest()
        {
            var engine = new MonkeyspeakEngine();
            Page page = engine.LoadFromString(testScript);

            page.Error += DebugAllErrors;

            page.LoadAllLibraries();
            page.RemoveLibrary<Monkeyspeak.Libraries.Debug>();
            //page.LoadDebugLibrary();

            var var = page.SetVariable("%testVariable", "Hello WOrld", true);

            page.SetTriggerHandler(TriggerCategory.Cause, 0, HandleScriptStartCause);

            Console.WriteLine("Trigger Count: " + page.Size);

            page.Execute(0);
        }

        [TestMethod]
        public void DurabilityParseFile()
        {
            var engine = new MonkeyspeakEngine();

            // Set the trigger limit to int.MaxValue to prevent TriggerLimit reached exceptions
            engine.Options.TriggerLimit = int.MaxValue;

            Page page = engine.LoadFromFile("testBIG.ms");

            page.LoadAllLibraries();
            page.RemoveLibrary<Monkeyspeak.Libraries.Debug>();
            page.Error += DebugAllErrors;

            page.SetTriggerHandler(TriggerCategory.Cause, 0, HandleScriptStartCause);

            Console.WriteLine("Trigger Count: " + page.Size);
            page.Execute(0);
        }

        [TestMethod]
        public void DurabilityParseFileAsync()
        {
            var engine = new MonkeyspeakEngine();

            // Set the trigger limit to int.MaxValue to prevent TriggerLimit reached exceptions
            engine.Options.TriggerLimit = int.MaxValue;

            Page page = engine.LoadFromFile("testBIG.ms");

            page.Debug = true;

            page.LoadAllLibraries();
            page.RemoveLibrary<Monkeyspeak.Libraries.Debug>();

            page.Error += DebugAllErrors;

            page.SetTriggerHandler(TriggerCategory.Cause, 0, HandleScriptStartCause);

            Console.WriteLine("Trigger Count: " + page.Size);
            var timer = Stopwatch.StartNew();
            var pageTask = page.ExecuteAsync(0).ContinueWith(task => Logger.Info(timer.Elapsed));
            while (!pageTask.IsCompleted ||
                pageTask.Status == TaskStatus.Running ||
                pageTask.Status == TaskStatus.WaitingToRun ||
                pageTask.Status == TaskStatus.WaitingForActivation)
            {
                Thread.Sleep(100);
            }
        }

        [TestMethod]
        public void DurabilityParseString()
        {
            var engine = new MonkeyspeakEngine();

            // Set the trigger limit to int.MaxValue to prevent TriggerLimit reached exceptions
            engine.Options.TriggerLimit = int.MaxValue;

            var sb = new StringBuilder();
            for (int i = 0; i < 1; i++)
            {
                sb.AppendLine();
                sb.AppendLine("(0:0) when the script is started,");
                sb.AppendLine("(5:100) set %hello to {Hello World}.");
                sb.AppendLine("(1:104) and variable %hello equals {this will be false move on to next condition}");
                sb.AppendLine("(5:102) print {hello = %hello helloNum = % helloNum} to the console.");
                sb.AppendLine("(1:104) and variable %hello equals {Hello World}");
                sb.AppendLine("(5:101) set %helloNum to 5.");
                sb.AppendLine("(5:102) print {hello = %hello helloNum = %helloNum} to the console.");
                sb.AppendLine();
            }
            Logger.Info(sb.ToString());
            Page page = engine.LoadFromString(sb.ToString());
            page.LoadAllLibraries();
            page.RemoveLibrary<Monkeyspeak.Libraries.Debug>();

            var sb2 = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                sb.AppendLine();
                sb.AppendLine("(0:0) when the script is started,");
                sb.AppendLine("(5:100) set %hello to {Hello World}.");
                sb.AppendLine("(1:104) and variable %hello equals {this will be false move on to next condition}");
                sb.AppendLine("(5:102) print {hello = %hello helloNum = % helloNum} to the console.");
                sb.AppendLine("(1:104) and variable %hello equals {Hello World}");
                sb.AppendLine("(5:101) set %helloNum to 5.");
                sb.AppendLine("(5:102) print {hello = %hello helloNum = % helloNum} to the console.");
                sb.AppendLine();
            }

            engine.LoadFromString(page, sb2.ToString());

            page.Error += DebugAllErrors;

            page.SetTriggerHandler(TriggerCategory.Cause, 0, HandleScriptStartCause);

            Logger.Info($"Triggers: {page.Size}");
            page.Execute();
        }

        [TestMethod]
        public void DurabilityParseStringAsync()
        {
            var engine = new MonkeyspeakEngine();

            // Set the trigger limit to int.MaxValue to prevent TriggerLimit reached exceptions
            engine.Options.TriggerLimit = int.MaxValue;

            var sb = new StringBuilder();
            for (int i = 0; i < 10000; i++)
            {
                sb.AppendLine();
                sb.Append("(5:100) set %hello to {Hello World}.");
                sb.AppendLine();
            }

            Page page = engine.LoadFromString(sb.ToString());

            page.LoadAllLibraries();
            page.RemoveLibrary<Monkeyspeak.Libraries.Debug>();

            var sb2 = new StringBuilder();
            for (int i = 0; i < 50000; i++)
            {
                sb2.AppendLine();
                sb2.Append("(5:100) set %hello to {Hello World}.");
                sb2.AppendLine();
            }
            var tasks = new Task[5];
            for (int i = 0; i <= tasks.Length - 1; i++)
            {
                tasks[i] = engine.LoadFromStringAsync(page, sb2.ToString());
                tasks[i].ContinueWith(task => Logger.Info($"Triggers: {page.Size}"));
            }

            page.Error += DebugAllErrors;

            page.SetTriggerHandler(TriggerCategory.Cause, 0, HandleScriptStartCause);

            Task.WaitAll(tasks);
            Logger.Info($"Triggers: {page.Size}");
            //page.Execute(0);
        }

        [TestMethod]
        public void SetGetVariableTest()
        {
            MonkeyspeakEngine engine = new MonkeyspeakEngine();
            engine.Options.VariableCountLimit = 100000;
            Page page = engine.LoadFromString(testScript);

            page.Error += DebugAllErrors;

            for (int i = 0; i <= 10000; i++)
                page.SetVariable(i.ToString(), true.ToString(), false);
            foreach (var variable in page.Scope)
            {
                Console.WriteLine(variable.ToString());
            }
        }

        [TestMethod]
        public void IOLibraryTest()
        {
            var ioTestString = @"
(0:0) when the script starts,
	(5:100) set variable %file to {test.txt}.
	(5:102) print {%file} to the console.

(0:0) when the script starts,
	(1:200) and the file {%file} exists,
		(5:202) delete file {%file}.
		(5:203) create file {%file}.

(0:0) when the script starts,
	(1:200) and the file {%file} exists,
	(1:203) and the file {%file} can be written to,
		(5:200) append {Hello World from Monkeyspeak %VERSION!} to file {%file}.

(0:0) when the script starts,
	(5:150) take variable %test and add 2 to it.
	(5:102) print {%test} to the console.
";
            var engine = new MonkeyspeakEngine();
            var page = engine.LoadFromString(ioTestString);

            page.Error += DebugAllErrors;

            page.LoadAllLibraries();
            page.RemoveLibrary<Monkeyspeak.Libraries.Debug>();

            page.SetTriggerHandler(TriggerCategory.Cause, 0, HandleScriptStartCause);

            page.Execute(0);
            foreach (var v in page.Scope)
            {
                Logger.Debug(v);
            }
        }

        [TestMethod]
        public void ErrorTriggerTest()
        {
            var errorTestScript = @"
(0:0) when the script starts,
        (5:1000) test print {...}
		(5:102) print {This is a test of the error system} to the console
		(5:105) raise an error {Optional message would go here!}
        (5:105) raise an error * this works too!
		(5:102) print {This will NOT be displayed because an error was raised} to the console

(0:0) when the script is started,
    *Uncommented version
    (1:104) and variable %hello equals {this will be false move on to next condition}
		(5:100) set %hello to {Hello World}
        (5:102) print {First condition failed!} to the console
    (1:104) and variable %hello equals {Hello World}
        (5:102) print {Second condition win!} to the console
        (5:101) set %helloNum to 5
        (5:102) print {hello = %hello helloNum = %helloNum} to the console

";
            var engine = new MonkeyspeakEngine();
            Page page = engine.LoadFromString(errorTestScript);

            page.Error += DebugAllErrors;

            page.LoadAllLibraries();
            page.RemoveLibrary<Monkeyspeak.Libraries.Debug>();

            page.SetTriggerHandler(TriggerCategory.Cause, 0, HandleScriptStartCause);

            foreach (var desc in page.GetTriggerDescriptions()) Logger.Info(desc);

            try
            {
                for (int i = 0; i <= 5; i++)
                    page.Execute(0);
            }
            catch (Monkeyspeak.MonkeyspeakException ex)
            {
                Logger.Error(ex);
            }
        }

        [TriggerHandler(TriggerCategory.Effect, 42, "lOLOlololOLOLOlolOlOLoLOlOLOlol")]
        public static bool TestTriggerHandler(TriggerReader reader)
        {
            Console.WriteLine("lOLOlololOLOLOlolOlOLoLOlOLOlol");
            return true;
        }

        [TestMethod]
        public void GetTriggerDescriptionsTest()
        {
            MonkeyspeakEngine engine = new MonkeyspeakEngine();
            Page page = engine.LoadFromString("");

            page.Error += DebugAllErrors;

            page.LoadAllLibraries();
            page.RemoveLibrary<Monkeyspeak.Libraries.Debug>();

            foreach (string desc in page.GetTriggerDescriptions())
            {
                Console.WriteLine(desc);
            }
        }

        [TestMethod]
        public void TimerLibraryTest()
        {
            var timerLibTestScript = @"
(0:0) when the script starts,
    (5:303) pause script execution for 2 seconds.
    (5:304) get the current time in seconds and put it into variable %currentTime.
    (5:102) print {Current Time %currentTime secs} to the console.

(0:0) when the script starts,
    (5:101) set variable %timer to 1.
    (5:300) create timer %timer to go off every 2 second(s) with a start delay of 1 second(s).
    (5:300) create timer 2 to go off every 5 second(s). *don't need delay part here

(0:300) when timer 2 goes off,
(0:300) when timer %timer goes off,
    (5:302) get current timer and put the id into variable %timerId.
    (5:304) get the current uptime and put it into variable %currentTime.
    (5:102) print {Timer %timerId at %currentTime secs} to the console.

(0:300) when timer 2 goes off,
(0:300) when timer %timer goes off,
    (5:302) get current timer and put the id into variable %timerId.
    (5:304) get the current uptime and put it into variable %currentTime.
    (5:102) print {timerId = %timerId at %currentTime secs} to the console.
    (5:150) take variable %i2 and add 1 to it.
    (5:102) print {elapsed count = %i2} to the console.
";
            var engine = new MonkeyspeakEngine();
            Page page = engine.LoadFromString(timerLibTestScript);

            page.Error += DebugAllErrors;

            page.LoadAllLibraries();
            page.RemoveLibrary<Monkeyspeak.Libraries.Debug>();

            page.SetTriggerHandler(TriggerCategory.Cause, 0, HandleScriptStartCause);

            page.Execute(0);
            System.Threading.Thread.Sleep(10000);
            page.Dispose();
        }

        public static bool HandleScriptStartCause(Monkeyspeak.TriggerReader reader)
        {
            return true;
        }

        public static bool AlwaysFalseCond(TriggerReader reader)
        {
            return false;
        }

        public static void DebugAllErrors(TriggerHandler handler, Trigger trigger, Exception ex)
        {
            Logger.Error($"{trigger} {handler.Method.Name}");
            Logger.Debug(ex);
        }

        public static Token VisitTokens(ref Token token)
        {
            return new Token(TokenType.NONE, token.ValueStartPosition, token.Length, token.Position); // lolololol!
        }
    }
}