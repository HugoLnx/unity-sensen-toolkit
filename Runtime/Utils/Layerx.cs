using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace SensenToolkit
{
    public static class Layerx
    {
        private static Dictionary<int, int> s_masksByLayer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitalizeStatics()
        {
            s_masksByLayer = new Dictionary<int, int>();
            for (int i = 0; i < 32; i++)
            {
                int mask = 0;
                for (int j = 0; j < 32; j++)
                {
                    if (!Physics.GetIgnoreLayerCollision(i, j))
                    {
                        mask |= 1 << j;
                    }
                }
                s_masksByLayer.Add(i, mask);
            }
        }

        [Conditional("UNITY_EDITOR")]
        private static void InitializeStaticsInEditor()
        {
            if (Application.isPlaying) return;
            InitalizeStatics();
        }

        public static int MaskForWhatCollidesWith(int layer)
        {
            InitializeStaticsInEditor();
            return s_masksByLayer[layer];
        }

        public static int MaskForWhatDoesntCollideWith(int layer)
        {
            InitializeStaticsInEditor();
            return ~s_masksByLayer[layer];
        }

        public static int BuildLayerMask(params int[] layers)
        {
            InitializeStaticsInEditor();
            int mask = 0x00;
            foreach (int layer in layers)
            {
                mask |= 1 << layer;
            }
            return mask;
        }
    }
}
