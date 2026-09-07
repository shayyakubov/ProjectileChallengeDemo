using System;
using AnimalChallenge.Core;
using UnityEngine;

namespace AnimalChallenge.Catapult
{
    public class EquippedProjectileController : MonoBehaviour
    {
        [Serializable]
        private struct ProjectilePrefabEntry
        {
            public string id;
            public Projectile prefab;
        }

        [SerializeField] private ProjectileInventory _inventory;
        [SerializeField] private CatapultLauncher _launcher;
        [SerializeField] private CatapultGame _catapultGame;
        [SerializeField] private CameraTargetController _cameraTargetController;
        [SerializeField] private ProjectilePrefabEntry[] _projectiles;

        private void Awake()
        {
            if (_inventory == null)
                _inventory = ProjectileInventory.Instance;
        }

        private void OnEnable()
        {
            if (_inventory != null)
                _inventory.EquippedProjectileChanged += OnEquippedProjectileChanged;
        }

        private void OnDisable()
        {
            if (_inventory != null)
                _inventory.EquippedProjectileChanged -= OnEquippedProjectileChanged;
        }

        private void Start()
        {
            ApplyEquippedProjectile();
        }

        private void OnEquippedProjectileChanged(string projectileId)
        {
            ApplyEquippedProjectile();
        }

        private void ApplyEquippedProjectile()
        {
            if (_inventory == null || _launcher == null)
                return;

            var projectileId = _inventory.EquippedProjectileId;
            if (string.IsNullOrEmpty(projectileId))
                return;

            var prefab = GetPrefab(projectileId);
            if (prefab == null)
                return;

            var projectile = _launcher.ReplaceProjectile(prefab);
            if (projectile == null)
                return;

            _catapultGame?.SetProjectile(projectile);
            _cameraTargetController?.SetProjectileTarget(projectile.transform);
        }

        private Projectile GetPrefab(string projectileId)
        {
            if (_projectiles == null)
                return null;

            foreach (var entry in _projectiles)
            {
                if (entry.id == projectileId)
                    return entry.prefab;
            }

            return null;
        }
    }
}
