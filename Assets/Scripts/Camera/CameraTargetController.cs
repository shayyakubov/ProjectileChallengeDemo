using AnimalChallenge.Catapult;
using UnityEngine;

public class CameraTargetController : MonoBehaviour
{
    [SerializeField] private Transform _projectileTarget;
    [SerializeField] private CatapultLauncher _launcher;
    [SerializeField] private float _targetSmoothSpeed = 5f;

    private Transform _currentTarget;
    private Vector3 _positionOffset;
    private Quaternion _cameraRotation;

    private void Awake()
    {
        if (_launcher == null)
            return;

        _positionOffset = transform.position - _launcher.transform.position;
        _cameraRotation = transform.rotation;
    }

    private void OnEnable()
    {
        if (_launcher != null)
        {
            _launcher.ProjectileLaunched += SetProjectileTarget;
            _launcher.ResetCompleted += SetCatapultTarget;
        }
    }

    private void OnDisable()
    {
        if (_launcher != null)
        {
            _launcher.ProjectileLaunched -= SetProjectileTarget;
            _launcher.ResetCompleted -= SetCatapultTarget;
        }
    }

    private void Start()
    {
        SetCatapultTarget();
    }

    private bool _followEnabled = true;

    private void LateUpdate()
    {
        if (!_followEnabled || _currentTarget == null)
            return;

        var desiredPosition = _currentTarget.position + _positionOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * _targetSmoothSpeed);
        transform.rotation = _cameraRotation;
    }

    private void SetCatapultTarget()
    {
        _currentTarget = _launcher != null ? _launcher.transform : null;
    }

    private void SetProjectileTarget()
    {
        _currentTarget = _projectileTarget;
    }

    public void SetFollowEnabled(bool enabled)
    {
        _followEnabled = enabled;

        if (enabled)
            SetCatapultTarget();
    }

    public void SetProjectileTarget(Transform target)
    {
        _projectileTarget = target;
    }
}
