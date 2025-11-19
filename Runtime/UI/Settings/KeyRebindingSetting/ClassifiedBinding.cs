using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace SensenToolkit
{
    public class ClassifiedBinding
    {
        public InputBinding Binding;
        public string PathDeviceName;
        public string PathSubControlName;
        public string PathControlName;
        public bool IsKeyboardAndMouse;
        public bool IsKnownDevice;
        public string DeviceIdGroup;
        public string DeviceShortName;
        public int OrderIndex;
        public bool IsDefaultBinding;
        public bool IsComposite;
        public List<ClassifiedBinding> CompositeParts;
        public ClassifiedBinding ParentComposite;

        private string _displayString;
        public string DisplayString => _displayString ??= BindingDisplayStringUtils.GenerateDisplayStringFor(this);
        private string _displayStringShortenedForComposite;
        public string DisplayStringShortenedForComposite => _displayStringShortenedForComposite
            ??= BindingDisplayStringUtils.GenerateDisplayStringFor(this, shortenForComposite: true);

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
