using System.Collections.Generic;
using UnityEngine;

namespace AnimalChallenge.UI
{
    public sealed class ChallengePageViewModel
    {
        public string SelectedProjectileId { get; }
        public CharacterSelectionViewModel CharacterSelection { get; }
        public IReadOnlyList<CharacterPageViewModel> Pages { get; }

        public ChallengePageViewModel(
            string selectedProjectileId,
            CharacterSelectionViewModel characterSelection,
            IReadOnlyList<CharacterPageViewModel> pages)
        {
            SelectedProjectileId = selectedProjectileId;
            CharacterSelection = characterSelection;
            Pages = pages ?? System.Array.Empty<CharacterPageViewModel>();
        }
    }

    public sealed class CharacterPageViewModel
    {
        public string ProjectileId { get; }
        public bool IsVisible { get; }
        public string Title { get; }
        public bool ShowChallengeArt { get; }
        public IReadOnlyList<ChallengeStepViewModel> Steps { get; }
        public int ActiveStepIndex { get; }
        public SubstepPanelViewModel ActiveStepPanel { get; }
        public bool UsePurchaseSuccessRender { get; }
        public EquipCharacterPanelViewModel EquipCharacterPanel { get; }

        public CharacterPageViewModel(
            string projectileId,
            bool isVisible,
            string title,
            bool showChallengeArt,
            IReadOnlyList<ChallengeStepViewModel> steps,
            int activeStepIndex,
            SubstepPanelViewModel activeStepPanel,
            bool usePurchaseSuccessRender,
            EquipCharacterPanelViewModel equipCharacterPanel)
        {
            ProjectileId = projectileId;
            IsVisible = isVisible;
            Title = title;
            ShowChallengeArt = showChallengeArt;
            Steps = steps ?? System.Array.Empty<ChallengeStepViewModel>();
            ActiveStepIndex = activeStepIndex;
            ActiveStepPanel = activeStepPanel;
            UsePurchaseSuccessRender = usePurchaseSuccessRender;
            EquipCharacterPanel = equipCharacterPanel;
        }
    }

    public sealed class CharacterSelectionViewModel
    {
        public IReadOnlyList<CharacterSelectionIconViewModel> Icons { get; }

        public CharacterSelectionViewModel(IReadOnlyList<CharacterSelectionIconViewModel> icons)
        {
            Icons = icons ?? System.Array.Empty<CharacterSelectionIconViewModel>();
        }
    }

    public sealed class CharacterSelectionIconViewModel
    {
        public string ProjectileId { get; }
        public Sprite Icon { get; }
        public bool IsOwned { get; }
        public bool IsEquipped { get; }
        public bool IsSelected { get; }

        public CharacterSelectionIconViewModel(
            string projectileId,
            Sprite icon,
            bool isOwned,
            bool isEquipped,
            bool isSelected)
        {
            ProjectileId = projectileId;
            Icon = icon;
            IsOwned = isOwned;
            IsEquipped = isEquipped;
            IsSelected = isSelected;
        }
    }

    public sealed class EquipCharacterPanelViewModel
    {
        public bool ShowPanel { get; }
        public bool IsEquipped { get; }

        public EquipCharacterPanelViewModel(bool showPanel, bool isEquipped)
        {
            ShowPanel = showPanel;
            IsEquipped = isEquipped;
        }
    }

    public readonly struct SubstepViewData
    {
        public int StepIndex { get; }
        public int SubstepIndex { get; }
        public int Price { get; }
        public bool IsCompleted { get; }

        public SubstepViewData(int stepIndex, int substepIndex, int price, bool isCompleted)
        {
            StepIndex = stepIndex;
            SubstepIndex = substepIndex;
            Price = price;
            IsCompleted = isCompleted;
        }
    }

    public readonly struct MilestoneViewData
    {
        public int ScoreThreshold { get; }
        public bool IsClaimed { get; }
        public bool IsReady { get; }

        public MilestoneViewData(int scoreThreshold, bool isClaimed, bool isReady)
        {
            ScoreThreshold = scoreThreshold;
            IsClaimed = isClaimed;
            IsReady = isReady;
        }
    }
}
