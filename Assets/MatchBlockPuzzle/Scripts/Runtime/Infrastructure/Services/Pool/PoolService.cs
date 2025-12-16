using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using MatchPuzzle.Core.Interfaces;

namespace MatchPuzzle.Infrastructure.Services
{
    /// <summary>
    /// Manages object pools to reduce instantiation overhead.
    /// Uses IAssetService to load prefabs by string keys.
    /// </summary>
    public class PoolService : IPoolService
    {
        private readonly IAssetService _assetService;
        private readonly Dictionary<string, GameObject> _prefabs;
        private readonly Dictionary<string, Queue<GameObject>> _pools;
        private readonly Dictionary<GameObject, string> _instanceToKey;
        private readonly Dictionary<string, UniTask<GameObject>> _prefabLoadTasks;
        private readonly Transform _poolRoot;
        private readonly ILoggerService _logger;

        public PoolService(IAssetService assetService, ILoggerService logger)
        {
            _assetService = assetService;
            _logger = logger;
            _prefabs = new Dictionary<string, GameObject>();
            _pools = new Dictionary<string, Queue<GameObject>>();
            _instanceToKey = new Dictionary<GameObject, string>();
            _prefabLoadTasks = new Dictionary<string, UniTask<GameObject>>();

            // Create a root object to hold all pooled instances
            _poolRoot = new GameObject("[Pool Root]").transform;
            Object.DontDestroyOnLoad(_poolRoot.gameObject);
        }

        public async UniTask<T> GetAsync<T>(string key, Transform parent = null) where T: Component
        {
            GameObject instance = await GetAsync(key, parent);
            if (!instance)
                return null;

            T component = instance.GetComponent<T>();

            if (!component)
            {
                _logger.LogError($"[PoolService] Prefab at key '{key}' does not have component of type {typeof(T).Name}");
            }

            return component;
        }

        public async UniTask<GameObject> GetAsync(string key, Transform parent = null)
        {
            var prefab = await LoadPrefabAsync(key);
            if (!prefab)
                return null;

            // Get from pool or create new
            GameObject instance;
            if (_pools.ContainsKey(key) && _pools[key].Count > 0)
            {
                instance = _pools[key].Dequeue();
                instance.SetActive(true);
            }
            else
            {
                instance = Object.Instantiate(prefab);
                _instanceToKey[instance] = key;
            }

            if (parent)
            {
                instance.transform.SetParent(parent, false);
            }

            return instance;
        }

        public void Return(string key, GameObject instance)
        {
            if (!instance)
            {
                _logger.LogWarning($"[PoolService] Attempted to return null instance for key: {key}");
                return;
            }

            // Ensure pool exists
            if (!_pools.ContainsKey(key))
            {
                _pools[key] = new Queue<GameObject>();
            }

            // Reset objects that implement IPoolObject (no reflection)
            var poolObjects = instance.GetComponentsInChildren<IPoolObject>(true);
            if (poolObjects != null && poolObjects.Length > 0)
            {
                foreach (var obj in poolObjects)
                {
                    try
                    {
                        obj.ResetState();
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError($"[PoolService] Exception while resetting pool object: {ex}");
                    }
                }
            }

            // Deactivate and move to pool root
            instance.SetActive(false);
            instance.transform.SetParent(_poolRoot, false);

            // Add to pool
            _pools[key].Enqueue(instance);
        }

        public void Return<T>(string key, T instance) where T: Component
        {
            if (!instance)
            {
                _logger.LogWarning($"[PoolService] Attempted to return null instance for key: {key}");
                return;
            }

            Return(key, instance.gameObject);
        }

        public async UniTask WarmupAsync(string key, int count)
        {
            var prefab = await LoadPrefabAsync(key);
            if (!prefab)
                return;

            if (!_pools.ContainsKey(key))
            {
                _pools[key] = new Queue<GameObject>();
            }

            // Pre-instantiate instances
            for (int i = 0; i < count; i++)
            {
                GameObject instance = Object.Instantiate(prefab, _poolRoot);
                instance.SetActive(false);
                _pools[key].Enqueue(instance);
                _instanceToKey[instance] = key;
            }

            _logger.LogInformation($"[PoolService] Warmed up {count} instances for key: {key}");
        }

        public void Clear(string key)
        {
            if (!_pools.ContainsKey(key))
            {
                return;
            }

            // Destroy all instances in the pool
            while (_pools[key].Count > 0)
            {
                GameObject instance = _pools[key].Dequeue();
                if (instance)
                {
                    _instanceToKey.Remove(instance);
                    Object.Destroy(instance);
                }
            }

            _pools.Remove(key);
            _logger.LogInformation($"[PoolService] Cleared pool for key: {key}");
        }

        public void ClearAll()
        {
            // Destroy all pooled instances
            foreach (var pool in _pools.Values)
            {
                while (pool.Count > 0)
                {
                    GameObject instance = pool.Dequeue();
                    if (instance)
                    {
                        Object.Destroy(instance);
                    }
                }
            }

            _pools.Clear();
            _instanceToKey.Clear();
            _prefabs.Clear();
            _prefabLoadTasks.Clear();

            _logger.LogInformation("[PoolService] Cleared all pools");
        }

        private async UniTask<GameObject> LoadPrefabAsync(string key)
        {
            if (_prefabs.TryGetValue(key, out var prefab))
            {
                return prefab;
            }

            if (_prefabLoadTasks.TryGetValue(key, out var existingTask))
            {
                return await existingTask;
            }

            var loadTask = _assetService.LoadAsync<GameObject>(key);
            _prefabLoadTasks[key] = loadTask;

            prefab = await loadTask;
            _prefabLoadTasks.Remove(key);

            if (!prefab)
            {
                _logger.LogError($"[PoolService] Failed to load prefab at key: {key}");
                return null;
            }

            _prefabs[key] = prefab;
            _logger.LogInformation($"[PoolService] Loaded prefab: {key}");
            return prefab;
        }
    }
}
