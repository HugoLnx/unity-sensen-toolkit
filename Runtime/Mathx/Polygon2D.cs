using System;
using System.Collections.Generic;
using System.Linq;
using SensenToolkit.Internal;
using UnityEngine;

namespace SensenToolkit
{
    public class Polygon2D
    {
        public Vector2[] Vertices { get; private set; }
        public SimpleSegment2D[] Segments { get; }
        public HashSet<Vector2> VerticesSet { get; }
        public SimpleBounds Bounds { get; }

        public Polygon2D(Vector2[] vertices)
        {
            Assertx.IsTrue(vertices.Length >= 3, "Polygon2D must have at least 3 vertices");
            Vertices = vertices;
            Segments = new SimpleSegment2D[vertices.Length];
            VerticesSet = new HashSet<Vector2>(vertices);
            Bounds = new SimpleBounds(min: vertices[0], max: vertices[0]);
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 begin = vertices[i];
                Vector2 end = vertices[(i + 1) % vertices.Length];
                Segments[i] = new SimpleSegment2D(begin, end);
                Bounds = Bounds.Encapsulate(begin);
            }
        }

        // Cast point to one of the segments of the polygon, if it hits a pair number of segments,
        // it is outside the polygon
        public bool Contains(Vector2 point)
        {
            if (VerticesSet.Contains(point)) return true;
            Vector2 dir = (Segments[0].Center - point).normalized;
            HashSet<Vector2> intersectionPoints = new();
            foreach (SimpleSegment2D segment in Segments)
            {
                if (segment.Contains(point)) return true;

                SimpleSegment2D testSegment = new(
                    position: point,
                    direction: dir,
                    lengthForward: Mathf.Infinity
                );
                Segment2DIntersectionResult intersectResult = testSegment.Intersect(segment);
                if (intersectResult.IsIntersecting)
                {
                    intersectionPoints.Add(intersectResult.ProjectionPoint.RoundComponents(3));
                }
            }
            bool hasHitOddNumberOfSegments = intersectionPoints.Count % 2 == 1;
            return hasHitOddNumberOfSegments;
        }
        public static Polygon2D BuildRegular(int vertexCount, float radius)
        {
            var vertices = new Vector2[vertexCount];
            float angleStep = 360f / vertexCount;
            for (int i = 0; i < vertexCount; i++)
            {
                float angle = angleStep * i;
                vertices[i] = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius
                );
            }
            return new Polygon2D(vertices);
        }
    }
}
