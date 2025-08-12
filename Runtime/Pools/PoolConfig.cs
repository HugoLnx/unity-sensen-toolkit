using System;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    [Serializable]
    public class PoolConfig
    {
        public int MinSize;
        public int MaxCreations;

        [Tooltip("If true, the pool will create minSize instances on start.")]
        public bool Prefill;

        [Tooltip("If true, the instance will deactivate on creation, activate on getting it, and deactivated again on release.")]
        public bool AutoDeactivate;
        [SerializeField, ReadOnly] public GameObject Container;
    }
}
