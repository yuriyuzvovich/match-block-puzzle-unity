using System;

namespace MatchPuzzle.Features.UI
{
    public interface IMatchPuzzleUIPresenter
    {
        event Action RestartButtonClicked;
        event Action NextButtonClicked;
        void Initialize();
        void Dispose();
    }
}
