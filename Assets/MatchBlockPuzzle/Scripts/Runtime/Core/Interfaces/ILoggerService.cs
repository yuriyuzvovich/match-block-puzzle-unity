using System;
using Object = UnityEngine.Object;

namespace MatchPuzzle.Core.Interfaces
{
    /// <summary>
    /// Abstraction for game logging with level filtering and enable/disable support.
    /// </summary>
    public interface ILoggerService
    {
        /// <summary>
        /// Minimum level that will be logged.
        /// </summary>
        LogLevel MinimumLevel { get; set; }

        /// <summary>
        /// True when logging is active.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Enables or disables logging output.
        /// </summary>
        void SetEnabled(bool enabled);

        /// <summary>
        /// Logs a message with the given severity.
        /// </summary>
        void Log(LogLevel level, string message, Object context = null, Exception exception = null);
    }
}
