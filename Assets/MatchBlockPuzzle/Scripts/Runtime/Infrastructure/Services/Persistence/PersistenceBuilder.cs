using System;
using MatchPuzzle.Core.Interfaces;

namespace MatchPuzzle.Infrastructure.Services
{
    public sealed class PersistenceBuilder
    {
        private IPersistentStorage _storage;
        private IJsonParser _parser = new NewtonsoftJsonParser();
        private ILoggerService _logger;
        private string _gameStateKey;

        public PersistenceBuilder()
        {
        }

        public PersistenceBuilder WithStorage(IPersistentStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            return this;
        }

        public PersistenceBuilder WithParser(IJsonParser parser)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            return this;
        }

        public PersistenceBuilder WithLogger(ILoggerService logger)
        {
            _logger = logger;
            return this;
        }

        public PersistenceBuilder WithGameStateKey(string gameStateKey)
        {
            _gameStateKey = gameStateKey ?? throw new ArgumentNullException(nameof(gameStateKey));
            return this;
        }

        public PersistenceService Build()
        {
            if (_storage == null)
            {
                throw new InvalidOperationException("Persistent storage must be provided.");
            }

            return new PersistenceService(
                _gameStateKey,
                _storage,
                _parser,
                _logger
            );
        }
    }
}