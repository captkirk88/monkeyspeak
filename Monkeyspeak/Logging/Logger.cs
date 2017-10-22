#region Usings

using System;
using System.Collections.Concurrent;

#endregion Usings

namespace Monkeyspeak.Logging
{
    public enum Level : byte
    {
        Info = 1,
        Warning = 2,
        Error = 3,
        Debug = 4
    }

    public struct LogMessage
    {
        private static readonly LogMessage Empty = new LogMessage(Level.Info, null, TimeSpan.FromDays(365))
        {
            IsSpam = true
        };

        public readonly string message;
        private readonly DateTime expires;
        private readonly Level level;

        private bool IsEmpty
        {
            get { return string.IsNullOrEmpty(message); }
        }

        public bool IsSpam
        {
            get;
            private set;
        }

        public Level Level { get { return level; } }

        private LogMessage(Level level, string msg, TimeSpan expireDuration)
        {
            this.level = level;
            message = msg;
            expires = DateTime.Now.Add(expireDuration);
            IsSpam = false;
        }

        public static LogMessage From(Level level, string msg)
        {
            LogMessage logMsg = new LogMessage(level, msg, Logger.MessagesExpire);
            var now = DateTime.Now;
            bool found = false;

            logMsg.IsSpam |= found && Logger.SuppressSpam;

            if (!logMsg.IsSpam)
            {
                return logMsg;
            }
            return Empty;
        }

        public override int GetHashCode()
        {
            return message.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is LogMessage && GetHashCode() == ((LogMessage)obj).GetHashCode();
        }

        public override string ToString()
        {
            return message;
        }
    }

    public static class Logger
    {
        private static ILogOutput _logOutput;
        internal static readonly ConcurrentStack<LogMessage> queue = new ConcurrentStack<LogMessage>();
        private static bool _infoEnabled = true;
        private static bool _warningEnabled = true;
        private static bool _errorEnabled = true;
        private static bool _debugEnabled;
        private static bool _suppressSpam;
        private static TimeSpan _messagesExpire = TimeSpan.FromSeconds(10);

        static Logger()
        {
            _logOutput = new ConsoleLogOutput();
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => Error(args.ExceptionObject);
#if DEBUG
            _debugEnabled = true;
#else
            _debugEnabled = false; // can be set via property
#endif
        }

        public static bool InfoEnabled
        {
            get { return _infoEnabled; }
            set { _infoEnabled = value; }
        }

        public static bool WarningEnabled
        {
            get { return _warningEnabled; }
            set { _warningEnabled = value; }
        }

        public static bool ErrorEnabled
        {
            get { return _errorEnabled; }
            set { _errorEnabled = value; }
        }

        public static bool DebugEnabled
        {
            get { return _debugEnabled; }
            set { _debugEnabled = value; }
        }

        public static bool SuppressSpam
        {
            get { return _suppressSpam; }
            set { _suppressSpam = value; }
        }

        /// <summary>
        /// Gets or sets the messages expire time limit.
        /// Messages that have expired are removed from history.
        /// This property used in conjunction with SupressSpam = true prevents
        /// too much memory from being used over time
        /// </summary>
        /// <value>
        /// The messages expire time limit.
        /// </value>
        public static TimeSpan MessagesExpire
        {
            get { return _messagesExpire; }
            set { _messagesExpire = value; }
        }

        /// <summary>
        /// Sets the <see cref="ILogOutput"/>.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <exception cref="System.ArgumentNullException">output</exception>
        public static ILogOutput LogOutput
        {
            get { return _logOutput; }
            set
            {
                _logOutput = value;
            }
        }

        private static void Log(LogMessage msg)
        {
            if (msg.IsSpam && SuppressSpam)
                return;
            switch (msg.Level)
            {
                case Level.Debug:
                    if (!_debugEnabled)
                        return;
                    break;

                case Level.Error:
                    if (!_errorEnabled)
                        return;
                    break;

                case Level.Info:
                    if (!_infoEnabled)
                        return;
                    break;

                case Level.Warning:
                    if (!_warningEnabled)
                        return;
                    break;
            }
            _logOutput.Log(msg);
        }

        public static bool Assert(bool cond, string failMsg)
        {
            if (!cond)
            {
                Error(failMsg);
                return false;
            }
            return true;
        }

        public static bool Assert<T>(bool cond, string failMsg)
        {
            if (!cond)
            {
                Error<T>(failMsg);
                return false;
            }
            return true;
        }

        public static bool Fails(bool cond, string failMsg)
        {
            if (cond)
            {
                Error(failMsg);
                return true;
            }
            return false;
        }

        public static bool Fails<T>(bool cond, string failMsg)
        {
            if (cond)
            {
                Error<T>(failMsg);
                return true;
            }
            return false;
        }

        public static void Debug(object msg)
        {
            if (msg == null)
                msg = "null";
            Log(LogMessage.From(Level.Debug, msg.ToString()));
        }

        public static void Debug<T>(object msg)
        {
            Log(LogMessage.From(Level.Debug, typeof(T).Name + ": " + msg));
        }

        public static void DebugFormat(string format, params object[] args)
        {
            Log(
                LogMessage.From(Level.Debug, (String.IsNullOrEmpty(format) ? String.Join("\t", args) : String.Format(format, args))));
        }

        public static void DebugFormat<T>(string format, params object[] args)
        {
            Log(
                LogMessage.From(Level.Debug, typeof(T).Name + ": " + (String.IsNullOrEmpty(format) ? String.Join("\t", args) : String.Format(format, args))));
        }

        public static void Info(object msg)
        {
            Log(LogMessage.From(Level.Info, msg != null ? msg.ToString() : "null"));
        }

        public static void Info<T>(object msg)
        {
            Log(LogMessage.From(Level.Info, typeof(T).Name + ": " + msg));
        }

        public static void InfoFormat(string format, params object[] args)
        {
            Log(
                LogMessage.From(Level.Info, String.IsNullOrEmpty(format) ? String.Join("\t", args) : String.Format(format, args)));
        }

        public static void InfoFormat<T>(string format, params object[] args)
        {
            Log(
                LogMessage.From(Level.Info, typeof(T).Name + ": " + (String.IsNullOrEmpty(format) ? String.Join("\t", args) : String.Format(format, args))));
        }

        public static void Error(object msg)
        {
            Log(LogMessage.From(Level.Error, msg != null ? msg.ToString() : "null"));
        }

        public static void Error<T>(object msg)
        {
            Log(LogMessage.From(Level.Error, typeof(T).Name + ": " + msg));
        }

        public static void ErrorFormat(string format, params object[] args)
        {
            Log(LogMessage.From(Level.Error, String.Format(format, args)));
        }

        public static void ErrorFormat<T>(string format, params object[] args)
        {
            Log(LogMessage.From(Level.Error, typeof(T).Name + ": " + String.Format(format, args)));
        }

        public static void Warn(object msg)
        {
            Log(LogMessage.From(Level.Warning, msg != null ? msg.ToString() : "null"));
        }

        public static void Warn<T>(object msg)
        {
            Log(LogMessage.From(Level.Warning, typeof(T).Name + ": " + msg));
        }

        public static void WarnFormat(string format, params object[] args)
        {
            Log(LogMessage.From(Level.Warning, String.Format(format, args)));
        }

        public static void WarnFormat<T>(string format, params object[] args)
        {
            Log(LogMessage.From(Level.Warning, typeof(T).Name + ": " + String.Format(format, args)));
        }
    }
}