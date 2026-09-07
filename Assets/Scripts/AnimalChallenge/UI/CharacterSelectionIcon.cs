using System;
using UnityEngine;
using UnityEngine.UI;

namespace AnimalChallenge.UI
{
    public class CharacterSelectionIcon : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _iconImage;
        [SerializeField] private GameObject _selectedIndicator;

        public event Action<CharacterSelectionIconViewModel> Clicked;

        private CharacterSelectionIconViewModel _viewModel;

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            if (_iconImage == null)
                _iconImage = GetComponent<Image>();

            if (_button != null)
                _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnButtonClicked);
        }

        public void Render(CharacterSelectionIconViewModel viewModel)
        {
            _viewModel = viewModel;

            if (viewModel == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (_iconImage != null)
            {
                if (viewModel.Icon != null)
                    _iconImage.sprite = viewModel.Icon;

                _iconImage.color = Color.white;
            }

            if (_selectedIndicator != null)
                _selectedIndicator.SetActive(viewModel.IsSelected);

            if (_button != null)
                _button.interactable = true;
        }

        private void OnButtonClicked()
        {
            if (_viewModel != null)
                Clicked?.Invoke(_viewModel);
        }
    }
}
