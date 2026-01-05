using System;

namespace SensenToolkit.Internal
{
    public interface IAppCore_RuntimeInit_SubsystemRegistration_Internal
    {
        // Can't use abstract static methods in interfaces yet, so we use this pattern
        // hopefully Unity'll support C#11 features in the future
        static void AppCore_RuntimeInit_SubsystemRegistration_Internal() => throw new NotImplementedException();
    }
}

namespace SensenToolkit
{
    public interface IAppCore_RuntimeInit_SubsystemRegistration
    {
        // Can't use abstract static methods in interfaces yet, so we use this pattern
        // hopefully Unity'll support C#11 features in the future
        static void AppCore_RuntimeInit_SubsystemRegistration() => throw new NotImplementedException();
    }
}
