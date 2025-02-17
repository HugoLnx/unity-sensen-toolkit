using System;
using UnityEngine;

namespace SensenToolkit
{
    public static class Gizmosx
    {
        public static readonly WithColor WithColorInstance = new();
        public static WithColor Color(Color color)
        {
            // TODO: Use a pool of WithColorInstance instead of a single instance
            // to avoid problem with multiple objects / coroutines using it
            return WithColorInstance.UseColor(color);
        }

        public static void DrawArrow(
            Vector3 from,
            Vector3 to,
            float extendBy = 1f,
            float headSizeModifier = 0.3f,
            float marginStart = 0f,
            float marginEnd = 0f
        )
        {
            Vector3 rawVector = to - from;
            to = from + (rawVector * extendBy);
            ApplyMarginsToLine(ref from, ref to, marginStart, marginEnd);
            Vector3 arrowVector = to - from;
            Gizmos.DrawLine(from, from + arrowVector);
            Vector3 arrowHeadVector = -arrowVector * headSizeModifier;
            const float Angle = 30f;
            Gizmos.DrawLine(to, to + Quaternion.Euler(0, 0, Angle) * arrowHeadVector);
            Gizmos.DrawLine(to, to + Quaternion.Euler(0, 0, -Angle) * arrowHeadVector);
            Gizmos.DrawLine(to, to + Quaternion.Euler(0, Angle, 0) * arrowHeadVector);
            Gizmos.DrawLine(to, to + Quaternion.Euler(0, -Angle, 0) * arrowHeadVector);
            Gizmos.DrawLine(to, to + Quaternion.Euler(Angle, 0, 0) * arrowHeadVector);
            Gizmos.DrawLine(to, to + Quaternion.Euler(-Angle, 0, 0) * arrowHeadVector);
        }

        private static void ApplyMarginsToLine(ref Vector3 from, ref Vector3 to, float marginStart, float marginEnd)
        {
            Vector3 vec = to - from;
            from += vec * marginStart;
            to -= vec * marginEnd;
        }

        public static void DebugDrawPointAsterist(Vector3 point, Color? color = null, float size = 0.2f, float duration = 0.5f)
        {
            color ??= UnityEngine.Color.green;
            Debug.DrawLine(point + Vector3.one * size, point - Vector3.one * size, color.Value, duration);
            Debug.DrawLine(point + new Vector3(1f, 1f, -1f) * size, point - new Vector3(1f, 1f, -1f) * size, color.Value, duration);
            Debug.DrawLine(point + new Vector3(1f, -1f, 1f) * size, point - new Vector3(1f, -1f, 1f) * size, color.Value, duration);
        }

        public static void DrawCircle(Vector3 center, float radius, Vector3 axis, int segments = 10)
        {
            Vector3 firstPoint = center + Quaternion.AngleAxis(360f / segments, axis) * (Vector3.right * radius);
            Vector3 previousPoint = firstPoint;
            for (int i = 1; i <= segments; i++)
            {
                Vector3 nextPoint = center + Quaternion.AngleAxis(360f / segments * i, axis) * (Vector3.right * radius);
                Gizmos.DrawLine(previousPoint, nextPoint);
                previousPoint = nextPoint;
            }
            Gizmos.DrawLine(previousPoint, firstPoint);
        }
    }
}
