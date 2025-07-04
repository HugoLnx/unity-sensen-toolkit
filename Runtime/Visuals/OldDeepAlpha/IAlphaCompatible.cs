using UnityEngine;

namespace SensenToolkit
{
    public interface IAlphaCompatible
    {
        float Alpha { get; set; }
        Transform Transform { get; }
    }
}
