using System.Net.NetworkInformation;
using UnityEngine;

namespace SensenToolkit
{
    public static class Vector3ExtensionMethods
    {
        public static Vector2 XZ(this Vector3 v3)
        {
            return new Vector2(v3.x, v3.z);
        }
        public static Vector3 X0Z(this Vector3 v3)
        {
            return new Vector3(v3.x, 0, v3.z);
        }
        public static Vector3 XYZ(this Vector3 v3)
        {
            return new Vector3(v3.x, v3.y, v3.z);
        }
        public static Vector3 XZY(this Vector3 v3)
        {
            return new Vector3(v3.x, v3.z, v3.y);
        }
        public static Vector2 XY(this Vector3 v3)
        {
            return (Vector2)v3;
        }

        public static Vector3 Abs(this Vector3 v3)
        {
            return new Vector3(Mathf.Abs(v3.x), Mathf.Abs(v3.y), Mathf.Abs(v3.z));
        }

        public static Vector3Int AsVector3Int(this Vector3 v3)
        {
            return new Vector3Int(
                Mathf.RoundToInt(v3.x),
                Mathf.RoundToInt(v3.y),
                Mathf.RoundToInt(v3.z)
            );
        }

        public static Vector3 Clone(this Vector3 v3, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(
                x ?? v3.x,
                y ?? v3.y,
                z ?? v3.z
            );
        }

        public static Vector3 SetOnly(this Vector3 v3, float? x = null, float? y = null, float? z = null)
        {
            v3.Set(
                x ?? v3.x,
                y ?? v3.y,
                z ?? v3.z
            );
            return v3;
        }

        public static Vector3 With(this Vector3 v3, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(
                x ?? v3.x,
                y ?? v3.y,
                z ?? v3.z
            );
        }

        public static Vector3 FindAPerpendicular(this Vector3 v3)
        {
            if (v3.x == 0) return new(0, -v3.z, v3.y);
            if (v3.y == 0) return new(-v3.z, 0, v3.x);
            if (v3.z == 0) return new(-v3.y, v3.x, 0);
            float mag = v3.magnitude;
            return Vector3.Cross(v3, v3.With(x: v3.x + 1f)).normalized * mag;
        }
    }
}
