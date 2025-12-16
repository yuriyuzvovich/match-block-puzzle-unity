using Newtonsoft.Json;
using UnityEngine;

namespace MatchPuzzle.Core.Domain
{
    /// <summary>
    /// Represents the complete game state that needs to be persisted
    /// </summary>
    [System.Serializable]
    public class GameStateProfileData
    {
        [JsonProperty, SerializeField] private int currentLevelNumber = 1;
        [JsonProperty, SerializeField] private LevelStateProfileData currentLevelState;

        [JsonIgnore] public int CurrentLevelNumber
        {
            get => currentLevelNumber;
            set => currentLevelNumber = value;
        }

        [JsonIgnore] public LevelStateProfileData CurrentLevelState
        {
            get => currentLevelState;
            set => currentLevelState = value;
        }

        public GameStateProfileData()
        {
        }
    }
}