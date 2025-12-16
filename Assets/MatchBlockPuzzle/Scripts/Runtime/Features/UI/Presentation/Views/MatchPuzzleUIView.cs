using System;
using UnityEngine;
using UnityEngine.UI;

namespace MatchPuzzle.Features.UI
{
    public class MatchPuzzleUIView : MonoBehaviour, IMatchPuzzleUIView
    {
        [Header("UI Elements")]
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Canvas _canvas;

        public event Action RestartClicked;
        public event Action NextClicked;
        public Canvas Canvas => _canvas;

        private void Awake()
        {
            if (!_canvas) throw new Exception("[MatchPuzzleUIView] Canvas reference is missing!");
            if (!_restartButton) throw new Exception("[MatchPuzzleUIView] Restart button reference is missing!");
            if (!_nextButton) throw new Exception("[MatchPuzzleUIView] Next button reference is missing!");

            _restartButton.onClick.AddListener(HandleRestartClicked);
            _nextButton.onClick.AddListener(HandleNextClicked);
        }

        void OnDestroy()
        {
            _restartButton.onClick.RemoveListener(HandleRestartClicked);
            _nextButton.onClick.RemoveListener(HandleNextClicked);
        }

        private void HandleRestartClicked()
        {
            RestartClicked?.Invoke();
        }

        private void HandleNextClicked()
        {
            NextClicked?.Invoke();
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
