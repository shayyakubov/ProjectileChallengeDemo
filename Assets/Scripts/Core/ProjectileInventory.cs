using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnimalChallenge.Core
{
    public class ProjectileInventory : MonoBehaviour
    {
        public static ProjectileInventory Instance { get; private set; }

        public event Action InventoryChanged;
        public event Action<string> EquippedProjectileChanged;

        [SerializeField] private string _defaultProjectileId = "Default";

        private readonly HashSet<string> _ownedProjectileIds = new();
        private string _equippedProjectileId;
        private bool _isInitialized;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            GrantDefaultProjectile();
        }

        private void GrantDefaultProjectile()
        {
            if (string.IsNullOrEmpty(_defaultProjectileId))
                return;

            _ownedProjectileIds.Add(_defaultProjectileId);

            if (string.IsNullOrEmpty(_equippedProjectileId))
                _equippedProjectileId = _defaultProjectileId;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public string EquippedProjectileId => _equippedProjectileId;

        public bool Owns(string projectileId)
        {
            EnsureInitialized();
            return !string.IsNullOrEmpty(projectileId) && _ownedProjectileIds.Contains(projectileId);
        }

        public bool IsEquipped(string projectileId)
        {
            EnsureInitialized();
            return !string.IsNullOrEmpty(projectileId) && _equippedProjectileId == projectileId;
        }

        public bool Add(string projectileId)
        {
            EnsureInitialized();
            if (string.IsNullOrEmpty(projectileId) || !_ownedProjectileIds.Add(projectileId))
                return false;

            InventoryChanged?.Invoke();
            return true;
        }

        public bool Equip(string projectileId)
        {
            EnsureInitialized();

            if (!Owns(projectileId) || _equippedProjectileId == projectileId)
                return false;

            _equippedProjectileId = projectileId;
            EquippedProjectileChanged?.Invoke(projectileId);
            return true;
        }

        public IReadOnlyList<string> GetOwnedProjectileIds()
        {
            EnsureInitialized();
            var ids = new List<string>(_ownedProjectileIds);
            ids.Sort(StringComparer.Ordinal);
            return ids;
        }
    }
}
