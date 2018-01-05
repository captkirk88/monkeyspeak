using Monkeyspeak.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Logging
{
    public class FileLogger : ILogOutput
    {
        private readonly Level level;
        private readonly string filePath;

        public FileLogger(Level level = Level.Error)
        {
            filePath = Path.Combine(Assembly.GetExecutingAssembly()?.Location, $"{Assembly.GetCallingAssembly()?.GetName().Name}.{level}.log");
            if (!IOPermissions.HasAccess(filePath))
            {
                filePath = Path.Combine(Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.Personal)), Path.GetFileName(filePath));
            }

            if (File.Exists(filePath)) File.WriteAllText(filePath, ""); // make sure it is a clean file
            this.level = level;
        }

        public void Log(LogMessage logMsg)
        {
            if (logMsg.Level != level) return;
            logMsg = BuildMessage(ref logMsg);
            using (FileStream stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Write, 4096))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.WriteLine(logMsg.message);
            }
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
              //.Append(msg.TimeStamp.ToString("dd-MMM-yyyy")).Append(' ')
              .Append((msg.TimeStamp - Process.GetCurrentProcess().StartTime).ToString(@"hh\:mm\:ss\:fff"))
              .Append(" - ")
              .Append(text);
            msg.message = sb.ToString();
            return msg;
        }
    }
}