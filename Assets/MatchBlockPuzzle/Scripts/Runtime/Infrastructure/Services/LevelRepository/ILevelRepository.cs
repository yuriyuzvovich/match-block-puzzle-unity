using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Domain;

namespace MatchPuzzle.Infrastructure.Services.LevelRepository
{
    public interface ILevelRepository
    {
        int LevelCount { get; }

        UniTask InitializeAsync();
        UniTask<Level> LoadLevelAsync(int levelNumber);
        void ClearCache();
    }
}