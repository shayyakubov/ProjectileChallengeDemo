using UnityEngine;
using UnityEngine.UI;

namespace AnimalChallenge.Navigation
{
    public class NavigationPanel : MonoBehaviour
    {
        [SerializeField] private SceneNavigationController _navigation;
        [SerializeField] private Button _animalChallengeButton;
        [SerializeField] private Button _catapultButton;
        [SerializeField] private Text _animalChallengeLabel;
        [SerializeField] private Text _catapultLabel;
        [SerializeField] private Color _selectedColor = Color.white;
        [SerializeField] private Color _normalColor = new(0.75f, 0.75f, 0.75f, 1f);

        private void Awake()
        {
            if (_animalChallengeButton != null)
                _animalChallengeButton.onClick.AddListener(() => _navigation?.NavigateTo(NavigationPage.AnimalChallenge));

            if (_catapultButton != null)
                _catapultButton.onClick.AddListener(() => _navigation?.NavigateTo(NavigationPage.Catapult));
        }

        private void Update()
        {
            if (_navigation == null)
                return;

            var onChallenge = _navigation.CurrentPage == NavigationPage.AnimalChallenge;
            SetLabelColor(_animalChallengeLabel, onChallenge, _selectedColor, _normalColor);
            SetLabelColor(_catapultLabel, !onChallenge, _selectedColor, _normalColor);
        }

        private static void SetLabelColor(Text label, bool selected, Color selectedColor, Color normalColor)
        {
            if (label == null)
                return;

            label.color = selected ? selectedColor : normalColor;
        }
    }
}
