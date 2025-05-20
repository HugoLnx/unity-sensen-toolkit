using System;
using UnityEngine;

namespace SensenToolkit
{
    public struct SimpleBounds
    {
        public Vector2 Min { get; private set; }
        public Vector2 Max { get; private set; }

        public SimpleBounds(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }

        public SimpleBounds Encapsulate(Vector2 point)
        {
            Min = Vector2.Min(Min, point);
            Max = Vector2.Max(Max, point);
            return this;
        }

        public SimpleBounds Encapsulate(SimpleBounds bounds)
        {
            Min = Vector2.Min(Min, bounds.Min);
            Max = Vector2.Max(Max, bounds.Max);
            return this;
        }

        public void Encapsulate(Bounds bounds)
        {
            Min = Vector2.Min(Min, bounds.min);
            Max = Vector2.Max(Max, bounds.max);
        }

        public Bounds ToBounds()
        {
            return new Bounds(Center, Size);
        }

        public Vector2 ClampVector(Vector2 vec)
        {
            return new Vector2(
                x: Mathf.Clamp(vec.x, Min.x, Max.x),
                y: Mathf.Clamp(vec.y, Min.y, Max.y)
            );
        }

        public SimpleBounds Scale(float v)
        {
            Vector2 center = Center;
            Vector2 size = Size * v;
            return new SimpleBounds(
                min: center - size * 0.5f,
                max: center + size * 0.5f
            );
        }

        public readonly Vector2 Center => (Min + Max) * 0.5f;
        public readonly Vector2 Size => new(
            Mathf.Abs(Max.x - Min.x),
            Mathf.Abs(Max.y - Min.y)
        );

        public static SimpleBounds operator +(SimpleBounds bounds, Vector2 offset)
        {
            return new SimpleBounds(bounds.Min + offset, bounds.Max + offset);
        }

        public static SimpleBounds operator -(SimpleBounds bounds, Vector2 offset)
        {
            return new SimpleBounds(bounds.Min - offset, bounds.Max - offset);
        }
    }
}
