using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Domain;
using MatchPuzzle.Core.Interfaces;
using System;
using MatchPuzzle.ApplicationLayer;

namespace MatchPuzzle.ApplicationLayerLayer.Commands
{
    /// <summary>
    /// Command to start a level
    /// </summary>
    public class SwitchLevelCommand : ICommand
    {
        private readonly GameStateManager _gameState;
        private readonly Level _level;
        private readonly IGlobalEventBus _eventBus;
        private readonly Action _onLevelSwitched;

        public SwitchLevelCommand(
            GameStateManager gameState,
            Level level,
            IGlobalEventBus eventBus,
            Action onLevelSwitched
        )
        {
            _gameState = gameState;
            _level = level;
            _eventBus = eventBus;
            _onLevelSwitched = onLevelSwitched;
        }

        public bool CanExecute()
        {
            return _level != null;
        }

        public async UniTask ExecuteAsync()
        {
            // Wait for any ongoing normalization
            while (_gameState.IsNormalizing)
            {
                await UniTask.Yield();
            }

            // Initialize level
            _gameState.InitializeLevel(_level);

            // Notify view layer
            _onLevelSwitched?.Invoke();

            // Publish event
            _eventBus?.Publish(new LevelStartedEvent(_level.LevelNumber));

            await UniTask.Yield();
        }
    }
}