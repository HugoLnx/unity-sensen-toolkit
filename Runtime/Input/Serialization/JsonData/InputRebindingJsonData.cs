using System.Collections.Generic;

namespace SensenToolkit.InputRebinding.Internal
{
    [System.Serializable]
    public class InputRebindingJsonData
    {
        public List<InputBindingJson> BindingDeletions = new();
        public List<InputBindingJson> BindingAdditions = new();
        public List<InputActionJson> Actions = new();
        public List<InputActionMapJson> ActionMaps = new();
        public bool IsEmpty => BindingDeletions.Count == 0 && BindingAdditions.Count == 0;
    }
}
