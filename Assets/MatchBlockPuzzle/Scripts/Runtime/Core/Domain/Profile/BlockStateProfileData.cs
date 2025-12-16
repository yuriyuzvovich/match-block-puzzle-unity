using Newtonsoft.Json;
using UnityEngine;

namespace MatchPuzzle.Core.Domain
{
    /// <summary>
    /// Serializable block state
    /// </summary>
    [System.Serializable]
    public class BlockStateProfileData
    {
        [JsonProperty,SerializeField] private long id; // Lightweight numeric id
        [JsonProperty,SerializeField] private BlockTypeId type;
        [JsonProperty,SerializeField] private int row;
        [JsonProperty,SerializeField] private int column;

        [JsonIgnore] public long Id => id;
        [JsonIgnore] public BlockTypeId Type => type;
        [JsonIgnore] public int Row => row;
        [JsonIgnore] public int Column => column;

        public BlockStateProfileData()
        {
        }

        public BlockStateProfileData(long id, BlockTypeId type, int row, int column)
        {
            this.id = id;
            this.type = type;
            this.row = row;
            this.column = column;
        }
    }
}