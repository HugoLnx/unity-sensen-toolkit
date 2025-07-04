using UnityEngine;

namespace SensenToolkit
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "Audio3DSettings", menuName = "Sensen/Audio/3D Settings", order = 1)]
    public class Audio3DSettings : ScriptableObject
    {
        private static Audio3DSettings s_default;
        public static Audio3DSettings Default => s_default;

        [field: SerializeField] public float MinDistance { get; private set; } = 1f;
        [field: SerializeField] public float MaxDistance { get; private set; } = 500f;
        [field: SerializeField, Range(0f, 1f)] public float SpatialBlend { get; private set; } = 0f;
        [field: SerializeField] public float Spread { get; private set; } = 0f;

        public void ApplyToSource(AudioSource source)
        {
            source.minDistance = MinDistance;
            source.maxDistance = MaxDistance;
            source.spatialBlend = SpatialBlend;
            source.spread = Spread;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitStatics()
        {
            s_default = CreateInstance<Audio3DSettings>();
        }
    }
}
