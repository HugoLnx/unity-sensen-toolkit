using System;
using UnityEngine;

namespace SensenToolkit
{

    public struct Segment2DIntersectionResult
    {
        public bool IsIntersecting;
        public Vector2 ProjectionPoint;
    }

    public struct SimpleSegment2D
    {
        public Vector2 Position;
        public Vector2 Direction;
        public float LengthForward;
        public float LengthBackward;
        public float Length => LengthForward + LengthBackward;
        public readonly Vector2 Begin => Position - Direction * LengthBackward;
        public readonly Vector2 End => Position + Direction * LengthForward;
        public readonly Vector2 Center => float.IsInfinity(LengthForward) || float.IsInfinity(LengthBackward)
            ? Position
            : Position + Direction * ((LengthForward - LengthBackward) * 0.5f);
        public readonly bool IsInfiniteBothSides => float.IsInfinity(LengthForward) && float.IsInfinity(LengthBackward);
        public SimpleSegment2D(Vector2 begin, Vector2 end)
        {
            Assertx.IsDifferent(begin, end, "Segment cannot have the same begin and end");
            Position = begin;
            Vector2 vec = end - begin;
            LengthForward = vec.magnitude;
            LengthBackward = 0f;
            Direction = vec / LengthForward;
        }

        public SimpleSegment2D(Vector2 position, Vector2 direction, float lengthForward, float lengthBackward = 0f)
        {
            Assertx.IsNotZero(direction, "Direction cannot be zero");
            Assertx.IsTrue(Floatx.IsGreaterOrEqual(lengthForward, 0f), "LengthForward must be non-negative");
            Assertx.IsTrue(Floatx.IsGreaterOrEqual(lengthBackward, 0f), "LengthBackward must be non-negative");
            Position = position;
            Direction = direction;
            LengthForward = lengthForward;
            LengthBackward = lengthBackward;
        }

        public Segment2DIntersectionResult Intersect(SimpleSegment2D other)
        {
            Vector2 myPerp = new(-Direction.y, Direction.x);
            Vector2 toMyPosition = Position - other.Position;
            Vector2 projectionPoint = other.Position + other.Direction * (
                Vector2.Dot(myPerp, toMyPosition)
                    / Vector2.Dot(myPerp, other.Direction)
            );
            float projectionOfMyPosToPoint = Vector2.Dot(Direction, projectionPoint - Position);
            bool isIntersecting = Floatx.IsBetween(projectionOfMyPosToPoint, -LengthBackward, LengthForward);
            if (isIntersecting)
            {
                float projectionOfOtherPosToPoint = Vector2.Dot(other.Direction, projectionPoint - other.Position);
                isIntersecting = Floatx.IsBetween(projectionOfOtherPosToPoint, -other.LengthBackward, other.LengthForward);
            }
            return new Segment2DIntersectionResult
            {
                IsIntersecting = isIntersecting,
                ProjectionPoint = projectionPoint
            };
        }

        public bool Contains(Vector2 point)
        {
            Vector2 toPoint = point - Position;
            float projection = Vector2.Dot(Direction, toPoint);
            float distanceToLine = Mathf.Abs(Vector2.Dot(new Vector2(-Direction.y, Direction.x), toPoint));
            return Floatx.IsBetween(projection, -LengthBackward, LengthForward) && Floatx.IsLessOrEqual(distanceToLine, 0.01f);
        }

        public string BuildSpec()
        {
            return $"new {nameof(SimpleSegment2D)}(\n"
            + $"\tposition: new Vector2({Position.x}f, {Position.y}f),\n"
            + $"\tdirection: new Vector2({Direction.x}f, {Direction.y}f),\n"
            + $"\tlengthForward: {LengthForward}f,\n"
            + $"\tlengthBackward: {LengthBackward}f\n"
            + ")";
        }

        public SimpleSegment2D InverseDirection()
        {
            return new SimpleSegment2D(Position, -Direction, LengthBackward, LengthForward);
        }
    }
}
