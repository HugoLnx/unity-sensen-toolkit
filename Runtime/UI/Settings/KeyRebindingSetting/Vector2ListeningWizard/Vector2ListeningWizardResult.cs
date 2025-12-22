using System.Collections.Generic;

namespace SensenToolkit
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
