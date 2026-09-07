using System;
using System.Collections.Generic;
using AnimalChallenge.Core;
using AnimalChallenge.Model;
using AnimalChallenge.UI;

namespace AnimalChallenge.Controllers
{
    public static class ChallengePageViewModelMapper
    {
        public static ChallengePageViewModel Map(
            AnimalChallengeModel model,
            CurrencyWallet wallet,
            EquipCharacterPanelViewModel equipCharacterPanel,
            int? justPurchasedSubstepIndex = null,
            bool didCompleteStep = false)
        {
            var stepCount = model.StepCount;
            var activeStepIndex = GetActiveStepIndex(model, stepCount);
            var steps = BuildStepViewModels(model, stepCount, activeStepIndex, didCompleteStep);
            var milestones = BuildMilestones(model);
            var activeStepPanel = activeStepIndex >= 0
                ? BuildActiveStepPanel(model, wallet, activeStepIndex)
                : null;
            var usePurchaseSuccessRender = justPurchasedSubstepIndex != null && !didCompleteStep;

            return new ChallengePageViewModel(
                model.DisplayName,
                model.Score,
                steps,
                activeStepIndex,
                activeStepPanel,
                usePurchaseSuccessRender,
                milestones,
                equipCharacterPanel);
        }

        private static List<ChallengeStepViewModel> BuildStepViewModels(
            AnimalChallengeModel model,
            int stepCount,
            int activeStepIndex,
            bool didCompleteStep)
        {
            var steps = new List<ChallengeStepViewModel>(stepCount);
            var completedNowStepIndex = didCompleteStep
                ? activeStepIndex >= 0 ? activeStepIndex - 1 : stepCount - 1
                : -1;

            for (var stepIndex = 0; stepIndex < stepCount; stepIndex++)
            {
                ChallengeStepState state;

                if (stepIndex == completedNowStepIndex)
                    state = ChallengeStepState.CompletedNow;
                else if (IsStepComplete(model, stepIndex))
                    state = ChallengeStepState.CompletedIdle;
                else if (stepIndex == activeStepIndex)
                    state = ChallengeStepState.Active;
                else
                    state = ChallengeStepState.NotCompleted;

                steps.Add(new ChallengeStepViewModel(state));
            }

            return steps;
        }

        private static SubstepPanelViewModel BuildActiveStepPanel(
            AnimalChallengeModel model,
            CurrencyWallet wallet,
            int activeStepIndex)
        {
            var doneStates = new List<bool>();
            int? nextPrice = null;
            var startIndex = model.GetStepSubstepStartIndex(activeStepIndex);
            var count = model.GetStepSubstepCount(activeStepIndex);

            for (var i = startIndex; i < startIndex + count; i++)
            {
                doneStates.Add(model.IsSubstepCompleted(i));

                if (nextPrice == null && !model.IsSubstepCompleted(i))
                    nextPrice = model.Substeps[i].Price;
            }

            var canPurchase = nextPrice != null
                              && wallet != null
                              && wallet.GetBalance("AT") >= nextPrice.Value;

            return new SubstepPanelViewModel(doneStates, nextPrice, canPurchase);
        }

        private static List<MilestoneViewData> BuildMilestones(AnimalChallengeModel model)
        {
            var milestones = new List<MilestoneViewData>(model.Milestones.Count);

            for (var i = 0; i < model.Milestones.Count; i++)
            {
                var milestone = model.Milestones[i];
                var isClaimed = model.IsMilestoneClaimed(i);
                milestones.Add(new MilestoneViewData(
                    milestone.ScoreThreshold,
                    isClaimed,
                    !isClaimed && model.Score >= milestone.ScoreThreshold));
            }

            return milestones;
        }

        private static int GetActiveStepIndex(AnimalChallengeModel model, int stepCount)
        {
            for (var stepIndex = 0; stepIndex < stepCount; stepIndex++)
            {
                if (!IsStepComplete(model, stepIndex))
                    return stepIndex;
            }

            return -1;
        }

        private static bool IsStepComplete(AnimalChallengeModel model, int stepIndex)
        {
            var startIndex = model.GetStepSubstepStartIndex(stepIndex);
            var count = model.GetStepSubstepCount(stepIndex);

            for (var i = startIndex; i < startIndex + count; i++)
            {
                if (!model.IsSubstepCompleted(i))
                    return false;
            }

            return true;
        }
    }
}
