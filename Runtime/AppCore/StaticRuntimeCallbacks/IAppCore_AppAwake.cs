using System;

namespace SensenToolkit
{
    public interface IAppCore_AppAwake
    {
        // Can't use abstract static methods in interfaces yet, so we use this pattern
        // hopefully Unity'll support C#11 features in the future
        static void AppCore_AppAwake() => throw new NotImplementedException();
    }
}
