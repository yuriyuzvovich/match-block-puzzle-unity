using UnityEngine;

namespace MatchPuzzle.Features.Background
{
    /// <summary>
    /// Simple background view. Presentation logic handled by BackgroundPresenter.
    /// </summary>
    public class BackgroundView : MonoBehaviour, IBackgroundView
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Transform _groundLine;

        public SpriteRenderer SpriteRenderer => _spriteRenderer;
        public Transform GroundLine => _groundLine;
        public Transform RootTransform => transform;

        private void Awake()
        {
            if (!_spriteRenderer) throw new System.Exception("BackgroundView: SpriteRenderer is not assigned!");
            if (!_groundLine) throw new System.Exception("BackgroundView: GroundLine Transform is not assigned!");

            gameObject.SetActive(false);
        }

        public void SetActive(bool isActive) => gameObject.SetActive(isActive);

        public void SetScale(Vector3 scale) => transform.localScale = scale;

        public void SetPosition(Vector3 position) => transform.position = position;

        public void Move(Vector3 delta) => transform.position += delta;
    }
}