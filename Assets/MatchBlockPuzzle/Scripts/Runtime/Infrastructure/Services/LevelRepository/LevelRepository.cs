using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Domain;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Data;
using Newtonsoft.Json;
using UnityEngine;

namespace MatchPuzzle.Infrastructure.Services.LevelRepository
{
    /// <summary>
    /// Lazy-loading repository for level data stored as JSON TextAssets.
    /// Keeps a small cache and unloads evicted TextAssets to reduce memory pressure.
    /// </summary>
    public class LevelRepository : ILevelRepository
    {
        private string _levelConfigurationKey;

        private readonly IAssetService _assetService;
        private readonly ILoggerService _logger;
        private readonly AssetKeys _assetKeys;

        // LRU (Least Recently Used) cache eviction strategy
        // ensures frequently accessed levels stay cached while infrequently used ones get evicted first, reducing memory pressure
        private int _cacheSize = 3;
        private readonly Dictionary<int, Level> _cache = new Dictionary<int, Level>();
        private readonly LinkedList<int> _levelsLinkedList = new LinkedList<int>();

        private LevelConfiguration _config;

        public int LevelCount => _config?.LevelCount ?? 0;

        public LevelRepository(IAssetService assetService, ILoggerService logger, AssetKeys assetKeys)
        {
            _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _assetKeys = assetKeys ?? throw new ArgumentNullException(nameof(assetKeys));
        }

        public async UniTask InitializeAsync()
        {
            // Load LevelRepositorySettings via asset keys
            try
            {
                var settings = await _assetService.LoadAsync<LevelRepositorySettings>(_assetKeys.LevelRepositorySettingsKey);
                if (settings)
                {
                    _levelConfigurationKey = settings.LevelConfigurationKey;
                    _cacheSize = Math.Max(1, settings.CacheSize);
                    _assetService.Unload(settings);
                }
                else
                {
                    _logger?.LogError("LevelRepositorySettings not found! Using defaults.");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to load LevelRepositorySettings: {ex.Message}");
            }

            _config = await _assetService.LoadAsync<LevelConfiguration>(_levelConfigurationKey);

            if (!_config)
            {
                _logger?.LogError("LevelConfiguration not found!");
                _config = ScriptableObject.CreateInstance<LevelConfiguration>();
            }
        }

        public async UniTask<Level> LoadLevelAsync(int levelNumber)
        {
            if (!_config)
            {
                await InitializeAsync();
            }

            if (_cache.TryGetValue(levelNumber, out var cached))
            {
                Touch(levelNumber);
                return cached;
            }

            var metadata = _config.GetMetadata(levelNumber);
            if (metadata == null)
            {
                _logger?.LogError($"Level metadata for level {levelNumber} not found.");
                return null;
            }

            if (string.IsNullOrEmpty(metadata.ResourcePath))
            {
                metadata.ResourcePath = $"Levels/Data/level_{metadata.LevelNumber:D4}";
            }

            var textAsset = await _assetService.LoadAsync<TextAsset>(metadata.ResourcePath);
            if (!textAsset)
            {
                _logger?.LogError($"Level file not found at {metadata.ResourcePath}");
                return null;
            }

            Level level = null;
            try
            {
                level = JsonConvert.DeserializeObject<Level>(textAsset.text);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to parse level {levelNumber}: {ex.Message}");
            }
            finally
            {
                _assetService.Unload(textAsset);
            }

            if (level != null)
            {
                AddToCache(levelNumber, level);
            }

            return level;
        }

        public void ClearCache()
        {
            _cache.Clear();
            _levelsLinkedList.Clear();
        }

        private void AddToCache(int levelNumber, Level level)
        {
            _cache[levelNumber] = level;
            Touch(levelNumber);

            if (_cache.Count <= _cacheSize)
            {
                return;
            }

            // Evict the oldest level from the cache
            var oldest = _levelsLinkedList.First?.Value ?? levelNumber;
            _levelsLinkedList.RemoveFirst();
            _cache.Remove(oldest);
        }

        private void Touch(int levelNumber)
        {
            _levelsLinkedList.Remove(levelNumber);
            _levelsLinkedList.AddLast(levelNumber);
        }
    }
}