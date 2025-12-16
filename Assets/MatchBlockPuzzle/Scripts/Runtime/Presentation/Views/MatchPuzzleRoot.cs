using MatchPuzzle.Core.Interfaces;
using UnityEngine;

namespace MatchPuzzle.Runtime.Presentation
{
    public class MatchPuzzleRoot : MonoBehaviour, IMatchPuzzleRoot
    {
        private Transform _transform;
        private GameObject _gameObject;

        public Transform ThisTransform => _transform ??= this.transform;
        public GameObject ThisGameObject => _gameObject ??= this.gameObject;

        public void Show()
        {
            ThisGameObject.SetActive(true);
        }

        public void Hide()
        {
            ThisGameObject.SetActive(false);
        }

        public void Cleanup()
        {
        }
    }
}