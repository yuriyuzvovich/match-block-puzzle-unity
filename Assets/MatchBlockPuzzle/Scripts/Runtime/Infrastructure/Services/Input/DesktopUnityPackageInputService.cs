using System;
using UnityEngine;
using UnityEngine.InputSystem;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Data;

namespace MatchPuzzle.Infrastructure.Services
{
    /// <summary>
    /// Input service for Editor/Desktop using Mouse input.
    /// Uses Unity's new Input System with Mouse.current for desktop testing.
    /// </summary>
    public class DesktopUnityPackageInputService : IInputService
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

        public DesktopUnityPackageInputService(PlayerInputSettings inputSettings)
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
            if (Mouse.current != null)
            {
                _lastPointerPosition = Mouse.current.position.ReadValue();
            }

            return _lastPointerPosition;
        }

        public void DoFrameTick()
        {
            if (!_isEnabled)
                return;

            if (Mouse.current == null)
                return;

            var wasPressed = Mouse.current.leftButton.wasPressedThisFrame;
            var wasReleased = Mouse.current.leftButton.wasReleasedThisFrame;

            if (Mouse.current.leftButton.isPressed || wasPressed || wasReleased)
            {
                _lastPointerPosition = Mouse.current.position.ReadValue();
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
