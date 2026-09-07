using System;
using UnityEngine;

namespace AnimalChallenge.Core
{
    [CreateAssetMenu(fileName = "ProjectileIconCatalog", menuName = "Animal Challenge/Projectile Icon Catalog")]
    public class ProjectileIconCatalog : ScriptableObject
    {
        [Serializable]
        public struct IconEntry
        {
            public string projectileId;
            public Sprite icon;
            public GameObject prefab;
        }

        [SerializeField] private IconEntry[] _icons;

        public Sprite GetIcon(string projectileId)
        {
            if (string.IsNullOrEmpty(projectileId) || _icons == null)
                return null;

            foreach (var entry in _icons)
            {
                if (entry.projectileId == projectileId)
                    return entry.icon;
            }

            return null;
        }

        public GameObject GetPrefab(string projectileId)
        {
            if (string.IsNullOrEmpty(projectileId) || _icons == null)
                return null;

            foreach (var entry in _icons)
            {
                if (entry.projectileId == projectileId)
                    return entry.prefab;
            }

            return null;
        }
    }
}
