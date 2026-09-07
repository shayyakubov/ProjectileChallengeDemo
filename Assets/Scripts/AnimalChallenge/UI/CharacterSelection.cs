using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnimalChallenge.UI
{
    public class CharacterSelection : MonoBehaviour
    {
        [SerializeField] private Transform _iconContainer;
        [SerializeField] private CharacterSelectionIcon _iconPrefab;

        public event Action<string> IconSelected;

        private readonly List<CharacterSelectionIcon> _activeIcons = new();

        private void Awake()
        {
            if (_iconContainer == null)
            {
                var layoutGroup = GetComponentInChildren<UnityEngine.UI.HorizontalLayoutGroup>(true);
                if (layoutGroup != null)
                    _iconContainer = layoutGroup.transform;
            }
        }

        public void Render(CharacterSelectionViewModel viewModel)
        {
            if (viewModel == null || viewModel.Icons.Count == 0)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            EnsureIconCount(viewModel.Icons.Count);

            for (var i = 0; i < viewModel.Icons.Count; i++)
                _activeIcons[i].Render(viewModel.Icons[i]);
        }

        private void EnsureIconCount(int count)
        {
            while (_activeIcons.Count < count)
            {
                if (_iconPrefab == null || _iconContainer == null)
                    return;

                var instance = Instantiate(_iconPrefab, _iconContainer);
                instance.Clicked += OnIconClicked;
                _activeIcons.Add(instance);
            }

            for (var i = 0; i < _activeIcons.Count; i++)
                _activeIcons[i].gameObject.SetActive(i < count);
        }

        private void OnDestroy()
        {
            foreach (var icon in _activeIcons)
            {
                if (icon != null)
                    icon.Clicked -= OnIconClicked;
            }
        }

        private void OnIconClicked(CharacterSelectionIconViewModel viewModel)
        {
            if (viewModel == null || string.IsNullOrEmpty(viewModel.ProjectileId))
                return;

            IconSelected?.Invoke(viewModel.ProjectileId);
        }
    }
}
