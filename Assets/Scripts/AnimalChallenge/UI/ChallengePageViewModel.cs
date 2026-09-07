using System.Collections.Generic;

namespace AnimalChallenge.UI
{
    public sealed class ChallengePageViewModel
    {
        public string Title { get; }
        public int Score { get; }
        public IReadOnlyList<ChallengeStepViewModel> Steps { get; }
        public int ActiveStepIndex { get; }
        public SubstepPanelViewModel ActiveStepPanel { get; }
        public bool UsePurchaseSuccessRender { get; }
        public IReadOnlyList<MilestoneViewData> Milestones { get; }
        public EquipCharacterPanelViewModel EquipCharacterPanel { get; }

        public ChallengePageViewModel(
            string title,
            int score,
            IReadOnlyList<ChallengeStepViewModel> steps,
            int activeStepIndex,
            SubstepPanelViewModel activeStepPanel,
            bool usePurchaseSuccessRender,
            IReadOnlyList<MilestoneViewData> milestones,
            EquipCharacterPanelViewModel equipCharacterPanel)
        {
            Title = title;
            Score = score;
            Steps = steps ?? System.Array.Empty<ChallengeStepViewModel>();
            ActiveStepIndex = activeStepIndex;
            ActiveStepPanel = activeStepPanel;
            UsePurchaseSuccessRender = usePurchaseSuccessRender;
            Milestones = milestones ?? System.Array.Empty<MilestoneViewData>();
            EquipCharacterPanel = equipCharacterPanel;
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
