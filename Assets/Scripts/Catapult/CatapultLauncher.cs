using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AnimalChallenge.Catapult
{
    public class CatapultLauncher : MonoBehaviour
    {
        public event Action ProjectileLaunched;
        public event Action ResetCompleted;

        [SerializeField] private Transform _armParent;
        [SerializeField] private Projectile _projectile;
        [SerializeField] private Rigidbody _projectileRigidbody;
        [SerializeField] private Camera _camera;
        [SerializeField] private float _maxBendAngleX = -75f;
        [SerializeField] private float _safeAreaBottomPaddingRatio = 0.1f;
        [SerializeField] private float _minBendToLaunch = 3f;
        [SerializeField] private float _minLaunchSpeed = 5f;
        [SerializeField] private float _maxLaunchSpeed = 28f;
        [SerializeField] private float _armSwingSmoothTime = 0.12f;
        [SerializeField] private float _launchAngleDegrees = 45f;
        [SerializeField] private float _resetDelay = 0.35f;
        [SerializeField] private LayerMask _catapultLayerMask = ~0;

        private Transform _projectileParent;
        private Vector3 _projectileLocalPosition;
        private Quaternion _projectileLocalRotation;
        private Quaternion _restRotation;
        private float _currentBendX;
        private float _armSwingVelocity;
        private float _pendingLaunchT = -1f;
        private bool _isPulling;
        private bool _canPull = true;
        private bool _navigationInputEnabled = true;
        private Coroutine _armSwingCoroutine;
        private Collider _projectileCollider;
        private float _pullStartScreenY;
        private float _cachedMaxDragPixels;

        private void Awake()
        {
            if (_camera == null)
                _camera = Camera.main;

            if (_projectile != null)
                _projectileCollider = _projectile.GetComponent<Collider>();

            CacheRestPose();
        }

        private void Start()
        {
            CacheRestPose();
            AttachProjectileToArm(restoreTransform: false);
        }

        private void Update()
        {
            if (!_navigationInputEnabled || !_canPull || _camera == null || !TryGetPointerPosition(out _))
                return;

            if (WasPointerPressedThisFrame() && IsPointerOnProjectile())
                BeginPull();

            if (_isPulling && IsPointerPressed())
                UpdatePull();

            if (_isPulling && WasPointerReleasedThisFrame())
                Release();
        }

        public void ScheduleReset()
        {
            Invoke(nameof(ResetCatapult), _resetDelay);
        }

        public void SetInputEnabled(bool enabled)
        {
            _navigationInputEnabled = enabled;

            if (!enabled)
                _isPulling = false;
        }

        public Projectile ReplaceProjectile(Projectile prefab)
        {
            if (prefab == null)
                return _projectile;

            if (_projectileParent == null)
                CacheRestPose();

            if (_projectileParent == null)
                return _projectile;

            PrepareForProjectileSwap();

            if (_projectile != null)
                Destroy(_projectile.gameObject);

            var instance = Instantiate(prefab, _projectileParent);
            instance.transform.localPosition = _projectileLocalPosition;
            instance.transform.localRotation = _projectileLocalRotation;
            DisablePreviewCameras(instance.gameObject);

            _projectile = instance;
            _projectileRigidbody = instance.GetComponent<Rigidbody>();
            _projectileCollider = instance.GetComponent<Collider>();

            AttachProjectileToArm(restoreTransform: false);
            return _projectile;
        }

        private void PrepareForProjectileSwap()
        {
            CancelInvoke(nameof(ResetCatapult));

            if (_armSwingCoroutine != null)
            {
                StopCoroutine(_armSwingCoroutine);
                _armSwingCoroutine = null;
            }

            _isPulling = false;
            _canPull = true;
            _pendingLaunchT = -1f;
            _currentBendX = 0f;
            _armSwingVelocity = 0f;

            if (_armParent != null)
                ApplyArmBend(0f);
        }

        private static void DisablePreviewCameras(GameObject projectileObject)
        {
            foreach (var camera in projectileObject.GetComponentsInChildren<Camera>(true))
                camera.gameObject.SetActive(false);
        }

        private void CacheRestPose()
        {
            if (_armParent != null)
            {
                _restRotation = _armParent.localRotation;
                _currentBendX = 0f;
            }

            if (_projectile == null)
                return;

            var projectileTransform = _projectile.transform;
            _projectileParent = projectileTransform.parent;
            _projectileLocalPosition = projectileTransform.localPosition;
            _projectileLocalRotation = projectileTransform.localRotation;
        }

        private Vector3 ComputeLaunchDirection()
        {
            var horizontal = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            if (horizontal.sqrMagnitude < 0.001f)
                horizontal = Vector3.ProjectOnPlane(_armParent.forward, Vector3.up).normalized;

            var angleRad = _launchAngleDegrees * Mathf.Deg2Rad;
            return (horizontal * Mathf.Cos(angleRad) + Vector3.up * Mathf.Sin(angleRad)).normalized;
        }

        private void BeginPull()
        {
            if (!TryGetPointerPosition(out var pointerPosition))
                return;

            _isPulling = true;
            _pullStartScreenY = pointerPosition.y;
            CacheDragRange();
            UpdatePull();
        }

        private void CacheDragRange()
        {
            var dragLimitY = GetDragLimitScreenY();
            var ballBottomY = GetProjectileBottomScreenY();
            _cachedMaxDragPixels = Mathf.Max(ballBottomY - dragLimitY, 1f);
        }

        private void UpdatePull()
        {
            if (!TryGetPointerPosition(out var pointerPosition))
                return;

            var pixelsPulled = _pullStartScreenY - pointerPosition.y;
            var pullT = Mathf.Clamp01(pixelsPulled / _cachedMaxDragPixels);
            _currentBendX = Mathf.Lerp(0f, _maxBendAngleX, pullT);
            ApplyArmBend(_currentBendX);
        }

        private float GetProjectileBottomScreenY()
        {
            if (_projectileCollider == null || _camera == null)
                return GetPointerScreenY();

            var center = _projectileCollider.bounds.center;
            var radius = _projectileCollider.bounds.extents.x;
            var screenCenter = _camera.WorldToScreenPoint(center);

            if (screenCenter.z <= 0f)
                return GetPointerScreenY();

            var screenEdge = _camera.WorldToScreenPoint(center + _camera.transform.right * radius);
            var screenRadius = Vector2.Distance(
                new Vector2(screenCenter.x, screenCenter.y),
                new Vector2(screenEdge.x, screenEdge.y));

            return screenCenter.y - screenRadius;
        }

        private float GetDragLimitScreenY()
        {
            var safeArea = Screen.safeArea;
            return safeArea.y + safeArea.height * _safeAreaBottomPaddingRatio;
        }

        private void Release()
        {
            _isPulling = false;
            _pendingLaunchT = -1f;

            if (Mathf.Abs(_currentBendX) < _minBendToLaunch)
            {
                StartArmSwing(0f);
                return;
            }

            _canPull = false;
            _pendingLaunchT = GetLaunchT();
            StartArmSwing(0f);
        }

        private float GetLaunchT()
        {
            return Mathf.InverseLerp(0f, _maxBendAngleX, _currentBendX);
        }

        private void LaunchProjectile(float launchT)
        {
            var launchSpeed = Mathf.Lerp(_minLaunchSpeed, _maxLaunchSpeed, launchT);
            var launchDirection = ComputeLaunchDirection();

            _projectile.transform.SetParent(null, true);

            if (_projectileCollider != null)
                _projectileCollider.isTrigger = false;

            _projectileRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _projectileRigidbody.detectCollisions = true;
            _projectileRigidbody.isKinematic = false;
            _projectileRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _projectileRigidbody.linearVelocity = launchDirection * launchSpeed;
            _projectile.BeginFlight();
            ProjectileLaunched?.Invoke();
        }

        private void StartArmSwing(float targetBendX)
        {
            if (_armSwingCoroutine != null)
                StopCoroutine(_armSwingCoroutine);

            _armSwingCoroutine = StartCoroutine(SwingArmToBend(targetBendX));
        }

        private IEnumerator SwingArmToBend(float targetBendX)
        {
            _armSwingVelocity = 0f;
            var launchOnArrival = _pendingLaunchT >= 0f;

            if (launchOnArrival)
            {
                var releaseSpeed = Mathf.Abs(_maxBendAngleX) / Mathf.Max(_armSwingSmoothTime, 0.01f);

                while (_currentBendX < targetBendX - 0.001f)
                {
                    _currentBendX = Mathf.MoveTowards(_currentBendX, targetBendX, releaseSpeed * Time.deltaTime);
                    ApplyArmBend(_currentBendX);
                    yield return null;
                }

                _currentBendX = targetBendX;
                ApplyArmBend(_currentBendX);
                _armSwingCoroutine = null;
                LaunchProjectile(_pendingLaunchT);
                _pendingLaunchT = -1f;
                yield break;
            }

            while (Mathf.Abs(_currentBendX - targetBendX) > 0.05f)
            {
                _currentBendX = Mathf.SmoothDamp(
                    _currentBendX,
                    targetBendX,
                    ref _armSwingVelocity,
                    _armSwingSmoothTime);
                ApplyArmBend(_currentBendX);
                yield return null;
            }

            _currentBendX = targetBendX;
            ApplyArmBend(_currentBendX);
            _armSwingCoroutine = null;
        }

        private void ApplyArmBend(float bendX)
        {
            _armParent.localRotation = _restRotation * Quaternion.Euler(bendX, 0f, 0f);
        }

        private void AttachProjectileToArm(bool restoreTransform)
        {
            if (_projectile == null || _projectileRigidbody == null)
                return;

            var projectileTransform = _projectile.transform;

            if (restoreTransform)
            {
                projectileTransform.SetParent(_projectileParent, false);
                projectileTransform.localPosition = _projectileLocalPosition;
                projectileTransform.localRotation = _projectileLocalRotation;
            }

            _projectileRigidbody.linearVelocity = Vector3.zero;
            _projectileRigidbody.angularVelocity = Vector3.zero;
            _projectileRigidbody.isKinematic = true;
            _projectileRigidbody.detectCollisions = false;
            _projectileRigidbody.interpolation = RigidbodyInterpolation.None;
            _projectileRigidbody.constraints = RigidbodyConstraints.FreezeAll;

            if (_projectileCollider != null)
                _projectileCollider.isTrigger = true;
        }

        private void ResetCatapult()
        {
            CancelInvoke(nameof(ResetCatapult));

            if (_armSwingCoroutine != null)
            {
                StopCoroutine(_armSwingCoroutine);
                _armSwingCoroutine = null;
            }

            _isPulling = false;
            _canPull = true;
            _pendingLaunchT = -1f;
            _currentBendX = 0f;
            _armSwingVelocity = 0f;

            if (_armParent != null)
                ApplyArmBend(0f);

            AttachProjectileToArm(restoreTransform: true);
            ResetCompleted?.Invoke();
        }

        private bool IsPointerOnProjectile()
        {
            if (_projectileCollider == null || _camera == null || !TryGetPointerPosition(out var pointerPosition))
                return false;

            var ray = _camera.ScreenPointToRay(pointerPosition);
            return _projectileCollider.Raycast(ray, out _, 100f);
        }

        private bool TryGetPointerPosition(out Vector2 position)
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                position = Touchscreen.current.primaryTouch.position.ReadValue();
                return true;
            }

            if (Mouse.current != null)
            {
                position = Mouse.current.position.ReadValue();
                return true;
            }

            position = default;
            return false;
        }

        private float GetPointerScreenY()
        {
            return TryGetPointerPosition(out var position) ? position.y : 0f;
        }

        private bool WasPointerPressedThisFrame()
        {
            return (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                   (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame);
        }

        private bool IsPointerPressed()
        {
            return (Mouse.current != null && Mouse.current.leftButton.isPressed) ||
                   (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed);
        }

        private bool WasPointerReleasedThisFrame()
        {
            return (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame) ||
                   (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame);
        }
    }
}
