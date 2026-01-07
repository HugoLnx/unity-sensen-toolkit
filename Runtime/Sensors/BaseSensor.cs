using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public abstract class SensorBase : MonoBehaviour
    {
        #region Properties
        [SerializeField] private string[] _onlyTags;
        private GameObject _oneSensed;

        public readonly HashSet<GameObject> Sensed = new();
        public readonly HashSet<SensorTarget> _sensedTargets = new();
        public readonly List<SensorTarget> _sensedTargetsBuffer = new();
        public bool IsSensing => Sensed.Count > 0;
        public GameObject OneSensed => IsSensing ? GetOneSensed() : null;
        #endregion

        #region Events
        public event Action<GameObject> OnSensedEnter;
        public event Action<GameObject> OnSensedExit;
        #endregion

        #region Lifecycle
        protected void OnEnter(Collider other)
        {
            if (IsValid(other))
            {
                SensorTarget target = Componentsx.EnsureComponent<SensorTarget>(other.gameObject);
                EnterTarget(target);
            }
        }

        protected void OnExit(Collider other)
        {
            if (IsValid(other) && Sensed.Contains(other.gameObject))
            {
                SensorTarget target = other.GetComponent<SensorTarget>();
                ExitTarget(target);
            }
        }

        private void OnDisable() => ClearTargets();
        private void OnDestroy() => ClearTargets();
        #endregion

        #region Private

        private void EnterTarget(SensorTarget target)
        {
            target.OnDestroyed += ExitTarget;
            target.OnDisabled += ExitTarget;
            Sensed.Add(target.gameObject);
            _sensedTargets.Add(target);
            OnSensedEnter?.Invoke(target.gameObject);
        }

        private void ExitTarget(SensorTarget target)
        {
            target.OnDestroyed -= ExitTarget;
            target.OnDisabled -= ExitTarget;
            Sensed.Remove(target.gameObject);
            _sensedTargets.Remove(target);
            if (!Sensed.Contains(_oneSensed))
            {
                _oneSensed = null;
            }
            OnSensedExit?.Invoke(target.gameObject);
        }
        private bool IsValid(Collider other)
        {
            if (_onlyTags.Length == 0)
            {
                return true;
            }

            foreach (string tag in _onlyTags)
            {
                if (other.CompareTag(tag))
                {
                    return true;
                }
            }

            return false;
        }

        private void ClearTargets()
        {
            _sensedTargetsBuffer.Clear();
            _sensedTargetsBuffer.AddRange(_sensedTargets);
            foreach (SensorTarget target in _sensedTargetsBuffer)
            {
                ExitTarget(target);
            }
        }

        private GameObject GetOneSensed()
        {
            if (_oneSensed == null)
            {
                _oneSensed = Sensed.FirstOrDefault();
            }
            return _oneSensed;
        }
        #endregion

        [SerializeField][ReadOnly] private bool _isSensingDebug;
        [SerializeField][ReadOnly] private List<string> _sensedDebug = new();

#if UNITY_EDITOR
        private void Start()
        {
            OnSensedEnter += (_) => RefreshSensedDebug();
            OnSensedExit += (_) => RefreshSensedDebug();
        }

        private void Update()
        {
            _isSensingDebug = IsSensing;
        }

        private void RefreshSensedDebug()
        {
            _sensedDebug.Clear();
            foreach (GameObject sensed in Sensed)
            {
                _sensedDebug.Add(sensed == null ? "<null>" : sensed.name);
            }
        }
#endif
    }
}
