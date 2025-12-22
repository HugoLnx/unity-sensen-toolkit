using System.Collections.Generic;
using SensenToolkit.InputRebinding.Data;

namespace SensenToolkit.InputRebinding.Internal
{
    public struct Vector2ListeningWizardResult
    {
        public BindingPlus NewBinding;
        public List<Vector2CompositionPartListeningResult> RawResults;
        public bool IsSingleBinding;
        public Vector2CompositionPartListeningResult? SingleCompositePartResult => IsSingleBinding ? RawResults[^1] : null;
        public bool IsSuccess;
    }
}
