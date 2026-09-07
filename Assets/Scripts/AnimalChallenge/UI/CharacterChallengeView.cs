using System;
using System.Collections.Generic;
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

            var pagesByProjectileId = BuildPageLookup();

            foreach (var pageViewModel in viewModel.Pages)
            {
                if (pageViewModel == null || string.IsNullOrEmpty(pageViewModel.ProjectileId))
                    continue;

                if (!pagesByProjectileId.TryGetValue(pageViewModel.ProjectileId, out var page))
                    continue;

                page.Render(pageViewModel);
            }
        }

        private Dictionary<string, CharacterPage> BuildPageLookup()
        {
            var lookup = new Dictionary<string, CharacterPage>();

            if (_characterPages == null)
                return lookup;

            foreach (var page in _characterPages)
            {
                if (page == null || string.IsNullOrEmpty(page.ProjectileId))
                    continue;

                lookup[page.ProjectileId] = page;
            }

            return lookup;
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
