using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MatchPuzzle.Infrastructure.Services.LevelRepository
{
    [CreateAssetMenu(fileName = "LevelConfiguration", menuName = "MatchPuzzle/Level Configuration")]
    public class LevelConfiguration : ScriptableObject
    {
        [SerializeField] private List<LevelMetadata> levels = new List<LevelMetadata>();

        public IReadOnlyList<LevelMetadata> Levels => levels;

        public int LevelCount => levels?.Count ?? 0;

        public void SetLevels(List<LevelMetadata> entries)
        {
            levels = entries ?? new List<LevelMetadata>();
        }

        public LevelMetadata GetMetadata(int levelNumber)
        {
            return levels.FirstOrDefault(x => x.LevelNumber == levelNumber);
        }

        public void Sort()
        {
            levels = levels.OrderBy(x => x.LevelNumber).ToList();
        }
    }
}
