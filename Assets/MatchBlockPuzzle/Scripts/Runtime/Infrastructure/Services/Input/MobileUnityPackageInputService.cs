using System;
using UnityEngine;
using UnityEngine.InputSystem;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Data;

namespace MatchPuzzle.Infrastructure.Services
{
    /// <summary>
    /// Input service for mobile devices using Touch input.
    /// Uses Unity's new Input System with Touchscreen.current for mobile devices.
    /// </summary>
    public class MobileUnityPackageInputService : IInputService
    {
        public event Action<SwipeData> OnSwipe;
        public event Action<Vector2> OnTap;

        private readonly PlayerInputSettings _inputSettings;
        private Vector2 _startPosition;
        private Vector2 _lastPointerPosition;
        private bool _isDragging;
        private bool _isEnabled;

        public PlayerInputSettings Settings => _inputSettings;

        private float MinSwipeDistance => Mathf.Max(_inputSettings.MinSwipeDistance);

        private bool TriggerTapOnShortSwipe => _inputSettings.TriggerTapOnShortSwipe;

        public MobileUnityPackageInputService(PlayerInputSettings inputSettings)
        {
            _inputSettings = inputSettings;
        }

        public void Enable()
        {
            _isEnabled = true;
        }

        public void Disable()
        {
            _isEnabled = false;
            _isDragging = false;
        }

        public Vector2 GetPointerPosition()
        {
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.isPressed)
                {
                    _lastPointerPosition = touch.position.ReadValue();
                }
            }

            return _lastPointerPosition;
        }

        public void DoFrameTick()
        {
            if (!_isEnabled)
                return;

            if (Touchscreen.current == null)
                return;

            var touch = Touchscreen.current.primaryTouch;
            var wasPressed = touch.press.wasPressedThisFrame;
            var wasReleased = touch.press.wasReleasedThisFrame;

            if (touch.press.isPressed || wasPressed || wasReleased)
            {
                _lastPointerPosition = touch.position.ReadValue();
            }

            if (wasPressed)
            {
                _startPosition = _lastPointerPosition;
                _isDragging = true;
            }
            else if (wasReleased && _isDragging)
            {
                var endPosition = _lastPointerPosition;
                _isDragging = false;

                var swipeData = new SwipeData {
                    StartPosition = _startPosition,
                    EndPosition = endPosition
                };

                if (swipeData.Distance >= MinSwipeDistance)
                {
                    OnSwipe?.Invoke(swipeData);
                }
                else if (TriggerTapOnShortSwipe)
                {
                    OnTap?.Invoke(_startPosition);
                }
            }
        }
    }
}
