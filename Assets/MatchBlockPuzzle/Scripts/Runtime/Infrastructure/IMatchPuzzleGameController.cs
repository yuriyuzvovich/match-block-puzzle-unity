using System;
using Cysharp.Threading.Tasks;

namespace MatchPuzzle.Infrastructure
{
    public interface IMatchPuzzleGameController : IDisposable
    {
        UniTask StartGameAsync();
    }
}
