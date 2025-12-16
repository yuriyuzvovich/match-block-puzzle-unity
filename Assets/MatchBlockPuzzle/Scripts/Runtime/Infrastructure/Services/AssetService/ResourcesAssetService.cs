using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Interfaces;
using UnityEngine;

namespace MatchPuzzle.Infrastructure.Services
{
    /// <summary>
    /// Asset service implementation using Unity Resources
    /// </summary>
    public class ResourcesAssetService : IAssetService
    {
        public async UniTask<T> LoadAsync<T>(string path) where T : Object
        {
            // Simulate async load (Resources is synchronous, but we wrap it)
            await UniTask.Yield();
            return Resources.Load<T>(path);
        }

        public void Unload(Object asset)
        {
            if (asset)
            {
                Resources.UnloadAsset(asset);
            }
        }
    }
}
