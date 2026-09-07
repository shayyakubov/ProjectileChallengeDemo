using System.Collections.Generic;
using UnityEngine;

namespace AnimalChallenge.Catapult
{
    public class LandingField : MonoBehaviour
    {
        [SerializeField] private LandingZone[] _zones;

        public bool TryResolve(Vector3 worldPoint, out Dictionary<string, int> reward)
        {
            reward = null;

            if (_zones == null)
                return false;

            foreach (var zone in _zones)
            {
                if (zone == null || !zone.Contains(worldPoint))
                    continue;

                reward = zone.GetReward();
                return true;
            }

            return false;
        }
    }
}
