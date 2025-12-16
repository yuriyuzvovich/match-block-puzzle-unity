using MatchPuzzle.Core.Interfaces;
using UnityEngine;

namespace MatchPuzzle.Infrastructure.Services
{
    /// <summary>
    /// Configures logging behavior per build type.
    /// </summary>
    [CreateAssetMenu(fileName = "LoggingSettings", menuName = "MatchPuzzle/Logging Settings")]
    public class LoggingSettings : ScriptableObject
    {
        [Header("Enable By Build")]
        public bool EnableInEditor = true;
        public bool EnableInDevelopmentBuild = true;
        public bool EnableInReleaseBuild = false;

        [Header("Overrides")]
        public bool ForceEnable;
        public LogLevel MinimumLevel = LogLevel.Information;
    }
}
