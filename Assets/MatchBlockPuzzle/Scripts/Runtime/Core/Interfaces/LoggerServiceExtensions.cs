using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MatchPuzzle.Core.Interfaces
{
    /// <summary>
    /// Helper extensions to make logger usage concise and provide Debug fallbacks when no logger is available.
    /// </summary>
    public static class LoggerServiceExtensions
    {
        public static void LogTrace(this ILoggerService logger, string message, Object context = null)
        {
            LogInternal(logger, LogLevel.Trace, message, context);
        }

        public static void LogDebug(this ILoggerService logger, string message, Object context = null)
        {
            LogInternal(logger, LogLevel.Debug, message, context);
        }

        public static void LogInformation(this ILoggerService logger, string message, Object context = null)
        {
            LogInternal(logger, LogLevel.Information, message, context);
        }

        public static void LogWarning(this ILoggerService logger, string message, Object context = null)
        {
            LogInternal(logger, LogLevel.Warning, message, context);
        }

        public static void LogError(this ILoggerService logger, string message, Object context = null, Exception exception = null)
        {
            LogInternal(logger, LogLevel.Error, message, context, exception);
        }

        public static void LogCritical(this ILoggerService logger, string message, Object context = null, Exception exception = null)
        {
            LogInternal(logger, LogLevel.Critical, message, context, exception);
        }

        public static void LogException(this ILoggerService logger, Exception exception, string message = null, Object context = null)
        {
            var combined = string.IsNullOrEmpty(message) ? exception?.Message : $"{message}\n{exception}";
            LogInternal(logger, LogLevel.Error, combined, context, exception);
        }

        private static void LogInternal(
            ILoggerService logger,
            LogLevel level,
            string message,
            Object context,
            Exception exception = null
        )
        {
            if (logger != null)
            {
                logger.Log(level, message, context, exception);
                return;
            }

            if (level == LogLevel.None)
                return;

            switch (level)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Information:
                    Debug.Log(message, context);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(message, context);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    if (exception != null)
                    {
                        Debug.LogException(exception, context);
                    }
                    else
                    {
                        Debug.LogError(message, context);
                    }
                    break;
            }
        }
    }
}