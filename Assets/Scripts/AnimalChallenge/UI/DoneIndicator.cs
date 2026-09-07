using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AnimalChallenge.UI
{
    public class DoneIndicator : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private Color _doneColor = Color.green;
        [SerializeField] private Color _notDoneColor = Color.black;

        public void Render(bool isDone)
        {
            _image.color = isDone ? _doneColor : _notDoneColor;
        }

        public IEnumerator AnimateToDone(float duration)
        {
            Render(false);

            if (duration <= 0f)
            {
                Render(true);
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _image.color = Color.Lerp(_notDoneColor, _doneColor, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            Render(true);
        }
    }
}
