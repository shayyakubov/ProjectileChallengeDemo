using System;

namespace AnimalChallenge.Config
{
    [Serializable]
    public class AnimalChallengeConfigData
    {
        public string id;
        public string displayName;
        public string challengeAddressableKey;
        public string rewardProjectileId;
        public StepConfigData[] steps;
        public MilestoneConfigData[] milestones;
    }

    [Serializable]
    public class StepConfigData
    {
        public SubstepConfigData[] substeps;
    }

    [Serializable]
    public class SubstepConfigData
    {
        public int price;
    }

    [Serializable]
    public class MilestoneConfigData
    {
        public int score;
        public RewardConfigData[] rewards;
    }

    [Serializable]
    public class RewardConfigData
    {
        public CurrencyRewardConfigData currency;
        public string rewardId;
    }

    [Serializable]
    public class CurrencyRewardConfigData
    {
        public int AT;
        public int CN;
    }
}
