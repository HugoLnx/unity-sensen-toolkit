using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SensenToolkit
{
    public sealed class Direction4
    {
        public enum Direction
        {
            Right,
            Down,
            Left,
            Up,
        }
        public static readonly Direction4 Right = new(Vector2.right, Direction.Right);
        public static readonly Direction4 Down = new(Vector2.down, Direction.Down);
        public static readonly Direction4 Left = new(Vector2.left, Direction.Left);
        public static readonly Direction4 Up = new(Vector2.up, Direction.Up);
        private static readonly List<(Direction, Direction4)> s_enumMap = new()
        {
            (Direction.Up, Up),
            (Direction.Right, Right),
            (Direction.Down, Down),
            (Direction.Left, Left),
        };

        private static readonly Dictionary<Direction4, Direction4> s_oppositeMap = new()
        {
            {Right, Left},
            {Down, Up},
            {Left, Right},
            {Up, Down}
        };

        private static readonly Dictionary<Direction4, (Direction4, Direction4)> s_perpendicularMap = new()
        {
            // DIRECTION (ANTICLOCKWISE, CLOCKWISE)
            {Up, (Left, Right)},
            {Right, (Up, Down)},
            {Down, (Right, Left)},
            {Left, (Down, Up)},
        };

        public static IEnumerable<Direction4> All
        {
            get
            {
                foreach ((_, Direction4 direction) in s_enumMap)
                {
                    yield return direction;
                }
            }
        }

        public float MyAxisCoordinate(Vector2 size)
            => IsHorizontal ? size.x : size.y;

        public float AltAxisCoordinate(Vector2 size)
            => IsHorizontal ? size.y : size.x;

        public Vector2 MyAxisOnly(Vector2 v)
            => IsHorizontal ? new Vector2(v.x, 0f) : new Vector2(0f, v.y);

        public Vector2 AltAxisOnly(Vector2 v)
            => IsHorizontal ? new Vector2(0f, v.y) : new Vector2(v.x, 0f);

        public Vector2 SetMyAxis(Vector2 v, float value)
            => IsHorizontal ? new Vector2(value, v.y) : new Vector2(v.x, value);

        public Vector2 Vector { get; }
        public Vector3 Vector3 { get; }
        public Vector2 AbsVector => new(Mathf.Abs(Vector.x), Mathf.Abs(Vector.y));
        public Vector3 AbsVector3 => new(Mathf.Abs(Vector3.x), Mathf.Abs(Vector3.y), Mathf.Abs(Vector3.z));
        public Vector2Int VectorInt { get; }
        public RectTransform.Axis RectTransformAxis { get; }
        public float Angle { get; }

        public Direction Enum { get; }
        public string Name { get; }

        public bool IsVertical => Vector.x == 0f;
        public bool IsHorizontal => Vector.y == 0f;
        public bool IsPositive => Vector.x > 0f || Vector.y > 0f;
        public bool IsNegative => Vector.x < 0f || Vector.y < 0f;
        public bool IsUp => this == Up;
        public bool IsDown => this == Down;
        public bool IsRight => this == Right;
        public bool IsLeft => this == Left;
        public Direction4 Opposite => s_oppositeMap[this];
        public Direction4 PerpendicularAntiClockwise => s_perpendicularMap[this].Item1;
        public Direction4 PerpendicularClockwise => s_perpendicularMap[this].Item2;
        public float MyCoordinateValue => IsHorizontal ? Vector.x : Vector.y;

        private Direction4(Vector2 vector, Direction directionEnum)
        {
            this.Vector = vector;
            this.Vector3 = vector;
            this.VectorInt = vector.AsVector2Int();
            this.Angle = vector.Angle();
            this.Enum = directionEnum;
            this.Name = System.Enum.GetName(typeof(Direction), directionEnum);
            this.RectTransformAxis = IsHorizontal ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical;
        }

        public static Direction4 FromEnum(Direction v)
        {
            return v switch
            {
                Direction.Right => Right,
                Direction.Down => Down,
                Direction.Left => Left,
                Direction.Up => Up,
                _ => throw new System.Exception("Unrecognized Direction"),
            };
        }

        public static Direction4 FromVector(Vector2 v)
        {
            v = v.normalized;
            if (v == Vector2.right) return Right;
            else if (v == Vector2.down) return Down;
            else if (v == Vector2.left) return Left;
            else if (v == Vector2.up) return Up;
            throw new System.Exception($"Can't recognize direction on vector {v}");
        }
    }
}
