using System;
using System.Collections.Generic;

namespace AnimalChallenge.Core
{
    public class CurrencyWallet
    {
        public event Action<IReadOnlyDictionary<string, int>> BalanceChanged;

        private readonly Dictionary<string, int> _balances = new Dictionary<string, int>();

        public int GetBalance(string currencyId)
        {
            return _balances.TryGetValue(currencyId, out var balance) ? balance : 0;
        }

        public void Grant(IReadOnlyDictionary<string, int> amounts)
        {
            if (amounts == null || amounts.Count == 0)
                return;

            var granted = new Dictionary<string, int>();

            foreach (var entry in amounts)
            {
                if (string.IsNullOrEmpty(entry.Key) || entry.Value <= 0)
                    continue;

                _balances.TryGetValue(entry.Key, out var current);
                _balances[entry.Key] = current + entry.Value;
                granted[entry.Key] = entry.Value;
            }

            if (granted.Count > 0)
                BalanceChanged?.Invoke(granted);
        }

        public bool TrySpend(string currencyId, int amount)
        {
            if (amount <= 0 || GetBalance(currencyId) < amount)
                return false;

            _balances[currencyId] -= amount;
            BalanceChanged?.Invoke(new Dictionary<string, int> { { currencyId, -amount } });
            return true;
        }
    }
}
