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
    public class ConsoleLogOutput : ILogOutput
    {
        public ConsoleLogOutput()
        {
        }

        protected LogMessage BuildMessage(ref LogMessage msg)
        {
            var level = msg.Level;
            var text = msg.message;
            var sb = new StringBuilder();
            sb.Append('[')
              .Append(level.ToString().ToUpper())
              .Append(']')
              .Append("Thread+" + msg.Thread.ManagedThreadId)
              .Append(' ')
              .Append(msg.TimeStamp.ToString("dd-MMM-yyyy")).Append(' ')
              .Append((msg.TimeStamp - Process.GetCurrentProcess().StartTime).ToString(@"hh\:mm\:ss"))
              .Append(" - ")
              .Append(text);
            msg.message = sb.ToString();
            return msg;
        }

        public virtual void Log(LogMessage logMsg)
        {
            if (logMsg.message == null)
                return;

            logMsg = BuildMessage(ref logMsg);
            var msg = logMsg.message;
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
            if (Debugger.IsAttached)
                Debug.WriteLine(msg);
            Console.WriteLine(msg);
            try
            {
                Console.ResetColor();
            }
            catch { }
        }
    }
}