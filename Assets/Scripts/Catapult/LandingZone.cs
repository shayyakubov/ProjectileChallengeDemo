using System.Collections.Generic;
using UnityEngine;

namespace AnimalChallenge.Catapult
{
    public class LandingZone : MonoBehaviour
    {
        [SerializeField] private string _currencyId = "AT";
        [SerializeField] private int _amount = 5;
        [SerializeField] private BoxCollider _zoneCollider;

        public string CurrencyId => _currencyId;
        public int Amount => _amount;

        private void Reset()
        {
            _zoneCollider = GetComponent<BoxCollider>();
        }

        public bool Contains(Vector3 worldPoint)
        {
            if (_zoneCollider == null)
                return false;

            var bounds = _zoneCollider.bounds;
            bounds.Expand(new Vector3(0f, 5f, 0f));
            return bounds.Contains(worldPoint);
        }

        public Dictionary<string, int> GetReward()
        {
            return new Dictionary<string, int> { { _currencyId, _amount } };
        }
    }
}
