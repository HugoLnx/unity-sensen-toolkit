using System.Collections;
using UnityEngine;

namespace SensenToolkit
{
    public class MovementCopycat : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        private Vector3 _lastPosition;
        private Quaternion _lastRotation;
        [SerializeField] private bool _copyPosition = true;
        [SerializeField] private bool _copyRotation = true;
        [Tooltip("Automatically start copying when enabled.")]
        [SerializeField] private bool _autoStart = true;
        [Tooltip("Automatically stop copying when disabled.")]
        [SerializeField] private bool _autoStop = true;
        private WaitForEndOfFrame _waitForEndOfFrame = new();

        private void OnEnable()
        {
            if (_autoStart && _target != null) StartCopycat(_target);
        }

        private void OnDisable()
        {
            if (_autoStop) StopCopycat();
        }

        public void StartCopycat(Transform target)
        {
            _target = target;
            _lastPosition = target.position;
            _lastRotation = target.rotation;
            StartCoroutine(CopycatLoop());
        }

        public void StopCopycat()
        {
            _target = null;
        }

        private IEnumerator CopycatLoop()
        {
            yield return _waitForEndOfFrame;
            while (_target != null)
            {
                if (_copyPosition) transform.position += _target.position - _lastPosition;
                if (_copyRotation) transform.rotation *= _target.rotation * Quaternion.Inverse(_lastRotation);
                _lastPosition = _target.position;
                _lastRotation = _target.rotation;
                yield return _waitForEndOfFrame;
            }
        }
    }
}
