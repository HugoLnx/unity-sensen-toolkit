using EasyButtons;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public abstract class AAlphaDeep<T> : MonoBehaviour
    where T : MonoBehaviour, ISingleAlpha
    {
        [SerializeField, AutoProperty(AutoPropertyMode.Children)]
        private T[] _allAlphas;
        [SerializeField]
        private T[] _extraAlphas = new T[0];
        [SerializeField, ReadOnly] private float _alpha = 1f;

        public float Alpha
        {
            get => _alpha;
            set
            {
                _alpha = value;
                RefreshAlphas();
            }
        }

        private void RefreshAlphas()
        {
            foreach (T alphaComponent in _allAlphas)
            {
                if (alphaComponent == null) continue;
                alphaComponent.Alpha = _alpha;
            }

            foreach (T alphaComponent in _extraAlphas)
            {
                if (alphaComponent == null) continue;
                alphaComponent.Alpha = _alpha;
            }
        }

        [Button]
        protected void SetAlpha(float alpha = 1f)
        {
            Alpha = alpha;
        }
    }
}
