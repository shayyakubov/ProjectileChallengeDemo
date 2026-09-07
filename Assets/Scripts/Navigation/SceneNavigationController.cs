using System;
using System.Collections;
using AnimalChallenge.Catapult;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace AnimalChallenge.Navigation
{
    public enum NavigationPage
    {
        AnimalChallenge,
        Catapult
    }

    public class SceneNavigationController : MonoBehaviour
    {
        [SerializeField] private RectTransform _screenPager;
        [SerializeField] private RectTransform _challengePage;
        [SerializeField] private RectTransform _catapultPage;
        [SerializeField] private Canvas _rootCanvas;
        [SerializeField] private UnityEngine.Camera _camera;
        [SerializeField] private CatapultLauncher _launcher;
        [SerializeField] private CameraTargetController _cameraTargetController;
        [SerializeField] private float _slideDuration = 0.45f;
        [SerializeField] private float _swipeThresholdPixels = 60f;
        [SerializeField] private float _worldSlideDistance = 45f;

        private float _pageWidth;
        private Vector3 _baseCameraPosition;
        private NavigationPage _currentPage = NavigationPage.Catapult;
        private bool _isTransitioning;
        private Coroutine _slideCoroutine;
        private Vector2 _swipeStartPosition;
        private bool _trackingSwipe;

        public NavigationPage CurrentPage => _currentPage;
        public bool IsTransitioning => _isTransitioning;

        public event Action<NavigationPage> PageChanged;

        private void Awake()
        {
            if (_rootCanvas == null && _screenPager != null)
                _rootCanvas = _screenPager.GetComponentInParent<Canvas>();

            if (_camera != null)
                _baseCameraPosition = _camera.transform.position;

            CachePageWidth();
            LayoutPages();
        }

        private void Start()
        {
            ApplyPageState(NavigationPage.Catapult);
        }

        private void Update()
        {
            if (_isTransitioning)
                return;

            HandleSwipeInput();
        }

        public void NavigateTo(NavigationPage page)
        {
            if (page == _currentPage || _isTransitioning)
                return;

            if (_slideCoroutine != null)
                StopCoroutine(_slideCoroutine);

            _slideCoroutine = StartCoroutine(SlideToPage(page));
        }

        private void CachePageWidth()
        {
            Canvas.ForceUpdateCanvases();

            if (_rootCanvas != null)
                _pageWidth = _rootCanvas.GetComponent<RectTransform>().rect.width;

            if (_pageWidth <= 0f)
                _pageWidth = Screen.width;
        }

        private void LayoutPages()
        {
            if (_screenPager == null || _challengePage == null || _catapultPage == null)
                return;

            ConfigurePage(_challengePage, 0f);
            ConfigurePage(_catapultPage, _pageWidth);

            _screenPager.anchorMin = new Vector2(0f, 0f);
            _screenPager.anchorMax = new Vector2(0f, 1f);
            _screenPager.pivot = new Vector2(0f, 0.5f);
            _screenPager.sizeDelta = new Vector2(_pageWidth * 2f, 0f);
            _screenPager.anchoredPosition = new Vector2(_screenPager.anchoredPosition.x, 0f);
        }

        private void ConfigurePage(RectTransform page, float x)
        {
            page.anchorMin = new Vector2(0f, 0f);
            page.anchorMax = new Vector2(0f, 1f);
            page.pivot = new Vector2(0f, 0.5f);
            page.sizeDelta = new Vector2(_pageWidth, 0f);
            page.anchoredPosition = new Vector2(x, 0f);
        }

        private void HandleSwipeInput()
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.wasPressedThisFrame)
                    BeginSwipeTracking(touch.position.ReadValue());
                else if (_trackingSwipe && touch.press.wasReleasedThisFrame)
                    EndSwipeTracking(touch.position.ReadValue());

                return;
            }

            if (Mouse.current == null)
                return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
                BeginSwipeTracking(Mouse.current.position.ReadValue());
            else if (_trackingSwipe && Mouse.current.leftButton.wasReleasedThisFrame)
                EndSwipeTracking(Mouse.current.position.ReadValue());
        }

        private void BeginSwipeTracking(Vector2 position)
        {
            if (IsPointerOverBlockingUI())
                return;

            _swipeStartPosition = position;
            _trackingSwipe = true;
        }

        private void EndSwipeTracking(Vector2 position)
        {
            _trackingSwipe = false;

            var delta = position - _swipeStartPosition;
            if (Mathf.Abs(delta.x) < _swipeThresholdPixels || Mathf.Abs(delta.x) <= Mathf.Abs(delta.y))
                return;

            if (delta.x > 0f && _currentPage == NavigationPage.Catapult)
                NavigateTo(NavigationPage.AnimalChallenge);
            else if (delta.x < 0f && _currentPage == NavigationPage.AnimalChallenge)
                NavigateTo(NavigationPage.Catapult);
        }

        private bool IsPointerOverBlockingUI()
        {
            if (EventSystem.current == null)
                return false;

            if (_currentPage == NavigationPage.AnimalChallenge)
                return false;

            return EventSystem.current.IsPointerOverGameObject();
        }

        private IEnumerator SlideToPage(NavigationPage page)
        {
            _isTransitioning = true;

            if (page == NavigationPage.AnimalChallenge)
                SetCatapultSystemsEnabled(false);

            var startX = _screenPager.anchoredPosition.x;
            var targetX = GetPagerXForPage(page);
            var elapsed = 0f;

            while (elapsed < _slideDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.SmoothStep(0f, 1f, elapsed / _slideDuration);
                var pos = _screenPager.anchoredPosition;
                pos.x = Mathf.Lerp(startX, targetX, t);
                _screenPager.anchoredPosition = pos;
                ApplyCameraOffset();
                yield return null;
            }

            _screenPager.anchoredPosition = new Vector2(targetX, _screenPager.anchoredPosition.y);
            ApplyCameraOffset();
            _currentPage = page;
            _isTransitioning = false;
            _slideCoroutine = null;

            if (page == NavigationPage.Catapult)
                SetCatapultSystemsEnabled(true);

            PageChanged?.Invoke(page);
        }

        private void ApplyPageState(NavigationPage page)
        {
            _currentPage = page;
            SetCatapultSystemsEnabled(page == NavigationPage.Catapult);

            if (_screenPager != null)
            {
                _screenPager.anchoredPosition = new Vector2(GetPagerXForPage(page), _screenPager.anchoredPosition.y);
                ApplyCameraOffset();
            }

            PageChanged?.Invoke(page);
        }

        private void ApplyCameraOffset()
        {
            if (_camera == null || _screenPager == null || _pageWidth <= 0f)
                return;

            var slideT = (_screenPager.anchoredPosition.x + _pageWidth) / _pageWidth;
            var pos = _baseCameraPosition;
            pos.x -= slideT * _worldSlideDistance;
            _camera.transform.position = pos;
        }

        private float GetPagerXForPage(NavigationPage page)
        {
            return page == NavigationPage.Catapult ? -_pageWidth : 0f;
        }

        private void SetCatapultSystemsEnabled(bool enabled)
        {
            _cameraTargetController?.SetFollowEnabled(enabled);
            _launcher?.SetInputEnabled(enabled);
        }
    }
}
