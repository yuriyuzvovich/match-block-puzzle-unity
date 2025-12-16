using UnityEngine;

namespace MatchPuzzle.Features.Balloon
{
    public class BalloonView : MonoBehaviour, IBalloonView
    {
        public event System.Action<float> UpdateTick;
        public Transform Transform => transform;

        private void Update()
        {
            UpdateTick?.Invoke(Time.deltaTime);
        }

        public void ResetState()
        {
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
        }

        public void SetScale(float scale)
        {
            transform.localScale = Vector3.one * scale;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }
    }
}
