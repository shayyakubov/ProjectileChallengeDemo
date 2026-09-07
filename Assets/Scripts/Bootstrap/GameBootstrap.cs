using System.Collections;
using AnimalChallenge.Catapult;
using AnimalChallenge.Core;
using UnityEngine;

namespace AnimalChallenge.Bootstrap
{
    [DefaultExecutionOrder(-100)]
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private ProjectileInventory _projectileInventory;
        [SerializeField] private ProjectileIconCatalog _projectileIconCatalog;
        [SerializeField] private CatapultLauncher _catapultLauncher;
        [SerializeField] private CameraTargetController _cameraTargetController;
        [SerializeField] private AnimalChallengeBootstrap _animalChallengeBootstrap;
        [SerializeField] private CatapultGame _catapultGame;
        [SerializeField] private MainGameUI _mainGameUI;

        private CurrencyWallet _wallet;
        private EquippedProjectileController _equippedProjectileController;

        private void Awake()
        {
            _wallet = new CurrencyWallet();

            if (_projectileInventory == null)
                _projectileInventory = GetComponent<ProjectileInventory>();

            _projectileInventory?.EnsureInitialized();

            if (_animalChallengeBootstrap == null)
                _animalChallengeBootstrap = GetComponent<AnimalChallengeBootstrap>();

            if (_catapultGame == null)
                _catapultGame = FindAnyObjectByType<CatapultGame>();

            if (_catapultLauncher == null)
                _catapultLauncher = FindAnyObjectByType<CatapultLauncher>();

            if (_cameraTargetController == null)
                _cameraTargetController = FindAnyObjectByType<CameraTargetController>();

            if (_mainGameUI == null)
                _mainGameUI = FindAnyObjectByType<MainGameUI>();

            _catapultGame?.Initialize(_wallet);
            _mainGameUI?.Initialize(_wallet);

            _animalChallengeBootstrap?.Initialize(_wallet);

            var iconCatalog = _projectileIconCatalog ?? _animalChallengeBootstrap?.IconCatalog;
            if (_projectileInventory != null && iconCatalog != null && _catapultLauncher != null)
            {
                _equippedProjectileController = new EquippedProjectileController(
                    _projectileInventory,
                    iconCatalog,
                    _catapultLauncher,
                    _catapultGame,
                    _cameraTargetController);
            }
            else
            {
                Debug.LogError("GameBootstrap is missing references required for projectile equip.");
            }
        }

        private void Start()
        {
            StartCoroutine(ApplyEquippedProjectileNextFrame());
        }

        private IEnumerator ApplyEquippedProjectileNextFrame()
        {
            yield return null;
            _equippedProjectileController?.ApplyEquippedProjectile();
        }

        private void OnDestroy()
        {
            _equippedProjectileController?.Dispose();
        }
    }
}
