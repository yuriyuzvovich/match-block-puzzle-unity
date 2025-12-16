using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MatchPuzzle.Core.Interfaces
{
    /// <summary>
    /// Service for loading assets (prefabs, ScriptableObjects, etc.)
    /// Abstraction to support Resources, Addressables, etc.
    /// </summary>
    public interface IAssetService
    {
        UniTask<T> LoadAsync<T>(string path) where T : Object;
        void Unload(Object asset);
    }
}
