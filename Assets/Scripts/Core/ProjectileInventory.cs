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

        private readonly HashSet<string> _ownedProjectileIds = new();
        private string _equippedProjectileId;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public string EquippedProjectileId => _equippedProjectileId;

        public bool Owns(string projectileId)
        {
            return !string.IsNullOrEmpty(projectileId) && _ownedProjectileIds.Contains(projectileId);
        }

        public bool IsEquipped(string projectileId)
        {
            return !string.IsNullOrEmpty(projectileId) && _equippedProjectileId == projectileId;
        }

        public bool Add(string projectileId)
        {
            if (string.IsNullOrEmpty(projectileId) || !_ownedProjectileIds.Add(projectileId))
                return false;

            InventoryChanged?.Invoke();
            return true;
        }

        public bool Equip(string projectileId)
        {
            if (!Owns(projectileId) || _equippedProjectileId == projectileId)
                return false;

            _equippedProjectileId = projectileId;
            EquippedProjectileChanged?.Invoke(projectileId);
            return true;
        }
    }
}
