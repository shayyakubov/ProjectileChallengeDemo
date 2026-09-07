using System;

namespace AnimalChallenge.Data
{
    [Serializable]
    public class AnimalChallengeUserData
    {
        public int Score;
        public bool[] CompletedSubsteps = Array.Empty<bool>();
        public bool[] ClaimedMilestones = Array.Empty<bool>();

        public void EnsureSubstepCapacity(int substepCount)
        {
            if (CompletedSubsteps.Length == substepCount)
                return;

            var completed = new bool[substepCount];
            for (var i = 0; i < CompletedSubsteps.Length && i < substepCount; i++)
                completed[i] = CompletedSubsteps[i];

            CompletedSubsteps = completed;
        }

        public void EnsureMilestoneCapacity(int milestoneCount)
        {
            if (ClaimedMilestones.Length == milestoneCount)
                return;

            var claimed = new bool[milestoneCount];
            for (var i = 0; i < ClaimedMilestones.Length && i < milestoneCount; i++)
                claimed[i] = ClaimedMilestones[i];

            ClaimedMilestones = claimed;
        }
    }
}
