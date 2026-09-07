using System;

namespace AnimalChallenge.UI
{
    public interface IChallengePageView
    {
        event Action PurchaseNextClicked;
        event Action<string> EquipCharacterClicked;
        event Action<string> CharacterSelected;
        void Render(ChallengePageViewModel viewModel);
    }
}
