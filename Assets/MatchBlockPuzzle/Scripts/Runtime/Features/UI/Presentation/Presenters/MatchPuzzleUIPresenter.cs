using System;

namespace MatchPuzzle.Features.UI
{
    public class MatchPuzzleUIPresenter : IMatchPuzzleUIPresenter
    {
        private readonly IMatchPuzzleUIView _view;

        public event Action RestartButtonClicked;
        public event Action NextButtonClicked;

        public MatchPuzzleUIPresenter(IMatchPuzzleUIView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public void Initialize()
        {
            // Subscribe to view events
            _view.RestartClicked += OnRestartClicked;
            _view.NextClicked += OnNextClicked;
        }

        public void Dispose()
        {
            _view.RestartClicked -= OnRestartClicked;
            _view.NextClicked -= OnNextClicked;
        }

        private void OnRestartClicked()
        {
            RestartButtonClicked?.Invoke();
        }

        private void OnNextClicked()
        {
            NextButtonClicked?.Invoke();
        }
    }
}
