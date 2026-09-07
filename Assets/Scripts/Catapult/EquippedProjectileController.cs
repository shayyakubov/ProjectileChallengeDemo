using System.Collections;
using AnimalChallenge.Core;
using UnityEngine;

namespace AnimalChallenge.Catapult
{
    public class EquippedProjectileController : MonoBehaviour
    {
        [SerializeField] private ProjectileInventory _inventory;
        [SerializeField] private ProjectileIconCatalog _catalog;
        [SerializeField] private CatapultLauncher _launcher;
        [SerializeField] private CatapultGame _catapultGame;
        [SerializeField] private CameraTargetController _cameraTargetController;

        private void Awake()
        {
            if (_inventory == null)
                _inventory = GetComponent<ProjectileInventory>();
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
            StartCoroutine(ApplyEquippedProjectileNextFrame());
        }

        private IEnumerator ApplyEquippedProjectileNextFrame()
        {
            yield return null;
            ApplyEquippedProjectile();
        }

        private void OnEquippedProjectileChanged(string projectileId)
        {
            ApplyEquippedProjectile();
        }

        private void ApplyEquippedProjectile()
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

            var prefab = prefabObject.GetComponent<Projectile>();
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
    }
}
