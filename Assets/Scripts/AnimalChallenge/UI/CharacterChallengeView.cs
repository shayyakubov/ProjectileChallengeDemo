using System;
using UnityEngine;

namespace AnimalChallenge.UI
{
    public class CharacterChallengeView : MonoBehaviour, IChallengePageView
    {
        [SerializeField] private CharacterPage[] _characterPages;
        [SerializeField] private CharacterSelection _characterSelection;

        public event Action PurchaseNextClicked;
        public event Action<string> EquipCharacterClicked;
        public event Action<string> CharacterSelected;

        private void Awake()
        {
            if (_characterPages == null || _characterPages.Length == 0)
                _characterPages = GetComponentsInChildren<CharacterPage>(true);

            if (_characterSelection == null)
                _characterSelection = GetComponentInChildren<CharacterSelection>(true);

            foreach (var page in _characterPages)
            {
                if (page == null)
                    continue;

                page.PurchaseNextClicked += OnPagePurchaseNextClicked;
                page.EquipCharacterClicked += OnPageEquipCharacterClicked;
            }

            if (_characterSelection != null)
                _characterSelection.IconSelected += OnCharacterSelectionIconSelected;
        }

        private void OnDestroy()
        {
            if (_characterPages != null)
            {
                foreach (var page in _characterPages)
                {
                    if (page == null)
                        continue;

                    page.PurchaseNextClicked -= OnPagePurchaseNextClicked;
                    page.EquipCharacterClicked -= OnPageEquipCharacterClicked;
                }
            }

            if (_characterSelection != null)
                _characterSelection.IconSelected -= OnCharacterSelectionIconSelected;
        }

        public void Render(ChallengePageViewModel viewModel)
        {
            if (viewModel == null)
                return;

            if (_characterSelection != null)
                _characterSelection.Render(viewModel.CharacterSelection);

            if (_characterPages == null)
                return;

            for (var i = 0; i < _characterPages.Length; i++)
            {
                var page = _characterPages[i];
                if (page == null)
                    continue;

                var pageViewModel = i < viewModel.Pages.Count
                    ? viewModel.Pages[i]
                    : null;

                page.Render(pageViewModel);
            }
        }

        private void OnPagePurchaseNextClicked()
        {
            PurchaseNextClicked?.Invoke();
        }

        private void OnPageEquipCharacterClicked(string projectileId)
        {
            EquipCharacterClicked?.Invoke(projectileId);
        }

        private void OnCharacterSelectionIconSelected(string projectileId)
        {
            CharacterSelected?.Invoke(projectileId);
        }
    }
}
