using System;

namespace AnimalChallenge.UI
{
    public interface IChallengePageView
    {
        event Action PurchaseNextClicked;
        event Action EquipCharacterClicked;
        void Render(ChallengePageViewModel viewModel);
    }
}
