using UnityEngine;

namespace MatchPuzzle.Features.Balloon
{
    public class BalloonPresenter : IBalloonPresenter
    {
        private readonly IBalloonView _view;
        private float _speed;
        private float _direction;
        private float _sineAmplitude;
        private float _sineFrequency;
        private float _time;
        private Vector3 _startPosition;

        public BalloonPresenter(IBalloonView view)
        {
            _view = view;
            _view.UpdateTick += OnUpdateTick;
        }

        public void Initialize(
            Vector3 startPosition,
            float scale,
            float speed,
            float direction,
            float sineAmplitude,
            float sineFrequency
        )
        {
            _startPosition = startPosition;
            _speed = speed;
            _direction = direction;
            _sineAmplitude = sineAmplitude;
            _sineFrequency = sineFrequency;
            _time = 0f;
            _view.SetPosition(startPosition);
            _view.SetScale(scale);
        }

        private void OnUpdateTick(float dt)
        {
            DoUpdate(dt);
        }

        private void DoUpdate(float deltaTime)
        {
            _time += deltaTime;

            var horizontalOffset = _direction * _speed * _time;
            var verticalOffset = Mathf.Sin(_time * _sineFrequency) * _sineAmplitude;
            _view.SetPosition(_startPosition + new Vector3(horizontalOffset, verticalOffset, 0));
        }

        public bool IsOffScreen(Camera camera, float leftMargin, float rightMargin)
        {
            var viewportPos = camera.WorldToViewportPoint(_view.Transform.position);
            if (viewportPos.z < 0f)
                return true;

            // Horizontal checks with a small margin
            if (_direction > 0)
            {
                return viewportPos.x > rightMargin;
            }
            else
            {
                return viewportPos.x < leftMargin;
            }
        }

        public void Reset()
        {
            _speed = 0f;
            _direction = 0f;
            _sineAmplitude = 0f;
            _sineFrequency = 0f;
            _time = 0f;
            _startPosition = Vector3.zero;
            _view.ResetState();
            _view.UpdateTick -= OnUpdateTick;
        }
    }
}