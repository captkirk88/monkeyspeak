#region Usings

using Monkeyspeak.Logging;
using System;
using System.Runtime.CompilerServices;

#endregion Usings

public static class ExceptionExtensions
{
    public static void Log(this Exception ex, Level level = Level.Debug, [CallerMemberName] string memberName = "")
    {
        if (ex != null)
        {
            switch (level)
            {
                case Level.Info:
                    Logger.Info(ex.Flatten(), memberName);
                    break;

                case Level.Error:
                    Logger.Error(ex.Flatten(), memberName);
                    break;

                case Level.Debug:
                    Logger.Debug(ex.Flatten(), memberName);
                    break;

                case Level.Warning:
                    Logger.Warn(ex.Flatten(), memberName);
                    break;

                default:
                    Logger.Debug(ex.Flatten(), memberName);
                    break;
            }
        }
    }

    public static void Log<T>(this Exception ex, Level level = Level.Debug, [CallerMemberName] string memberName = "")
    {
        if (ex != null)
        {
            switch (level)
            {
                case Level.Info:
                    Logger.Info<T>(ex.Flatten(), memberName);
                    break;

                case Level.Error:
                    Logger.Error<T>(ex.Flatten(), memberName);
                    break;

                case Level.Debug:
                    Logger.Debug<T>(ex.Flatten(), memberName);
                    break;

                case Level.Warning:
                    Logger.Warn<T>(ex.Flatten(), memberName);
                    break;

                default:
                    Logger.Debug<T>(ex.Flatten(), memberName);
                    break;
            }
        }
    }

    public static void LogMessage(this Exception ex, Level level = Level.Debug, [CallerMemberName] string memberName = "")
    {
        if (ex != null)
        {
            switch (level)
            {
                case Level.Info:
                    Logger.Info(ex.Message, memberName);
                    break;

                case Level.Error:
                    Logger.Error(ex.Message, memberName);
                    break;

                case Level.Debug:
                    Logger.Debug(ex.Message, memberName);
                    break;

                case Level.Warning:
                    Logger.Warn(ex.Message, memberName);
                    break;
            }
        }
    }

    public static void LogMessage<T>(this Exception ex, Level level = Level.Debug, [CallerMemberName] string memberName = "")
    {
        if (ex != null)
        {
            switch (level)
            {
                case Level.Info:
                    Logger.Info<T>(ex.Message, memberName);
                    break;

                case Level.Error:
                    Logger.Error<T>(ex.Message, memberName);
                    break;

                case Level.Debug:
                    Logger.Debug<T>(ex.Message, memberName);
                    break;

                case Level.Warning:
                    Logger.Warn<T>(ex.Message, memberName);
                    break;
            }
        }
    }

    public static string Flatten(this Exception exception)
    {
        var stringBuilder = new System.Text.StringBuilder();

        while (exception != null)
        {
            stringBuilder.AppendLine(exception.Message);
            stringBuilder.AppendLine(exception.StackTrace);

            exception = exception.InnerException;
            if (exception != null) stringBuilder.AppendLine("Inner exception:");
        }

        return stringBuilder.ToString();
    }
}