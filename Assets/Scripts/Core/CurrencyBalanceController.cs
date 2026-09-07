using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnimalChallenge.Core
{
    public class CurrencyBalanceController : MonoBehaviour
    {
        [SerializeField] private CurrencyWallet _wallet;
        [SerializeField] private Text _atText;
        [SerializeField] private Text _cnText;

        private void OnEnable()
        {
            if (_wallet == null)
                _wallet = CurrencyWallet.Instance;

            if (_wallet != null)
                _wallet.BalanceChanged += OnBalanceChanged;

            Refresh();
        }

        private void OnDisable()
        {
            if (_wallet != null)
                _wallet.BalanceChanged -= OnBalanceChanged;
        }

        private void OnBalanceChanged(IReadOnlyDictionary<string, int> changes)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_wallet == null)
                return;

            if (_atText != null)
                _atText.text = $"AT: {_wallet.GetBalance("AT")}";

            if (_cnText != null)
                _cnText.text = $"CN: {_wallet.GetBalance("CN")}";
        }
    }
}
