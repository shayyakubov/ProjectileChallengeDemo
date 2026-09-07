using System;
using AnimalChallenge.Core;
using UnityEngine;

namespace AnimalChallenge.Catapult
{
    public sealed class EquippedProjectileController : IDisposable
    {
        private readonly ProjectileInventory _inventory;
        private readonly ProjectileIconCatalog _catalog;
        private readonly CatapultLauncher _launcher;
        private readonly CatapultGame _catapultGame;
        private readonly CameraTargetController _cameraTargetController;

        public EquippedProjectileController(
            ProjectileInventory inventory,
            ProjectileIconCatalog catalog,
            CatapultLauncher launcher,
            CatapultGame catapultGame,
            CameraTargetController cameraTargetController)
        {
            _inventory = inventory;
            _catalog = catalog;
            _launcher = launcher;
            _catapultGame = catapultGame;
            _cameraTargetController = cameraTargetController;

            if (_inventory != null)
                _inventory.EquippedProjectileChanged += OnEquippedProjectileChanged;
        }

        public void Dispose()
        {
            if (_inventory != null)
                _inventory.EquippedProjectileChanged -= OnEquippedProjectileChanged;
        }

        public void ApplyEquippedProjectile()
        {
            if (_inventory == null || _launcher == null || _catalog == null)
                return;

            var projectileId = _inventory.EquippedProjectileId;
            if (string.IsNullOrEmpty(projectileId))
                return;

            var prefabObject = _catalog.GetPrefab(projectileId);
            if (prefabObject == null)
            {
                Debug.LogWarning($"EquippedProjectileController: no prefab mapped for '{projectileId}'.");
                return;
            }

            var prefab = prefabObject.GetComponent<Projectile>()
                         ?? prefabObject.GetComponentInChildren<Projectile>(true);
            if (prefab == null)
            {
                Debug.LogWarning($"EquippedProjectileController: prefab for '{projectileId}' has no Projectile component.");
                return;
            }

            var projectile = _launcher.ReplaceProjectile(prefab);
            if (projectile == null)
                return;

            _catapultGame?.SetProjectile(projectile);
            _cameraTargetController?.SetProjectileTarget(projectile.transform);
        }

        private void OnEquippedProjectileChanged(string projectileId)
        {
            ApplyEquippedProjectile();
        }
    }
}
