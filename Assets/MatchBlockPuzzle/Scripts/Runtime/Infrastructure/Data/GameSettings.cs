using System.Collections.Generic;
using MatchPuzzle.Core.Domain;
using UnityEngine;

namespace MatchPuzzle.Infrastructure.Data
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "MatchPuzzle/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [SerializeField] private bool _isAutostartEnabled = true;
        [SerializeField] private List<BlockTypeData> _blockTypes = new List<BlockTypeData>();

        private Dictionary<BlockTypeId, BlockTypeData> _blockTypeLookup;
        public bool IsAutostartEnabled => _isAutostartEnabled;

        public BlockTypeData GetBlockTypeData(BlockTypeId type)
        {
            EnsureLookup();

            _blockTypeLookup.TryGetValue(type, out var data);
            return data;
        }

        public IReadOnlyList<BlockTypeData> GetAllBlockTypes()
        {
            return _blockTypes;
        }

        private void OnValidate()
        {
            RebuildLookup();
        }

        private void EnsureLookup()
        {
            if (_blockTypeLookup == null || _blockTypeLookup.Count != _blockTypes.Count)
            {
                RebuildLookup();
            }
        }

        private void RebuildLookup()
        {
            _blockTypeLookup = new Dictionary<BlockTypeId, BlockTypeData>();

            foreach (var data in _blockTypes)
            {
                if (!data)
                    continue;

                var key = data.TypeId;
                if (key.IsNone || _blockTypeLookup.ContainsKey(key))
                    continue;

                _blockTypeLookup[key] = data;
            }
        }
    }
}