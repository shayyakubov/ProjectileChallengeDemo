using System;
using System.Collections;
using UnityEngine;

namespace AnimalChallenge.Catapult
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        public event Action<Vector3> Landed;

        [SerializeField] private string _groundTag = "Ground";
        [SerializeField] private float _settleVelocityThreshold = 0.05f;
        [SerializeField] private float _settleMaxWait = 8f;

        private Rigidbody _rigidbody;
        private bool _inFlight;
        private bool _isSettling;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void BeginFlight()
        {
            _inFlight = true;
            _isSettling = false;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbody.angularVelocity = Vector3.zero;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_inFlight || _isSettling || !collision.gameObject.CompareTag(_groundTag))
                return;

            _isSettling = true;
            _rigidbody.angularVelocity = Vector3.zero;
            StartCoroutine(WaitForSettledLanding());
        }

        private IEnumerator WaitForSettledLanding()
        {
            var elapsed = 0f;
            while (_rigidbody.linearVelocity.magnitude > _settleVelocityThreshold && elapsed < _settleMaxWait)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            _inFlight = false;
            _isSettling = false;
            Landed?.Invoke(transform.position);
        }
    }
}
