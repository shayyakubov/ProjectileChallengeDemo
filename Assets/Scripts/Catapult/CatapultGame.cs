using System;
using AnimalChallenge.Core;
using UnityEngine;

namespace AnimalChallenge.Catapult
{
    public class CatapultGame : MonoBehaviour
    {
        public event Action ShotMissed;

        [SerializeField] private Projectile _projectile;
        [SerializeField] private CatapultLauncher _launcher;
        [SerializeField] private LandingField _landingField;
        [SerializeField] private CurrencyWallet _wallet;

        private void OnEnable()
        {
            if (_projectile != null)
                _projectile.Landed += HandleProjectileLanded;
        }

        private void OnDisable()
        {
            if (_projectile != null)
                _projectile.Landed -= HandleProjectileLanded;
        }

        public void SetProjectile(Projectile projectile)
        {
            if (_projectile == projectile)
                return;

            if (_projectile != null)
                _projectile.Landed -= HandleProjectileLanded;

            _projectile = projectile;

            if (_projectile != null && isActiveAndEnabled)
                _projectile.Landed += HandleProjectileLanded;
        }

        private void HandleProjectileLanded(Vector3 position)
        {
            if (_landingField != null && _landingField.TryResolve(position, out var reward))
                _wallet?.Grant(reward);
            else
                ShotMissed?.Invoke();

            _launcher?.ScheduleReset();
        }
    }
}
