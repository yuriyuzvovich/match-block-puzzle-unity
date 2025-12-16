using System;
using MatchPuzzle.Core.Interfaces;
using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Domain;

namespace MatchPuzzle.Infrastructure.Services
{
    public class PersistenceService : IPersistenceService
    {
        private readonly IPersistentStorage _storage;
        private readonly IJsonParser _parser;
        private readonly ILoggerService _logger;
        private readonly string _gameStateKey;
        private readonly bool _canParseOffThread;

        internal PersistenceService(
            string gameStateKey,
            IPersistentStorage storage,
            IJsonParser parser,
            ILoggerService logger
        )
        {
            _gameStateKey = gameStateKey;
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            // Only Newtonsoft is safe to use on a background thread; Unity's JsonUtility is not.
            _canParseOffThread = parser is NewtonsoftJsonParser;
        }

        public async UniTask SaveGameStateAsync(GameStateProfileData stateProfileData)
        {
            if (stateProfileData == null)
                throw new ArgumentNullException(nameof(stateProfileData));

            var json = _canParseOffThread
                ? await UniTask.Run(() => _parser.Serialize(stateProfileData))
                : _parser.Serialize(stateProfileData);

            _storage.SetString(_gameStateKey, json);
            _storage.Save();
            _logger?.LogInformation($"[PersistenceService] Saved game state JSON length: {json.Length}");

            return;
        }

        public async UniTask<GameStateProfileData> LoadGameStateAsync()
        {
            if (!HasSavedState())
            {
                return new GameStateProfileData();
            }

            var json = _storage.GetString(_gameStateKey);
            var jsonLength = string.IsNullOrEmpty(json) ? 0 : json.Length;
            _logger?.LogInformation($"[PersistenceService] Loaded game state JSON length: {jsonLength}");

            if (string.IsNullOrEmpty(json))
            {
                return new GameStateProfileData();
            }

            var data = _canParseOffThread
                ? await UniTask.Run(() => _parser.Deserialize<GameStateProfileData>(json))
                : _parser.Deserialize<GameStateProfileData>(json);

            return data ?? new GameStateProfileData();
        }

        public bool HasSavedState()
        {
            return _storage.HasKey(_gameStateKey);
        }

        public void ClearSavedState()
        {
            _storage.DeleteKey(_gameStateKey);
            _storage.Save();
            _logger.LogInformation("[PersistenceService] Cleared saved game state.");
        }
    }
}
