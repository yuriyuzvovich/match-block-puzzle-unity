using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Interfaces;
using System;
using MatchPuzzle.ApplicationLayer;

namespace MatchPuzzle.ApplicationLayerLayer.Commands
{
    /// <summary>
    /// Command to restart the current level
    /// </summary>
    public class RestartLevelCommand : ICommand
    {
        private readonly GameStateManager _gameState;
        private readonly IGlobalEventBus _eventBus;
        private readonly Action _onLevelRestarted;

        public RestartLevelCommand(
            GameStateManager gameState,
            IGlobalEventBus eventBus,
            Action onLevelRestarted
        )
        {
            _gameState = gameState;
            _eventBus = eventBus;
            _onLevelRestarted = onLevelRestarted;
        }

        public bool CanExecute()
        {
            return _gameState.CurrentLevel != null;
        }

        public async UniTask ExecuteAsync()
        {
            // Wait for any ongoing normalization
            while (_gameState.IsNormalizing)
            {
                await UniTask.Yield();
            }

            // Re-initialize the same level
            var level = _gameState.CurrentLevel;
            _gameState.InitializeLevel(level);

            // Notify view layer
            _onLevelRestarted?.Invoke();

            // Publish event
            _eventBus?.Publish(new LevelStartedEvent(level.LevelNumber));

            await UniTask.Yield();
        }
    }
}