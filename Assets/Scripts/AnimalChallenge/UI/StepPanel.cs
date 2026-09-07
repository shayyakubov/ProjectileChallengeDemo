using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnimalChallenge.UI
{

    public class StepPanel : MonoBehaviour
    {
        [SerializeField] private DoneIndicator[] _substepIndicators;
        [SerializeField] private GameObject[] _spacers;
        [SerializeField] private Button _purchaseStepButton;
        [SerializeField] private TMP_Text _stepPriceLabel;
        [SerializeField] private float _purchaseSuccessAnimationDuration = 0.4f;

        public event Action PurchaseClicked;

        private Coroutine _purchaseSuccessCoroutine;

        private void Awake()
        {
            if (_purchaseStepButton != null)
                _purchaseStepButton.onClick.AddListener(OnPurchaseStepClicked);
        }

        private void OnDestroy()
        {
            StopPurchaseSuccessAnimation();

            if (_purchaseStepButton != null)
                _purchaseStepButton.onClick.RemoveListener(OnPurchaseStepClicked);
        }

        public void Render(SubstepPanelViewModel viewModel)
        {
            if (viewModel == null)
                return;

            StopPurchaseSuccessAnimation();

            var activeIndicatorCount = viewModel.SubstepDoneStates.Count;
            var activeSpacerCount = activeIndicatorCount + 1;

            RenderIndicators(viewModel.SubstepDoneStates, activeIndicatorCount);
            RenderSpacers(activeSpacerCount);
            RenderPurchase(viewModel);
        }

        public void RenderPurchaseSuccess(SubstepPanelViewModel viewModel)
        {
            if (viewModel == null)
                return;

            StopPurchaseSuccessAnimation();
            gameObject.SetActive(true);
            _purchaseSuccessCoroutine = StartCoroutine(RenderPurchaseSuccessRoutine(viewModel));
        }

        private IEnumerator RenderPurchaseSuccessRoutine(SubstepPanelViewModel viewModel)
        {
            var doneStates = viewModel.SubstepDoneStates;
            var activeIndicatorCount = doneStates.Count;
            var animatingIndex = GetJustCompletedIndicatorIndex(doneStates);

            RenderSpacers(activeIndicatorCount + 1);

            DoneIndicator animatingIndicator = null;

            for (var i = 0; i < _substepIndicators.Length; i++)
            {
                var indicator = _substepIndicators[i];
                if (indicator == null)
                    continue;

                var isActive = i < activeIndicatorCount;
                indicator.gameObject.SetActive(isActive);

                if (!isActive)
                    continue;

                if (i < animatingIndex)
                    indicator.Render(true);
                else if (i == animatingIndex)
                {
                    indicator.Render(false);
                    animatingIndicator = indicator;
                }
                else
                    indicator.Render(false);
            }

            RenderPurchase(viewModel);

            if (animatingIndicator != null)
                yield return animatingIndicator.AnimateToDone(_purchaseSuccessAnimationDuration);

            _purchaseSuccessCoroutine = null;
        }

        private static int GetJustCompletedIndicatorIndex(IReadOnlyList<bool> doneStates)
        {
            for (var i = doneStates.Count - 1; i >= 0; i--)
            {
                if (doneStates[i])
                    return i;
            }

            return 0;
        }

        private void StopPurchaseSuccessAnimation()
        {
            if (_purchaseSuccessCoroutine == null)
                return;

            StopCoroutine(_purchaseSuccessCoroutine);
            _purchaseSuccessCoroutine = null;
        }

        private void RenderIndicators(IReadOnlyList<bool> doneStates, int activeCount)
        {
            if (_substepIndicators == null)
                return;

            for (var i = 0; i < _substepIndicators.Length; i++)
            {
                var indicator = _substepIndicators[i];
                if (indicator == null)
                    continue;

                var isActive = i < activeCount;
                indicator.gameObject.SetActive(isActive);

                if (isActive)
                    indicator.Render(doneStates[i]);
            }
        }

        private void RenderSpacers(int activeCount)
        {
            if (_spacers == null)
                return;

            for (var i = 0; i < _spacers.Length; i++)
            {
                var spacer = _spacers[i];
                if (spacer != null)
                    spacer.SetActive(i < activeCount);
            }
        }

        private void RenderPurchase(SubstepPanelViewModel viewModel)
        {
            if (_purchaseStepButton != null)
                _purchaseStepButton.interactable = viewModel.CanPurchase;

            if (_stepPriceLabel != null)
                _stepPriceLabel.text = viewModel.NextSubstepPrice != null
                    ? $"{viewModel.NextSubstepPrice} AT"
                    : "Complete";
        }

        private void OnPurchaseStepClicked()
        {
            PurchaseClicked?.Invoke();
        }
    }

    public sealed class SubstepPanelViewModel
    {
        public IReadOnlyList<bool> SubstepDoneStates { get; }
        public int? NextSubstepPrice { get; }
        public bool CanPurchase { get; }

        public SubstepPanelViewModel(
            IReadOnlyList<bool> substepDoneStates,
            int? nextSubstepPrice,
            bool canPurchase)
        {
            SubstepDoneStates = substepDoneStates ?? Array.Empty<bool>();
            NextSubstepPrice = nextSubstepPrice;
            CanPurchase = canPurchase;
        }
    }
}
