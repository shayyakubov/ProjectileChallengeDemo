using System;
using UnityEngine;
using UnityEngine.UI;

namespace AnimalChallenge.UI
{
    public class EquipCharacterPanel : MonoBehaviour
    {
        [SerializeField] private Button _equipButton;
        [SerializeField] private GameObject _equippedLabel;

        public event Action EquipClicked;

        private void Awake()
        {
            if (_equipButton == null)
                _equipButton = GetComponentInChildren<Button>(true);

            if (_equippedLabel == null)
            {
                var labelTransform = transform.Find("EquippedLabel");
                if (labelTransform != null)
                    _equippedLabel = labelTransform.gameObject;
            }

            if (_equipButton != null)
                _equipButton.onClick.AddListener(OnEquipButtonClicked);
        }

        private void OnDestroy()
        {
            if (_equipButton != null)
                _equipButton.onClick.RemoveListener(OnEquipButtonClicked);
        }

        public void Render(EquipCharacterPanelViewModel viewModel)
        {
            if (viewModel == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(viewModel.ShowPanel);

            if (_equipButton != null)
                _equipButton.gameObject.SetActive(!viewModel.IsEquipped);

            if (_equippedLabel != null)
                _equippedLabel.SetActive(viewModel.IsEquipped);
        }

        private void OnEquipButtonClicked()
        {
            EquipClicked?.Invoke();
        }
    }
}
