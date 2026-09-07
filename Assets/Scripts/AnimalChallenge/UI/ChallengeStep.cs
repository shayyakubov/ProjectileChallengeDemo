using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace AnimalChallenge.UI
{
    public enum ChallengeStepState
    {
        NotCompleted,
        Active,
        CompletedIdle,
        CompletedNow
    }

    public sealed class ChallengeStepViewModel
    {
        public ChallengeStepState State { get; }

        public ChallengeStepViewModel(ChallengeStepState state)
        {
            State = state;
        }
    }

    public class ChallengeStep : MonoBehaviour
    {
        [SerializeField] private UnityEvent _completedIdle;
        [SerializeField] private UnityEvent _completedNow;
        [SerializeField] private UnityEvent _notCompleted;
        [SerializeField] private UnityEvent _active;
        [SerializeField] private Image _image;
        [SerializeField] private CanvasGroup _canvasGroup;

        private void Reset()
        {
            _image = GetComponent<Image>();
            _canvasGroup = GetComponent<CanvasGroup>();
            DisableRaycastBlocking();
        }

        private void Awake()
        {
            DisableRaycastBlocking();
        }

        public void Render(ChallengeStepViewModel viewModel)
        {
            if (viewModel == null)
                return;

            switch (viewModel.State)
            {
                case ChallengeStepState.CompletedIdle:
                    _completedIdle.Invoke();
                    break;
                case ChallengeStepState.CompletedNow:
                    _completedNow.Invoke();
                    break;
                case ChallengeStepState.NotCompleted:
                    _notCompleted.Invoke();
                    break;
                case ChallengeStepState.Active:
                    _active.Invoke();
                    break;
            }
        }

        private void DisableRaycastBlocking()
        {
            if (_image != null)
                _image.raycastTarget = false;

            if (_canvasGroup != null)
                _canvasGroup.blocksRaycasts = false;
        }
    }
}
