using System;
using UnityEngine;
using UnityEngine.UI;

namespace AnimalChallenge.UI
{
    public class ChallengePage : MonoBehaviour, IChallengePageView
    {
        [SerializeField] private Image _background;
        [SerializeField] private Text _titleText;
        [SerializeField] private ChallengeStep[] _challengeSteps;
        [SerializeField] private StepPanel _stepPanel;
        [SerializeField] private EquipCharacterPanel _equipCharacterPanel;

        public event Action PurchaseNextClicked;
        public event Action EquipCharacterClicked;

        private void Awake()
        {
            if ((_challengeSteps == null || _challengeSteps.Length == 0))
                _challengeSteps = GetComponentsInChildren<ChallengeStep>(true);

            if (_stepPanel != null)
                _stepPanel.PurchaseClicked += OnStepPanelPurchaseClicked;

            if (_equipCharacterPanel == null)
                _equipCharacterPanel = GetComponentInChildren<EquipCharacterPanel>(true);

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

        public void Render(ChallengePageViewModel viewModel)
        {
            if (viewModel == null)
                return;

            if (_titleText != null)
                _titleText.text = viewModel.Title;

            RenderSteps(viewModel);
            RenderStepPanel(viewModel);
            RenderEquipCharacterPanel(viewModel);
        }

        private void RenderSteps(ChallengePageViewModel viewModel)
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

        private void RenderStepPanel(ChallengePageViewModel viewModel)
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

        private void RenderEquipCharacterPanel(ChallengePageViewModel viewModel)
        {
            if (_equipCharacterPanel == null)
                return;

            _equipCharacterPanel.Render(viewModel.EquipCharacterPanel);
        }

        private void OnEquipCharacterPanelClicked()
        {
            EquipCharacterClicked?.Invoke();
        }
    }
}
