using System;
using Newtonsoft.Json;
using UnityEngine;

namespace MatchPuzzle.Core.Domain
{
    /// <summary>
    /// Serializable block data for level definitions
    /// </summary>
    [Serializable]
    public class BlockData
    {
        [SerializeField, JsonProperty] private BlockTypeId type;
        [SerializeField, JsonProperty] private int row;
        [SerializeField, JsonProperty] private int column;

        [JsonIgnore] public BlockTypeId Type => type;
        [JsonIgnore] public int Row => row;
        [JsonIgnore] public int Column => column;

        public BlockData()
        {
        }

        public BlockData(BlockTypeId type, int row, int column)
        {
            this.type = type;
            this.row = row;
            this.column = column;
        }

        public GridPosition ToGridPosition()
        {
            return new GridPosition(Row, Column);
        }
    }
}