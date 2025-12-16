using System;
using MatchPuzzle.Core.Interfaces;
using UnityEngine;

namespace MatchPuzzle.Infrastructure.Services
{
    /// <summary>
    /// Raises Unity engine lifecycle events so non-MonoBehaviours can subscribe.
    /// Self-drives by owning a hidden MonoBehaviour that relays Unity's Update.
    /// </summary>
    public sealed class UnityEventsService : IUnityEventsService
    {
        public event Action<float> OnUpdate;

        private readonly UnityEventsDriver _driver;

        public UnityEventsService()
        {
            _driver = UnityEventsDriver.Create(this);
        }

        public void PublishUpdate(float deltaTime)
        {
            OnUpdate?.Invoke(deltaTime);
        }

        /// <summary>
        /// Lightweight driver MonoBehaviour that forwards Unity callbacks to the service.
        /// </summary>
        private sealed class UnityEventsDriver : MonoBehaviour
        {
            private UnityEventsService _service;

            public static UnityEventsDriver Create(UnityEventsService service)
            {
                var go = new GameObject("[UnityEventsService]");
                go.hideFlags = HideFlags.HideAndDontSave;
                DontDestroyOnLoad(go);

                var driver = go.AddComponent<UnityEventsDriver>();
                driver._service = service;
                return driver;
            }

            private void Update()
            {
                _service?.PublishUpdate(Time.deltaTime);
            }

            private void OnDestroy()
            {
                _service = null;
            }
        }
    }
}
