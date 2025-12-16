using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace MatchPuzzle.Core.Domain
{
    /// <summary>
    /// Represents level data (grid size and initial block configuration)
    /// </summary>
    [Serializable]
    public class Level
    {
        [SerializeField, JsonProperty] private int levelNumber;
        [SerializeField, JsonProperty] private int rows;
        [SerializeField, JsonProperty] private int columns;
        [SerializeField, JsonProperty] private List<BlockData> blocks;
        
        [JsonIgnore] public int LevelNumber => levelNumber;
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
        [JsonIgnore] public List<BlockData> Blocks
        {
            get => blocks;
            set => blocks = value;
        }

        public Level()
        {
            this.blocks = new List<BlockData>();
        }

        public Level(int levelNumber, int rows, int columns)
        {
            this.levelNumber = levelNumber;
            this.rows = rows;
            this.columns = columns;
            this.blocks = new List<BlockData>();
        }
    }
}