using System;
using System.Collections.Generic;
using AnimalChallenge.Core;
using AnimalChallenge.Model;
using AnimalChallenge.Navigation;
using AnimalChallenge.UI;
using UnityEngine;

namespace AnimalChallenge.Controllers
{
    public sealed class AnimalChallengeManager : IDisposable
    {
        private const string DefaultProjectileId = "Default";

        public event Action ModelChanged;

        private AnimalChallengeModel _challengeData;
        private IChallengePageView _animalChallengePage;
        private CurrencyWallet _wallet;
        private SceneNavigationController _navigation;
        private ProjectileInventory _projectileInventory;
        private ProjectileIconCatalog _iconCatalog;
        private string _selectedProjectileId;

        public void Initialize(
            AnimalChallengeModel model,
            IChallengePageView challengePage,
            CurrencyWallet wallet,
            SceneNavigationController navigation = null,
            ProjectileInventory projectileInventory = null,
            ProjectileIconCatalog iconCatalog = null)
        {
            _challengeData = model ?? throw new ArgumentNullException(nameof(model));
            _animalChallengePage = challengePage ?? throw new ArgumentNullException(nameof(challengePage));
            _wallet = wallet;
            _navigation = navigation;
            _projectileInventory = projectileInventory;
            _iconCatalog = iconCatalog;
            _selectedProjectileId = ResolveInitialSelectedProjectileId();

            _animalChallengePage.PurchaseNextClicked += OnPurchaseNextClicked;
            _animalChallengePage.EquipCharacterClicked += OnEquipCharacterClicked;
            _animalChallengePage.CharacterSelected += OnCharacterSelected;
            if (_wallet != null)
                _wallet.BalanceChanged += OnWalletChanged;

            if (_projectileInventory != null)
            {
                _projectileInventory.InventoryChanged += OnProjectileInventoryChanged;
                _projectileInventory.EquippedProjectileChanged += OnEquippedProjectileChanged;
            }

            if (_navigation != null)
                _navigation.PageChanged += OnPageChanged;

            RefreshView();
        }

        public void Dispose()
        {
            if (_animalChallengePage != null)
            {
                _animalChallengePage.PurchaseNextClicked -= OnPurchaseNextClicked;
                _animalChallengePage.EquipCharacterClicked -= OnEquipCharacterClicked;
                _animalChallengePage.CharacterSelected -= OnCharacterSelected;
            }

            if (_wallet != null)
                _wallet.BalanceChanged -= OnWalletChanged;

            if (_projectileInventory != null)
            {
                _projectileInventory.InventoryChanged -= OnProjectileInventoryChanged;
                _projectileInventory.EquippedProjectileChanged -= OnEquippedProjectileChanged;
            }

            if (_navigation != null)
                _navigation.PageChanged -= OnPageChanged;
        }

        private string ResolveInitialSelectedProjectileId()
        {
            var rewardProjectileId = _challengeData?.RewardProjectileId;
            if (!string.IsNullOrEmpty(rewardProjectileId))
                return rewardProjectileId;

            return DefaultProjectileId;
        }

        private void OnPageChanged(NavigationPage page)
        {
            if (page == NavigationPage.AnimalChallenge)
                RefreshView();
        }

        private void OnWalletChanged(IReadOnlyDictionary<string, int> granted)
        {
            RefreshView();
        }

        private void OnProjectileInventoryChanged()
        {
            RefreshView();
        }

        private void OnEquippedProjectileChanged(string projectileId)
        {
            RefreshView();
        }

        public void RefreshView(int? justPurchasedSubstepIndex = null, bool didCompleteStep = false)
        {
            if (_animalChallengePage == null || _challengeData == null)
                return;

            var challengePageViewModel = new ChallengePageViewModel(
                _selectedProjectileId,
                BuildCharacterSelectionViewModel(),
                BuildCharacterPageViewModels(justPurchasedSubstepIndex, didCompleteStep));
            _animalChallengePage.Render(challengePageViewModel);
        }

        private List<CharacterPageViewModel> BuildCharacterPageViewModels(
            int? justPurchasedSubstepIndex,
            bool didCompleteStep)
        {
            var pages = new List<CharacterPageViewModel>();
            var rewardProjectileId = _challengeData.RewardProjectileId;

            pages.Add(ChallengePageViewModelMapper.MapDefaultPage(
                DefaultProjectileId,
                _selectedProjectileId == DefaultProjectileId,
                _projectileInventory?.Owns(DefaultProjectileId) ?? false,
                _projectileInventory?.IsEquipped(DefaultProjectileId) ?? false));

            if (!string.IsNullOrEmpty(rewardProjectileId))
            {
                pages.Add(ChallengePageViewModelMapper.MapChallengePage(
                    _challengeData,
                    _wallet,
                    rewardProjectileId,
                    _selectedProjectileId == rewardProjectileId,
                    _projectileInventory?.Owns(rewardProjectileId) ?? false,
                    _projectileInventory?.IsEquipped(rewardProjectileId) ?? false,
                    justPurchasedSubstepIndex,
                    didCompleteStep));
            }

            return pages;
        }

        private CharacterSelectionViewModel BuildCharacterSelectionViewModel()
        {
            var icons = new List<CharacterSelectionIconViewModel>();
            var rewardProjectileId = _challengeData.RewardProjectileId;

            if (!string.IsNullOrEmpty(rewardProjectileId))
            {
                icons.Add(CreateCharacterSelectionIconViewModel(
                    rewardProjectileId,
                    _projectileInventory?.Owns(rewardProjectileId) ?? false,
                    _projectileInventory?.IsEquipped(rewardProjectileId) ?? false));
            }

            if (_projectileInventory != null)
            {
                foreach (var projectileId in _projectileInventory.GetOwnedProjectileIds())
                {
                    if (projectileId == rewardProjectileId)
                        continue;

                    icons.Add(CreateCharacterSelectionIconViewModel(
                        projectileId,
                        true,
                        _projectileInventory.IsEquipped(projectileId)));
                }
            }

            return new CharacterSelectionViewModel(icons);
        }

        private CharacterSelectionIconViewModel CreateCharacterSelectionIconViewModel(
            string projectileId,
            bool isOwned,
            bool isEquipped)
        {
            return new CharacterSelectionIconViewModel(
                projectileId,
                _iconCatalog?.GetIcon(projectileId),
                isOwned,
                isEquipped,
                _selectedProjectileId == projectileId);
        }

        private void OnCharacterSelected(string projectileId)
        {
            if (string.IsNullOrEmpty(projectileId))
                return;

            _selectedProjectileId = projectileId;
            RefreshView();
            ModelChanged?.Invoke();
        }

        private void OnPurchaseNextClicked()
        {
            if (_challengeData == null || _wallet == null)
                return;

            var availableAt = _wallet.GetBalance("AT");
            var result = _challengeData.TryPurchaseNextSubstep(availableAt);
            if (!result.Succeeded)
                return;

            _wallet.TrySpend("AT", result.SpentAmount);

            foreach (var milestone in result.ClaimedMilestones)
                GrantMilestoneRewards(milestone);

            if (_challengeData.IsChallengeComplete)
                GrantChallengeProjectile();

            RefreshView(result.PurchasedSubstepIndex, result.DidCompleteStep);
            ModelChanged?.Invoke();
        }

        private void OnEquipCharacterClicked(string projectileId)
        {
            if (_projectileInventory == null || string.IsNullOrEmpty(projectileId))
                return;

            if (!_projectileInventory.Equip(projectileId))
                return;

            _selectedProjectileId = projectileId;
            RefreshView();
            ModelChanged?.Invoke();
        }

        private void GrantChallengeProjectile()
        {
            var rewardProjectileId = _challengeData.RewardProjectileId;
            if (!string.IsNullOrEmpty(rewardProjectileId))
                _projectileInventory?.Add(rewardProjectileId);
        }

        private void GrantMilestoneRewards(MilestoneDefinition milestone)
        {
            var amounts = new Dictionary<string, int>();

            foreach (var reward in milestone.Rewards)
            {
                if (reward.currency != null)
                {
                    if (reward.currency.AT > 0)
                        amounts["AT"] = reward.currency.AT;

                    if (reward.currency.CN > 0)
                        amounts["CN"] = reward.currency.CN;
                }

                if (!string.IsNullOrEmpty(reward.rewardId))
                    _projectileInventory?.Add(reward.rewardId);
            }

            if (amounts.Count > 0)
                _wallet.Grant(amounts);
        }
    }
}
