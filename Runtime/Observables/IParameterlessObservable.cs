using System;

namespace SensenToolkit.Observables
{
    public interface IParameterlessObservable
    {
        event Action Callbacks;
    }
}
