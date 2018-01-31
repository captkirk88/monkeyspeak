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
    public class FileLogOutput : ILogOutput, IEquatable<FileLogOutput>
    {
        private readonly Level level;
        private readonly string filePath;

        public FileLogOutput(Level level = Level.Error)
        {
            if (Assembly.GetEntryAssembly() != null)
            {
                filePath = Path.Combine(Assembly.GetEntryAssembly().Location, $"{Assembly.GetEntryAssembly().GetName().Name}.{level}.log");
                if (!IOPermissions.HasAccess(filePath))
                    filePath = Path.Combine(Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.Personal)), $"{Assembly.GetEntryAssembly()?.GetName().Name}.{level}.log");
            }
            else
                filePath = Path.Combine(Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.Personal)), $"{Assembly.GetExecutingAssembly()?.GetName().Name}.{level}.log");

            if (File.Exists(filePath)) File.WriteAllText(filePath, ""); // make sure it is a clean file
            this.level = level;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FileLogOutput);
        }

        public bool Equals(FileLogOutput other)
        {
            return other != null &&
                   level == other.level &&
                   filePath == other.filePath;
        }

        public override int GetHashCode()
        {
            var hashCode = 1789646697;
            hashCode = hashCode * -1521134295 + level.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(filePath);
            return hashCode;
        }

        /// <summary>
        /// Logs the specified log message to the file.
        /// </summary>
        /// <param name="logMsg">The log MSG.</param>
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
              .Append((msg.TimeStamp - Process.GetCurrentProcess().StartTime).ToString("hh:mm:ss:fff"))
              .Append(" - ")
              .Append(text);
            msg.message = sb.ToString();
            return msg;
        }
    }
}