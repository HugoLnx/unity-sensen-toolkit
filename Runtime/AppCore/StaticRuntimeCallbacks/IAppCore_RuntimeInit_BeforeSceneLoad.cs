using System;

namespace SensenToolkit
{
    public interface IAppCore_RuntimeInit_BeforeSceneLoad
    {
        // Can't use abstract static methods in interfaces yet, so we use this pattern
        // hopefully Unity'll support C#11 features in the future
        static void AppCore_RuntimeInit_BeforeSceneLoad() => throw new NotImplementedException();
    }
}
