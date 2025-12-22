using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace SensenToolkit.InputRebindingSerialization
{
    [System.Serializable]
    internal class InputRebindingJsonData
    {
        public List<InputBindingJson> BindingDeletions = new();
        public List<InputBindingJson> BindingAdditions = new();
        public List<InputActionJson> Actions = new();
        public List<InputActionMapJson> ActionMaps = new();
        public bool IsEmpty => BindingDeletions.Count == 0 && BindingAdditions.Count == 0;
    }
}
