using System;
using MatchPuzzle.Core.Interfaces;
using UnityEngine;

namespace MatchPuzzle.Infrastructure.Services
{
    public class UnityLoggerService : ILoggerService
    {
        private readonly ILogger _unityLogger;
        private bool _isEnabled;

        public LogLevel MinimumLevel { get; set; }
        public bool IsEnabled => _isEnabled;

        public UnityLoggerService(LogLevel minimumLevel, ILogger unityLogger, bool isEnabled)
        {
            _unityLogger = unityLogger;
            MinimumLevel = minimumLevel;
            _isEnabled = isEnabled;
            _unityLogger.logEnabled = isEnabled;
        }

        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
            _unityLogger.logEnabled = enabled;
        }

        public void Log(LogLevel level, string message, UnityEngine.Object context = null, Exception exception = null)
        {
            if (!_isEnabled || level == LogLevel.None || level < MinimumLevel)
            {
                return;
            }

            var logType = MapLogType(level, exception != null);

            if (logType == LogType.Exception && exception != null)
            {
                _unityLogger.LogException(exception, context);
                return;
            }

            _unityLogger.Log(logType, (object) message, context);
        }

        private static LogType MapLogType(LogLevel level, bool hasException)
        {
            switch (level)
            {
                case LogLevel.Warning:
                    return LogType.Warning;
                case LogLevel.Error:
                    return hasException ? LogType.Exception : LogType.Error;
                case LogLevel.Critical:
                    return hasException ? LogType.Exception : LogType.Error;
                default:
                    return LogType.Log;
            }
        }
    }
}