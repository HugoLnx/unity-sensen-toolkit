using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    [CreateAssetMenu(menuName = "Sensen/ValueMirroring/BoolValueMirroring")]
    public class BoolValueMirroringSO : ScriptableObject, IScriptableCallbackSubscriber_OnAppAwake
    {
        [SerializeField, MustBeAssigned] private BoolValueSO _source;
        [SerializeField, MustBeAssigned] private BoolValueSO[] _targets;
        [SerializeField] private bool _invert = false;

        public void ScriptableCallback_OnAppAwake()
        {
            _source.AddSyncListener(OnSourceValueChanged);
        }

        private void OnSourceValueChanged(BoolValueSO _)
        {
            foreach (BoolValueSO target in _targets)
            {
                target.Value = _invert ? !_source.Value : _source.Value;
            }
        }
    }
}
