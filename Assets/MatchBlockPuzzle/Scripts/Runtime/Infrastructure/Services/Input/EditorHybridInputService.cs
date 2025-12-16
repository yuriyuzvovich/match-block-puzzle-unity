#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Data;

namespace MatchPuzzle.Infrastructure.Services
{
    /// <summary>
    /// Input service for Unity Editor that supports both Game view (mouse) and Device Simulator (touch).
    /// Automatically detects and uses simulated touch input when the Device Simulator is active.
    /// </summary>
    public class EditorHybridInputService : IInputService
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

        public EditorHybridInputService(PlayerInputSettings inputSettings)
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
            // Prefer simulated touch (Device Simulator) over mouse
            if (IsSimulatedTouchActive())
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.isPressed)
                {
                    _lastPointerPosition = touch.position.ReadValue();
                    return _lastPointerPosition;
                }
            }

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

            var pointer = ReadPointerState();

            if (pointer.WasPressed)
            {
                _startPosition = pointer.Position;
                _isDragging = true;
            }
            else if (pointer.WasReleased && _isDragging)
            {
                var endPosition = pointer.Position;
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

        private PointerState ReadPointerState()
        {
            var pointer = new PointerState {
                Position = _lastPointerPosition,
                WasPressed = false,
                WasReleased = false
            };

            // Check for simulated touch first (Device Simulator active)
            if (IsSimulatedTouchActive())
            {
                var touch = Touchscreen.current.primaryTouch;
                pointer.WasPressed = touch.press.wasPressedThisFrame;
                pointer.WasReleased = touch.press.wasReleasedThisFrame;

                if (touch.press.isPressed || pointer.WasPressed || pointer.WasReleased)
                {
                    pointer.Position = touch.position.ReadValue();
                    _lastPointerPosition = pointer.Position;
                    return pointer;
                }
            }

            // Fall back to mouse input (regular Game view)
            if (Mouse.current != null)
            {
                pointer.WasPressed = Mouse.current.leftButton.wasPressedThisFrame;
                pointer.WasReleased = Mouse.current.leftButton.wasReleasedThisFrame;

                if (Mouse.current.leftButton.isPressed || pointer.WasPressed || pointer.WasReleased)
                {
                    pointer.Position = Mouse.current.position.ReadValue();
                    _lastPointerPosition = pointer.Position;
                }
            }

            return pointer;
        }

        /// <summary>
        /// Checks if simulated touch input is active (Device Simulator is being used).
        /// </summary>
        private static bool IsSimulatedTouchActive()
        {
            if (Touchscreen.current == null)
                return false;

            // In the Device Simulator, touch is simulated and the device description
            // will indicate it's a simulated device
            var touch = Touchscreen.current.primaryTouch;
            return touch.press.isPressed || 
                touch.press.wasPressedThisFrame || 
                touch.press.wasReleasedThisFrame;
        }

        private struct PointerState
        {
            public Vector2 Position;
            public bool WasPressed;
            public bool WasReleased;
        }
    }
}

#endif