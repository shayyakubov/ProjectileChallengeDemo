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
        public event Action ModelChanged;

        private AnimalChallengeModel _challengeData;
        private IChallengePageView _animalChallengePage;
        private CurrencyWallet _wallet;
        private SceneNavigationController _navigation;
        private ProjectileInventory _projectileInventory;

        public void Initialize(
            AnimalChallengeModel model,
            IChallengePageView view,
            CurrencyWallet wallet,
            SceneNavigationController navigation = null,
            ProjectileInventory projectileInventory = null)
        {
            _challengeData = model ?? throw new ArgumentNullException(nameof(model));
            _animalChallengePage = view ?? throw new ArgumentNullException(nameof(view));
            _wallet = wallet;
            _navigation = navigation;
            _projectileInventory = projectileInventory;

            _animalChallengePage.PurchaseNextClicked += OnPurchaseNextClicked;
            _animalChallengePage.EquipCharacterClicked += OnEquipCharacterClicked;
            if (_wallet != null)
                _wallet.BalanceChanged += OnWalletChanged;

            if (_projectileInventory != null)
                _projectileInventory.InventoryChanged += OnProjectileInventoryChanged;

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
            }

            if (_wallet != null)
                _wallet.BalanceChanged -= OnWalletChanged;

            if (_projectileInventory != null)
                _projectileInventory.InventoryChanged -= OnProjectileInventoryChanged;

            if (_navigation != null)
                _navigation.PageChanged -= OnPageChanged;
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

        public void RefreshView(int? justPurchasedSubstepIndex = null, bool didCompleteStep = false)
        {
            if (_animalChallengePage == null || _challengeData == null)
                return;

            var challengePageViewModel = ChallengePageViewModelMapper.Map(
                _challengeData,
                _wallet,
                BuildEquipCharacterPanelViewModel(),
                justPurchasedSubstepIndex,
                didCompleteStep);
            _animalChallengePage.Render(challengePageViewModel);
        }

        private EquipCharacterPanelViewModel BuildEquipCharacterPanelViewModel()
        {
            var rewardProjectileId = _challengeData.RewardProjectileId;
            var ownsReward = _projectileInventory != null && _projectileInventory.Owns(rewardProjectileId);
            var isEquipped = _projectileInventory != null && _projectileInventory.IsEquipped(rewardProjectileId);
            return new EquipCharacterPanelViewModel(ownsReward, isEquipped);
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

        private void OnEquipCharacterClicked()
        {
            if (_challengeData == null || _projectileInventory == null)
                return;

            var rewardProjectileId = _challengeData.RewardProjectileId;
            if (!_projectileInventory.Equip(rewardProjectileId))
                return;

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
