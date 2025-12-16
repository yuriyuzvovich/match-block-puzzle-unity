using System.Collections.Generic;
using UnityEngine;

namespace MatchPuzzle.Features.Balloon
{
    [CreateAssetMenu(fileName = "BalloonSettings", menuName = "MatchPuzzle/Balloon Settings")]
    public class BalloonSettings : ScriptableObject
    {
        [SerializeField] private bool _isEnabled = true;
        public bool IsEnabled => _isEnabled;

        [Header("Pool Warmup")]
        [Tooltip("How many BalloonView objects to warmup in the pool at startup")]
        [SerializeField] private int _balloonViewWarmupCount = 10;
        public int BalloonViewWarmupCount => _balloonViewWarmupCount;
        
        [Tooltip("Offscreen margin relative to view width (min) for despawning balloons")]
        [SerializeField, Range(-1f, 0f)] private float _offscreenMarginRelativeToViewWidthMin = -0.3f;
        [Tooltip("Offscreen margin relative to view width (max) for despawning balloons")]
        [SerializeField, Range(1f, 2f)] private float _offscreenMarginRelativeToViewWidthMax = 1.3f;

        [SerializeField]
        private List<string> _balloonPrefabKeys = new List<string> { "Prefabs/BalloonView" };

        [Header("Balloon Behavior")]
        [SerializeField] private int _maxBalloons = 3;
        public int MaxBalloons => _maxBalloons;

        [SerializeField] private float _balloonMinSpeed = 0.5f;
        public float BalloonMinSpeed => _balloonMinSpeed;

        [SerializeField] private float _balloonMaxSpeed = 2f;
        public float BalloonMaxSpeed => _balloonMaxSpeed;

        [SerializeField] private float _balloonSineAmplitude = 0.5f;
        public float BalloonSineAmplitude => _balloonSineAmplitude;

        [SerializeField] private float _balloonSineFrequency = 1f;
        public float BalloonSineFrequency => _balloonSineFrequency;

        [Header("Spawn Position")]
        [SerializeField] private float _spawnZCoordinate = 10f;
        public float SpawnZCoordinate => _spawnZCoordinate;

        [SerializeField, Range(0f, 1f)] private float _spawnHeightMinNormalized = 0.2f;
        public float SpawnHeightMinNormalized => _spawnHeightMinNormalized;

        [SerializeField, Range(0f, 1f)] private float _spawnHeightMaxNormalized = 0.8f;
        public float SpawnHeightMaxNormalized => _spawnHeightMaxNormalized;

        [Tooltip("Minimum X offset multiplier relative to camera width (for spawn position)")]
        [SerializeField, Range(0f, 1f)] private float _spawnXOffsetMinRelativeToWidth = 0.1f;
        public float SpawnXOffsetMinRelativeToWidth => _spawnXOffsetMinRelativeToWidth;

        [Tooltip("Maximum X offset multiplier relative to camera width (for spawn position)")]
        [SerializeField, Range(0f, 1f)] private float _spawnXOffsetMaxRelativeToWidth = 0.3f;
        public float SpawnXOffsetMaxRelativeToWidth => _spawnXOffsetMaxRelativeToWidth;

        [Header("Balloon Size")]
        [Tooltip("Minimum size multiplier relative to camera orthographic size")]
        [SerializeField, Range(0.01f, 1f)] private float _sizeMinRelativeToCamera = 0.05f;
        public float SizeMinRelativeToCamera => _sizeMinRelativeToCamera;

        [Tooltip("Maximum size multiplier relative to camera orthographic size")]
        [SerializeField, Range(0.01f, 1f)] private float _sizeMaxRelativeToCamera = 0.15f;
        public float SizeMaxRelativeToCamera => _sizeMaxRelativeToCamera;
        
        public float OffscreenMarginRelativeToViewWidthMin => _offscreenMarginRelativeToViewWidthMin;
        public float OffscreenMarginRelativeToViewWidthMax => _offscreenMarginRelativeToViewWidthMax;

        public string GetRandomPrefabKey()
        {
            if (_balloonPrefabKeys == null || _balloonPrefabKeys.Count == 0)
            {
                throw new System.InvalidOperationException("Balloon prefab keys list is empty.");
            }

            int randomIndex = Random.Range(0, _balloonPrefabKeys.Count);
            return _balloonPrefabKeys[randomIndex];
        }
    }
}
