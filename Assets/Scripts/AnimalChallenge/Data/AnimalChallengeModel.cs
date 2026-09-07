using System;
using System.Collections.Generic;
using AnimalChallenge.Config;
using AnimalChallenge.Data;

namespace AnimalChallenge.Model
{
    public class AnimalChallengeModel
    {
        private readonly AnimalChallengeConfigData _config;
        private readonly AnimalChallengeUserData _userData;
        private readonly SubstepDefinition[] _substeps;
        private readonly MilestoneDefinition[] _milestones;

        public string Id => _config.id;
        public string DisplayName => _config.displayName;
        public string ChallengeAddressableKey => _config.challengeAddressableKey;
        public int Score => _userData.Score;
        public int StepCount => _config.steps?.Length ?? 0;
        public IReadOnlyList<SubstepDefinition> Substeps => _substeps;
        public IReadOnlyList<MilestoneDefinition> Milestones => _milestones;

        private AnimalChallengeModel(
            AnimalChallengeConfigData config,
            AnimalChallengeUserData userData,
            SubstepDefinition[] substeps,
            MilestoneDefinition[] milestones)
        {
            _config = config;
            _userData = userData;
            _substeps = substeps;
            _milestones = milestones;
        }

        public static AnimalChallengeModel Create(AnimalChallengeConfigData config, AnimalChallengeUserData userData = null)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            userData ??= new AnimalChallengeUserData();
            userData.EnsureSubstepCapacity(CountSubsteps(config));
            userData.EnsureMilestoneCapacity(config.milestones?.Length ?? 0);

            var substeps = BuildSubsteps(config);
            var milestones = BuildMilestones(config);
            return new AnimalChallengeModel(config, userData, substeps, milestones);
        }

        public int GetStepSubstepStartIndex(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= StepCount)
                return -1;

            var start = 0;
            for (var i = 0; i < stepIndex; i++)
                start += GetStepSubstepCount(i);

            return start;
        }

        public int GetStepSubstepCount(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= StepCount)
                return 0;

            return _config.steps[stepIndex].substeps?.Length ?? 0;
        }

        public int GetStepIndexForFlatSubstep(int flatIndex)
        {
            if (flatIndex < 0 || flatIndex >= _substeps.Length)
                return -1;

            var index = 0;
            for (var stepIndex = 0; stepIndex < StepCount; stepIndex++)
            {
                var count = GetStepSubstepCount(stepIndex);
                if (flatIndex < index + count)
                    return stepIndex;

                index += count;
            }

            return -1;
        }

        public bool IsSubstepCompleted(int flatIndex)
        {
            return flatIndex >= 0 && flatIndex < _userData.CompletedSubsteps.Length && _userData.CompletedSubsteps[flatIndex];
        }

        public bool IsMilestoneClaimed(int milestoneIndex)
        {
            return milestoneIndex >= 0
                   && milestoneIndex < _userData.ClaimedMilestones.Length
                   && _userData.ClaimedMilestones[milestoneIndex];
        }

        public int? GetNextAvailableSubstepIndex()
        {
            for (var i = 0; i < _substeps.Length; i++)
            {
                if (!IsSubstepCompleted(i))
                    return i;
            }

            return null;
        }

        public bool IsChallengeComplete => GetNextAvailableSubstepIndex() == null && _substeps.Length > 0;

        public string RewardProjectileId => _config.rewardProjectileId;

        public SubstepPurchaseResult TryPurchaseNextSubstep(int availableAt)
        {
            var nextIndex = GetNextAvailableSubstepIndex();
            if (nextIndex == null)
                return SubstepPurchaseResult.NoSubstepAvailable();

            return TryPurchaseSubstep(nextIndex.Value, availableAt);
        }

        public void MarkSubstepCompleted(int flatIndex)
        {
            if (flatIndex < 0 || flatIndex >= _substeps.Length || IsSubstepCompleted(flatIndex))
                return;

            _userData.CompletedSubsteps[flatIndex] = true;
            _userData.Score += _substeps[flatIndex].Price;
        }

        public void MarkMilestoneClaimed(int milestoneIndex)
        {
            if (milestoneIndex < 0 || milestoneIndex >= _milestones.Length)
                return;

            _userData.ClaimedMilestones[milestoneIndex] = true;
        }

        public AnimalChallengeUserData ToUserData() => _userData;

        private SubstepPurchaseResult TryPurchaseSubstep(int flatIndex, int availableAt)
        {
            var nextIndex = GetNextAvailableSubstepIndex();
            if (nextIndex == null || nextIndex.Value != flatIndex)
                return SubstepPurchaseResult.NoSubstepAvailable();

            var price = _substeps[flatIndex].Price;
            if (availableAt < price)
                return SubstepPurchaseResult.InsufficientFunds();

            MarkSubstepCompleted(flatIndex);
            var claimedMilestones = ClaimEligibleMilestones();
            var didCompleteStep = IsStepComplete(GetStepIndexForFlatSubstep(flatIndex));
            return SubstepPurchaseResult.Success(flatIndex, price, didCompleteStep, claimedMilestones);
        }

        private bool IsStepComplete(int stepIndex)
        {
            if (stepIndex < 0)
                return false;

            var startIndex = GetStepSubstepStartIndex(stepIndex);
            var count = GetStepSubstepCount(stepIndex);

            for (var i = startIndex; i < startIndex + count; i++)
            {
                if (!IsSubstepCompleted(i))
                    return false;
            }

            return true;
        }

        private List<MilestoneDefinition> ClaimEligibleMilestones()
        {
            var claimed = new List<MilestoneDefinition>();

            for (var i = 0; i < _milestones.Length; i++)
            {
                var milestone = _milestones[i];
                if (_userData.Score < milestone.ScoreThreshold || IsMilestoneClaimed(i))
                    continue;

                MarkMilestoneClaimed(i);
                claimed.Add(milestone);
            }

            return claimed;
        }

        private static SubstepDefinition[] BuildSubsteps(AnimalChallengeConfigData config)
        {
            var substeps = new List<SubstepDefinition>();
            var steps = config.steps ?? Array.Empty<StepConfigData>();

            foreach (var step in steps)
            {
                var stepSubsteps = step.substeps ?? Array.Empty<SubstepConfigData>();
                foreach (var substep in stepSubsteps)
                    substeps.Add(new SubstepDefinition(substep.price));
            }

            return substeps.ToArray();
        }

        private static MilestoneDefinition[] BuildMilestones(AnimalChallengeConfigData config)
        {
            var milestones = config.milestones ?? Array.Empty<MilestoneConfigData>();
            var result = new MilestoneDefinition[milestones.Length];

            for (var i = 0; i < milestones.Length; i++)
            {
                var milestone = milestones[i];
                result[i] = new MilestoneDefinition(milestone.score, milestone.rewards ?? Array.Empty<RewardConfigData>());
            }

            return result;
        }

        private static int CountSubsteps(AnimalChallengeConfigData config)
        {
            var count = 0;
            var steps = config.steps ?? Array.Empty<StepConfigData>();

            foreach (var step in steps)
                count += step.substeps?.Length ?? 0;

            return count;
        }
    }

    public readonly struct SubstepDefinition
    {
        public int Price { get; }

        public SubstepDefinition(int price)
        {
            Price = price;
        }
    }

    public readonly struct MilestoneDefinition
    {
        public int ScoreThreshold { get; }
        public RewardConfigData[] Rewards { get; }

        public MilestoneDefinition(int scoreThreshold, RewardConfigData[] rewards)
        {
            ScoreThreshold = scoreThreshold;
            Rewards = rewards;
        }
    }
}
