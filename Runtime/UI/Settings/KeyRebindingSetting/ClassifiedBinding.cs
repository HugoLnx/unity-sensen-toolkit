using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace SensenToolkit
{
    public struct ClassifiedBinding
    {
        public InputBinding Binding;
        public string PathDeviceName;
        public string PathSubControlName;
        public string PathControlName;
        public bool IsKeyboardAndMouse;
        public bool IsKnownDevice;
        public string DeviceIdGroup;
        public string DisplayString;
        public string DeviceShortName;
        public int OrderIndex;
        public bool IsDefaultBinding;
        public bool IsComposite;
        public List<ClassifiedBinding> CompositeParts;

        public IEnumerable<ClassifiedBinding> EnumerateAllBindings()
        {
            yield return this;
            if (IsComposite && CompositeParts != null)
            {
                foreach (ClassifiedBinding part in CompositeParts)
                {
                    yield return part;
                }
            }
        }
    }
}
