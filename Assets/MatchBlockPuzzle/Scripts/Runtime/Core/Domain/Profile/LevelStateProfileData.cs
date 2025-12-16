using Newtonsoft.Json;
using UnityEngine;

namespace MatchPuzzle.Core.Domain
{
    /// <summary>
    /// Represents the state of a specific level
    /// </summary>
    [System.Serializable]
    public class LevelStateProfileData
    {
        [JsonProperty, SerializeField] int levelNumber;
        [JsonProperty, SerializeField] int rows;
        [JsonProperty, SerializeField] int columns;
        [JsonProperty, SerializeField] BlockStateProfileData[] blocks = System.Array.Empty<BlockStateProfileData>();

        [JsonIgnore] public int LevelNumber
        {
            get => levelNumber;
            set => levelNumber = value;
        }

        [JsonIgnore] public int Rows
        {
            get => rows;
            set => rows = value;
        }
        [JsonIgnore] public int Columns
        {
            get => columns;
            set => columns = value;
        }
        [JsonIgnore] public BlockStateProfileData[] Blocks
        {
            get => blocks;
            set => blocks = value;
        }

        public LevelStateProfileData()
        {
        }
    }
}