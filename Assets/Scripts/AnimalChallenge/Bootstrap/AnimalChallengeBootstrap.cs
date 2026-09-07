using AnimalChallenge.Controllers;
using AnimalChallenge.Config;
using AnimalChallenge.Core;
using AnimalChallenge.Data;
using AnimalChallenge.Model;
using AnimalChallenge.Navigation;
using AnimalChallenge.UI;
using UnityEngine;

namespace AnimalChallenge.Bootstrap
{
    public class AnimalChallengeBootstrap : MonoBehaviour
    {
        [SerializeField] private TextAsset _configAsset;
        [SerializeField] private CharacterChallengeView _characterChallengeView;
        [SerializeField] private SceneNavigationController _navigation;
        [SerializeField] private ProjectileInventory _projectileInventory;
        [SerializeField] private ProjectileIconCatalog _iconCatalog;

        private AnimalChallengeManager _manager;
        private bool _isInitialized;

        public ProjectileIconCatalog IconCatalog => _iconCatalog;

        public void Initialize(CurrencyWallet wallet)
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            if (_configAsset == null || _characterChallengeView == null)
            {
                Debug.LogError("AnimalChallengeBootstrap is missing required references.");
                return;
            }

            if (_navigation == null)
                _navigation = GetComponent<SceneNavigationController>();

            if (_projectileInventory == null)
                _projectileInventory = ProjectileInventory.Instance;

            var config = JsonUtility.FromJson<AnimalChallengeConfigData>(_configAsset.text);
            if (config == null)
            {
                Debug.LogError("Failed to parse Animal Challenge config.");
                return;
            }

            var userData = new AnimalChallengeUserData();
            var model = AnimalChallengeModel.Create(config, userData);
            _manager = new AnimalChallengeManager();
            _manager.Initialize(model, _characterChallengeView, wallet, _navigation, _projectileInventory, _iconCatalog);
        }

        private void OnDestroy()
        {
            _manager?.Dispose();
        }
    }
}

