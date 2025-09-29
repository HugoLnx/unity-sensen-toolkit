using UnityEngine;

namespace SensenToolkit
{
    public abstract class AUiBindingBaseAbstract : MonoBehaviour
    {
        protected abstract void BindUiChanges();
        protected abstract void UnbindUiChanges();
        public abstract void PushCurrentValueToUi();
    }
}
