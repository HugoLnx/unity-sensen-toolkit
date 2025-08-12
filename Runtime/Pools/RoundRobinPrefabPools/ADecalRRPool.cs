using UnityEngine;
using MyBox;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.UIElements;


namespace SensenToolkit
{
    public abstract class ADecalRRPool : ATransientSingleton<ADecalRRPool>
    {
        [SerializeField, MustBeAssigned] protected DecalURPWrapper DecalPrefab;
        [Header("Randomization")]
        [SerializeField] protected RangedFloat ScaleRange = new(0.9f, 1.1f);
        [SerializeField] protected RangedFloat ZRotationRange = new(0f, 360f);
        [SerializeField] protected bool RandomizeTile = true;

        [Header("Fade Out")]
        [SerializeField] protected float FadeOutDelay = 15f;
        [SerializeField] protected float FadeOutDuration = 2f;
        [SerializeField] protected Ease FadeOutEase = Ease.InCubic;
        [SerializeField, AutoProperty(AutoPropertyMode.Scene)]
        protected RoundRobinPrefabPools Pools;

        public void SpawnAt(Vector3 position, Quaternion rotation)
        {
            DecalURPWrapper decal = GetNextDecal();
            decal.transform.SetPositionAndRotation(position, rotation);
            float zRotationAngle = UnityEngine.Random.Range(ZRotationRange.Min, ZRotationRange.Max);
            var zRotation = Quaternion.AngleAxis(zRotationAngle, decal.transform.forward);
            decal.transform.rotation = decal.transform.rotation * zRotation;
            if (RandomizeTile) decal.SetRandomTile();
        }

        public DecalURPWrapper GetNextDecal()
        {
            DecalURPWrapper decal = Pools.GetInstanceOf(DecalPrefab);
            float scale = UnityEngine.Random.Range(ScaleRange.Min, ScaleRange.Max);
            decal.Size = decal.OriginalSize * scale;
            decal.EnsureVisibleForDuration(FadeOutDelay, FadeOutDuration, FadeOutEase);
            return decal;
        }
    }
}
