using UnityEngine;

namespace AnimalChallenge.UI
{
    public class FadeOutAnimation : MonoBehaviour
    {
        [SerializeField] private bool _deactivateOnComplete = true;

        public void OnFadeOutComplete()
        {
            if (_deactivateOnComplete)
                gameObject.SetActive(false);
        }
    }
}
