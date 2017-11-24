#region Usings

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

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

    internal class LogMessageComparer : IComparer<LogMessage>
    {
        public int Compare(LogMessage x, LogMessage y)
        {
            if (x.TimeStamp > y.TimeStamp) return -1;
            if (x.TimeStamp < y.TimeStamp) return 1;
            return 0;
        }
    }

    public struct LogMessage
    {
        public string message;
        private readonly DateTime expires, timeStamp;
        private readonly Level level;

        private readonly Thread curThread;

        private bool IsEmpty
        {
            get { return string.IsNullOrWhiteSpace(message); }
        }

        public bool IsSpam
        {
            get;
            private set;
        }

        public Level Level { get { return level; } }

        public Thread Thread => curThread;

        public DateTime TimeStamp => timeStamp;

        private LogMessage(Level level, string msg, TimeSpan expireDuration)
        {
            this.level = level;
            message = msg;
            var now = DateTime.Now;
            expires = now.Add(expireDuration);
            timeStamp = now;
            IsSpam = false;
            curThread = Thread.CurrentThread;
        }

        public static LogMessage? From(Level level, string msg, TimeSpan expireDuration)
        {
            LogMessage logMsg = new LogMessage(level, msg, expireDuration);
            var now = DateTime.Now;
            bool found = false;

            for (int i = Logger.history.Count - 1; i >= 0; i--)
            {
                var logMessage = Logger.history[i];
                if (!found && logMessage.Equals(logMsg))
                {
                    found = true;
                    break;
                }
                if (logMessage.expires < now)
                    Logger.history.RemoveAt(i);
            }

            if (found && Logger.SuppressSpam)
                logMsg.IsSpam = true;
            else logMsg.IsSpam = false;

            if (!logMsg.IsSpam)
            {
                Logger.history.Add(logMsg);
                return logMsg;
            }
            return null;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return message.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj != null && obj is LogMessage && GetHashCode() == ((LogMessage)obj).GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return message;
        }
    }

    public static class Logger
    {
        private static ILogOutput _logOutput;
        internal static readonly ConcurrentList<LogMessage> history = new ConcurrentList<LogMessage>();
        internal static readonly ConcurrentQueue<LogMessage> queue = new ConcurrentQueue<LogMessage>();
        private static LogMessageComparer comparer = new LogMessageComparer();
        private static object syncObj = new object();
        private static bool _infoEnabled = true;
        private static bool _warningEnabled = true;
        private static bool _errorEnabled = true;
        private static bool _debugEnabled;
        private static bool _suppressSpam;
        private static TimeSpan _messagesExpire = TimeSpan.FromSeconds(10);

        private static bool singleThreaded, initialized;

        private static Task logTask;

        private static CancellationTokenSource cancelToken;

        public static event Action<LogMessage> SpamFound;

        static Logger()
        {
            _logOutput = new ConsoleLogOutput();
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => Error(args.ExceptionObject);

#if DEBUG
            _debugEnabled = true;
#else
            _debugEnabled = false; // can be set via property
#endif
            singleThreaded = true;

            Initialize();
        }

        private static void Initialize()
        {
            cancelToken = new CancellationTokenSource();
            logTask = new Task(() =>
            {
                while (true)
                {
                    Thread.Sleep(10);
                    if (!singleThreaded)
                    {
                        // take a dump
                        Dump();
                    }
                }
            }, cancelToken.Token, TaskCreationOptions.LongRunning);
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

        /// <summary>
        /// Gets or sets a value indicating whether [single threaded].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [single threaded]; otherwise, <c>false</c>.
        /// </value>
        public static bool SingleThreaded
        {
            get
            {
                return singleThreaded;
            }
            set
            {
                singleThreaded = value;
                if (singleThreaded) cancelToken.Cancel(false);
                else
                {
                    if (logTask.Status == TaskStatus.Running ||
                        logTask.Status == TaskStatus.WaitingForActivation ||
                        logTask.Status == TaskStatus.WaitingToRun ||
                        !logTask.IsCompleted)
                    {
                        try
                        {
                            logTask.Wait(100);
                        }
                        catch { /* fuck me */ }
                        return;
                    }
                    logTask.Start();
                }
            }
        }

        private static void Log(LogMessage? msg)
        {
            if (msg == null) return;
            queue.Enqueue(msg.Value);
            if (singleThreaded)
            {
                Dump();
            }
        }

        private static void Dump()
        {
            if (queue.Count == 0) return;
            if (queue.TryDequeue(out LogMessage msg))
            {
                if (msg.IsSpam)
                {
                    SpamFound?.Invoke(msg);
                    if (SuppressSpam)
                        return;
                }
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
        }

        public static bool Assert(bool cond, string failMsg)
        {
            if (!cond)
            {
                Error("ASSERT: " + failMsg);
                return false;
            }
            return true;
        }

        public static bool Assert<T>(bool cond, string failMsg)
        {
            if (!cond)
            {
                Error<T>("ASSERT: " + failMsg);
                return false;
            }
            return true;
        }

        public static bool Assert(Func<bool> cond, string failMsg)
        {
            if (!cond?.Invoke() ?? false)
            {
                Error("ASSERT: " + failMsg);
                return false;
            }
            return true;
        }

        public static bool Assert<T>(Func<bool> cond, string failMsg)
        {
            if (!cond?.Invoke() ?? false)
            {
                Error<T>("ASSERT: " + failMsg);
                return false;
            }
            return true;
        }

        public static bool Fails(bool cond, string failMsg)
        {
            if (cond)
            {
                Error("FAIL: " + failMsg);
                return true;
            }
            return false;
        }

        public static bool Fails<T>(bool cond, string failMsg)
        {
            if (cond)
            {
                Error<T>("FAIL: " + failMsg);
                return true;
            }
            return false;
        }

        public static bool Fails(Func<bool> cond, string failMsg)
        {
            if (cond?.Invoke() ?? false)
            {
                Error("FAIL: " + failMsg);
                return true;
            }
            return false;
        }

        public static bool Fails<T>(Func<bool> cond, string failMsg)
        {
            if (cond?.Invoke() ?? false)
            {
                Error<T>("FAIL: " + failMsg);
                return true;
            }
            return false;
        }

        public static void Debug(object msg)
        {
            Log(LogMessage.From(Level.Debug, msg != null ? msg.ToString() : "null", MessagesExpire));
        }

        public static void Debug<T>(object msg)
        {
            Log(LogMessage.From(Level.Debug, typeof(T).Name + ": " + msg, MessagesExpire));
        }

        public static void DebugFormat(string format, params object[] args)
        {
            Log(
                LogMessage.From(Level.Debug, (String.IsNullOrEmpty(format) ? String.Join("\t", args) : String.Format(format, args)), MessagesExpire));
        }

        public static void DebugFormat<T>(string format, params object[] args)
        {
            Log(
                LogMessage.From(Level.Debug, typeof(T).Name + ": " + (String.IsNullOrEmpty(format) ? String.Join("\t", args) : String.Format(format, args)), MessagesExpire));
        }

        public static void Info(object msg)
        {
            Log(LogMessage.From(Level.Info, msg != null ? msg.ToString() : "null", MessagesExpire));
        }

        public static void Info<T>(object msg)
        {
            Log(LogMessage.From(Level.Info, typeof(T).Name + ": " + msg, MessagesExpire));
        }

        public static void InfoFormat(string format, params object[] args)
        {
            Log(
                LogMessage.From(Level.Info, String.IsNullOrEmpty(format) ? String.Join("\t", args) : String.Format(format, args), MessagesExpire));
        }

        public static void InfoFormat<T>(string format, params object[] args)
        {
            Log(
                LogMessage.From(Level.Info, typeof(T).Name + ": " + (String.IsNullOrEmpty(format) ? String.Join("\t", args) : String.Format(format, args)), MessagesExpire));
        }

        public static void Error(object msg)
        {
            Log(LogMessage.From(Level.Error, msg != null ? msg.ToString() : "null", MessagesExpire));
        }

        public static void Error<T>(object msg)
        {
            Log(LogMessage.From(Level.Error, typeof(T).Name + ": " + msg, MessagesExpire));
        }

        public static void ErrorFormat(string format, params object[] args)
        {
            Log(LogMessage.From(Level.Error, String.Format(format, args), MessagesExpire));
        }

        public static void ErrorFormat<T>(string format, params object[] args)
        {
            Log(LogMessage.From(Level.Error, typeof(T).Name + ": " + String.Format(format, args), MessagesExpire));
        }

        public static void Warn(object msg)
        {
            Log(LogMessage.From(Level.Warning, msg != null ? msg.ToString() : "null", MessagesExpire));
        }

        public static void Warn<T>(object msg)
        {
            Log(LogMessage.From(Level.Warning, typeof(T).Name + ": " + msg, MessagesExpire));
        }

        public static void WarnFormat(string format, params object[] args)
        {
            Log(LogMessage.From(Level.Warning, String.Format(format, args), MessagesExpire));
        }

        public static void WarnFormat<T>(string format, params object[] args)
        {
            Log(LogMessage.From(Level.Warning, typeof(T).Name + ": " + String.Format(format, args), MessagesExpire));
        }
    }
}