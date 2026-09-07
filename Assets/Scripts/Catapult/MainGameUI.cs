using System.Collections.Generic;
using AnimalChallenge.Core;
using TMPro;
using UnityEngine;

namespace AnimalChallenge.Catapult
{
    public class MainGameUI : MonoBehaviour
    {
        [SerializeField] private CurrencyWallet _wallet;
        [SerializeField] private CatapultGame _shotController;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private TMP_Text _atText;
        [SerializeField] private TMP_Text _cnText;

        private void OnEnable()
        {
            if (_wallet == null)
                _wallet = CurrencyWallet.Instance;

            if (_wallet != null)
                _wallet.BalanceChanged += HandleBalanceChanged;

            if (_shotController != null)
                _shotController.ShotMissed += HandleShotMissed;

            RefreshBalances();
            SetStatus("Press and drag down on the catapult to bend the arm.");
        }

        private void OnDisable()
        {
            if (_wallet != null)
                _wallet.BalanceChanged -= HandleBalanceChanged;

            if (_shotController != null)
                _shotController.ShotMissed -= HandleShotMissed;
        }

        private void HandleShotMissed()
        {
            SetStatus("Missed the zones. No reward.");
        }

        private void HandleBalanceChanged(IReadOnlyDictionary<string, int> changes)
        {
            RefreshBalances();

            if (changes == null || changes.Count == 0)
                return;

            var status = "Landed in zone:";
            var hasGrant = false;

            foreach (var entry in changes)
            {
                if (entry.Value <= 0)
                    continue;

                status += $" +{entry.Value} {entry.Key}";
                hasGrant = true;
            }

            if (hasGrant)
                SetStatus(status);
        }

        private void RefreshBalances()
        {
            if (_wallet == null)
                return;

            if (_atText != null)
                _atText.text = $"AT: {_wallet.GetBalance("AT")}";

            if (_cnText != null)
                _cnText.text = $"CN: {_wallet.GetBalance("CN")}";
        }

        public void SetStatus(string message)
        {
            if (_statusText != null)
                _statusText.text = message;
        }
    }
}
