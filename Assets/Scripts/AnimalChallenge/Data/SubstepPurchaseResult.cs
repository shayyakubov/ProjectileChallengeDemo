using System;
using System.Collections.Generic;

namespace AnimalChallenge.Model
{
    public enum SubstepPurchaseStatus
    {
        Succeeded,
        NoSubstepAvailable,
        InsufficientFunds
    }

    public sealed class SubstepPurchaseResult
    {
        public SubstepPurchaseStatus Status { get; }
        public int PurchasedSubstepIndex { get; }
        public int SpentAmount { get; }
        public bool DidCompleteStep { get; }
        public IReadOnlyList<MilestoneDefinition> ClaimedMilestones { get; }

        public bool Succeeded => Status == SubstepPurchaseStatus.Succeeded;

        private SubstepPurchaseResult(
            SubstepPurchaseStatus status,
            int purchasedSubstepIndex,
            int spentAmount,
            bool didCompleteStep,
            IReadOnlyList<MilestoneDefinition> claimedMilestones)
        {
            Status = status;
            PurchasedSubstepIndex = purchasedSubstepIndex;
            SpentAmount = spentAmount;
            DidCompleteStep = didCompleteStep;
            ClaimedMilestones = claimedMilestones ?? Array.Empty<MilestoneDefinition>();
        }

        public static SubstepPurchaseResult NoSubstepAvailable()
        {
            return new SubstepPurchaseResult(SubstepPurchaseStatus.NoSubstepAvailable, -1, 0, false, null);
        }

        public static SubstepPurchaseResult InsufficientFunds()
        {
            return new SubstepPurchaseResult(SubstepPurchaseStatus.InsufficientFunds, -1, 0, false, null);
        }

        public static SubstepPurchaseResult Success(
            int purchasedSubstepIndex,
            int spentAmount,
            bool didCompleteStep,
            IReadOnlyList<MilestoneDefinition> claimedMilestones)
        {
            return new SubstepPurchaseResult(
                SubstepPurchaseStatus.Succeeded,
                purchasedSubstepIndex,
                spentAmount,
                didCompleteStep,
                claimedMilestones);
        }
    }
}
