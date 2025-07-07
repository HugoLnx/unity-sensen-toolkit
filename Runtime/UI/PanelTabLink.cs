#if DOTWEEN
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    [System.Serializable]
    public class PanelTabLink : MonoBehaviour
    {
        [field: SerializeField, AutoProperty]
        public RadioButton Button { get; private set; }

        [field: SerializeField, MustBeAssigned]
        public PanelFadable Panel { get; private set; }
    }
}
#endif
