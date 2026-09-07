using System;
using UnityEngine;
using UnityEngine.UI;

namespace AnimalChallenge.UI
{
    public class CharacterPage : MonoBehaviour
    {
        [SerializeField] private GameObject _artRoot;
        [SerializeField] private Image _background;
        [SerializeField] private Text _titleText;
        [SerializeField] private ChallengeStep[] _challengeSteps;
        [SerializeField] private StepPanel _stepPanel;
        [SerializeField] private EquipCharacterPanel _equipCharacterPanel;

        public event Action PurchaseNextClicked;
        public event Action<string> EquipCharacterClicked;

        private void Awake()
        {
            if (_challengeSteps == null || _challengeSteps.Length == 0)
                _challengeSteps = GetComponentsInChildren<ChallengeStep>(true);

            if (_equipCharacterPanel == null)
                _equipCharacterPanel = GetComponentInChildren<EquipCharacterPanel>(true);

            if (_stepPanel != null)
                _stepPanel.PurchaseClicked += OnStepPanelPurchaseClicked;

            if (_equipCharacterPanel != null)
                _equipCharacterPanel.EquipClicked += OnEquipCharacterPanelClicked;
        }

        private void OnDestroy()
        {
            if (_stepPanel != null)
                _stepPanel.PurchaseClicked -= OnStepPanelPurchaseClicked;

            if (_equipCharacterPanel != null)
                _equipCharacterPanel.EquipClicked -= OnEquipCharacterPanelClicked;
        }

        public void Render(CharacterPageViewModel viewModel)
        {
            if (viewModel == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(viewModel.IsVisible);

            if (!viewModel.IsVisible)
                return;

            if (_titleText != null)
                _titleText.text = viewModel.Title;

            if (_artRoot != null)
                _artRoot.SetActive(viewModel.ShowChallengeArt);

            if (viewModel.ShowChallengeArt)
                RenderChallengeContent(viewModel);
            else if (_stepPanel != null)
                _stepPanel.gameObject.SetActive(false);

            if (_equipCharacterPanel != null)
                _equipCharacterPanel.Render(viewModel.EquipCharacterPanel);
        }

        private void RenderChallengeContent(CharacterPageViewModel viewModel)
        {
            RenderSteps(viewModel);
            RenderStepPanel(viewModel);
        }

        private void RenderSteps(CharacterPageViewModel viewModel)
        {
            if (_challengeSteps == null)
                return;

            for (var i = 0; i < _challengeSteps.Length; i++)
            {
                var step = _challengeSteps[i];
                if (step == null)
                    continue;

                if (i < viewModel.Steps.Count)
                    step.Render(viewModel.Steps[i]);
            }
        }

        private void RenderStepPanel(CharacterPageViewModel viewModel)
        {
            if (_stepPanel == null)
                return;

            if (viewModel.ActiveStepIndex < 0
                || viewModel.ActiveStepPanel == null
                || _challengeSteps == null
                || viewModel.ActiveStepIndex >= _challengeSteps.Length
                || _challengeSteps[viewModel.ActiveStepIndex] == null)
            {
                _stepPanel.gameObject.SetActive(false);
                return;
            }

            var activeStep = _challengeSteps[viewModel.ActiveStepIndex];
            PositionStepPanel(activeStep);
            _stepPanel.gameObject.SetActive(true);

            if (viewModel.UsePurchaseSuccessRender)
                _stepPanel.RenderPurchaseSuccess(viewModel.ActiveStepPanel);
            else
                _stepPanel.Render(viewModel.ActiveStepPanel);
        }

        private void PositionStepPanel(ChallengeStep activeStep)
        {
            var panelRect = _stepPanel.transform as RectTransform;
            var stepRect = activeStep.transform as RectTransform;

            if (panelRect == null || stepRect == null)
                return;

            panelRect.position = stepRect.position;
        }

        private void OnStepPanelPurchaseClicked()
        {
            PurchaseNextClicked?.Invoke();
        }

        private void OnEquipCharacterPanelClicked(string projectileId)
        {
            EquipCharacterClicked?.Invoke(projectileId);
        }
    }
}
