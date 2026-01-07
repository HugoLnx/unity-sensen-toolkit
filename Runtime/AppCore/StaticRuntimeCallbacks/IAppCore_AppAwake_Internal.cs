using System;

namespace SensenToolkit.Internal
{
    public interface IAppCore_AppAwake_Internal
    {
        // Can't use abstract static methods in interfaces yet, so we use this pattern
        // hopefully Unity'll support C#11 features in the future
        static void AppCore_AppAwake_Internal() => throw new NotImplementedException();
    }
}
