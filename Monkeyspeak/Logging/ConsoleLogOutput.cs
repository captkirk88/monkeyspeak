#region Usings

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#endregion Usings

namespace Monkeyspeak.Logging
{
    public class DebuggerLogOutput : ConsoleLogOutput
    {
        public override void Log(LogMessage logMsg)
        {
            if (logMsg.message == null)
                return;
            try
            {
                ConsoleColor original = Console.ForegroundColor;
                ConsoleColor color = ConsoleColor.White;
                switch (logMsg.Level)
                {
                    case Level.Debug:
                    case Level.Warning:
                        color = ConsoleColor.Yellow;
                        break;

                    case Level.Error:
                        color = ConsoleColor.Red;
                        break;

                    case Level.Info:
                        color = ConsoleColor.White;
                        break;
                }
                Console.ForegroundColor = color;
            }
            catch
            {
            }

            Debug.Write(BuildMessage(logMsg));

            try
            {
                Console.ResetColor();
            }
            catch
            {
            }
        }
    }

    public class ConsoleLogOutput : ILogOutput
    {
        private Stack<LogMessage> history = new Stack<LogMessage>();
        private ConcurrentQueue<string> queue = new ConcurrentQueue<string>();

        private Task logTask;

        public ConsoleLogOutput()
        {
            logTask = Task.Run(() =>
            {
                while (!Environment.HasShutdownStarted)
                {
                    string msg;
                    if (queue.TryDequeue(out msg))
                    {
                        if (!Debugger.IsAttached)
                            Console.WriteLine(msg);
                        else Debug.WriteLine(msg);
                    }
                }
            });
            AppDomain.CurrentDomain.ProcessExit += (sender, e) => logTask.Wait(TimeSpan.FromMilliseconds(500));
        }

        private static readonly DateTime startTime = DateTime.Now;

        protected String BuildMessage(LogMessage msg)
        {
            var level = msg.Level;
            var text = msg.message;
            var sb = new StringBuilder();
            sb.Append(Thread.CurrentThread.Name ?? "Thread+" + Thread.CurrentThread.GetHashCode())
              .Append(' ')
              .Append(DateTime.Now.Subtract(TimeSpan.FromDays(1)).ToString("dd-MMM-yyyy "))
              .Append((DateTime.Now - startTime).ToString(@"hh\:mm\:ss\:fff"))
              .Append(' ')
              .Append(level.ToString().ToUpper())
              .Append(" - ")
              .Append(text);
            return sb.ToString();
        }

        public virtual void Log(LogMessage logMsg)
        {
            if (logMsg.message == null)
                return;
            queue.Enqueue(BuildMessage(logMsg));
        }
    }
}