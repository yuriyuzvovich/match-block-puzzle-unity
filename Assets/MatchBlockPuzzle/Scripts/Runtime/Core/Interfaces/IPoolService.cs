using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MatchPuzzle.Core.Interfaces
{
    /// <summary>
    /// Service for managing object pools to reduce instantiation overhead.
    /// Uses IAssetService to load prefabs by string keys.
    /// </summary>
    public interface IPoolService
    {
        /// <summary>
        /// Asynchronously gets an instance from the pool or creates a new one if the pool is empty.
        /// Automatically loads the prefab using IAssetService if not already loaded.
        /// </summary>
        /// <param name="key">The asset key to load the prefab from.</param>
        /// <param name="parent">Optional parent transform for the instantiated object.</param>
        /// <returns>A GameObject instance from the pool.</returns>
        UniTask<GameObject> GetAsync(string key, Transform parent = null);

        /// <summary>
        /// Returns an instance to the pool for reuse.
        /// </summary>
        /// <param name="key">The asset key that was used to create this instance.</param>
        /// <param name="instance">The GameObject instance to return to the pool.</param>
        void Return(string key, GameObject instance);

        /// <summary>
        /// Asynchronously gets a typed component from the pool or creates a new one if the pool is empty.
        /// Automatically loads the prefab using IAssetService if not already loaded.
        /// </summary>
        /// <typeparam name="T">The component type to retrieve.</typeparam>
        /// <param name="key">The asset key to load the prefab from.</param>
        /// <param name="parent">Optional parent transform for the instantiated object.</param>
        /// <returns>A component instance from the pool.</returns>
        UniTask<T> GetAsync<T>(string key, Transform parent = null) where T : Component;

        /// <summary>
        /// Returns a component instance to the pool for reuse.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="key">The asset key that was used to create this instance.</param>
        /// <param name="instance">The component instance to return to the pool.</param>
        void Return<T>(string key, T instance) where T : Component;

        /// <summary>
        /// Asynchronously preloads and pre-instantiates a number of instances for a given key.
        /// </summary>
        /// <param name="key">The asset key to preload.</param>
        /// <param name="count">Number of instances to pre-instantiate.</param>
        UniTask WarmupAsync(string key, int count);

        /// <summary>
        /// Clears all instances from the pool for a given key.
        /// </summary>
        /// <param name="key">The asset key to clear.</param>
        void Clear(string key);

        /// <summary>
        /// Clears all pools.
        /// </summary>
        void ClearAll();
    }
}
