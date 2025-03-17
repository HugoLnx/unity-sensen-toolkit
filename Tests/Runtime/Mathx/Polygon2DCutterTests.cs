using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SensenToolkit;
using SensenToolkit.Internal;
using UnityEngine;
using UnityEngine.TestTools.Utils;

public class Polygon2DCutterTests
{
    private static readonly Vector2EqualityComparer s_v2Comparer = new(10e-2f);

    [Test]
    public void CutASquareThrough()
    {
        Polygon2D square = new(new Vector2[]
        {
            new(-1f, 1f),
            new(1f, 1f),
            new(1f, -1f),
            new(-1f, -1f)
        });
        SimpleSegment2D cutSegment = new(
            position: new(-0.5f, 0f),
            direction: Vector2.down,
            lengthForward: Mathf.Infinity,
            lengthBackward: Mathf.Infinity
        );

        Polygon2D leftSide = new(new Vector2[]
        {
            new(-1f, 1f),
            new(-0.5f, 1f),
            new(-0.5f, -1f),
            new(-1f, -1f),
        });

        Polygon2D rightSide = new(new Vector2[]
        {
            new(-0.5f, 1f),
            new(1f, 1f),
            new(1f, -1f),
            new(-0.5f, -1f),
        });
        AssertMultipleCutWaysResults(
            square,
            cutSegment,
            expectedSideA: new() { leftSide },
            expectedSideB: new() { rightSide }
        );
    }

    [Test]
    public void OneSideIsTheWholeSquare_When_CutASquaresRightEdge()
    {
        Polygon2D square = new(new Vector2[]
        {
            new(-1f, 1f),
            new(1f, 1f),
            new(1f, -1f),
            new(-1f, -1f)
        });
        SimpleSegment2D cutSegment = new(
            position: new(1f, 0f),
            direction: Vector2.down,
            lengthForward: Mathf.Infinity,
            lengthBackward: Mathf.Infinity
        );

        AssertMultipleCutWaysResults(
            square,
            cutSegment,
            expectedSideA: new() { square },
            expectedSideB: new()
        );
    }

    [Test]
    public void OneSideIsTheWholeSquare_When_CutASquaresLeftEdge()
    {
        Polygon2D square = new(new Vector2[]
        {
            new(-1f, 1f),
            new(1f, 1f),
            new(1f, -1f),
            new(-1f, -1f)
        });
        SimpleSegment2D cutSegment = new(
            position: new(-1f, 0f),
            direction: Vector2.down,
            lengthForward: Mathf.Infinity,
            lengthBackward: Mathf.Infinity
        );

        AssertMultipleCutWaysResults(
            square,
            cutSegment,
            expectedSideA: new(),
            expectedSideB: new() { square }
        );
    }


    [Test]
    public void ExampleThatShouldNotEnterAnEndlessLoop()
    {
        Polygon2D polygon = new(new Vector2[]
        {
            new(-0.55f, 0.49f),
            new(-0.23f, -0.31f),

            new(0.54f, 0.31f),
            new(0.84f, 0.44f),
            new(0.63f, 1.03f),
        });
        float angle = Mathf.Deg2Rad * 30f;
        SimpleSegment2D cutSegment = new(
            position: new(0f, 0f),
            direction: new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)),
            lengthForward: Mathf.Infinity,
            lengthBackward: Mathf.Infinity
        );

        List<Polygon2D> sideA = new();
        sideA.Add(new(new Vector2[]
        {
            new(-0.29f, -0.17f),
            new(-0.23f, -0.31f),
            new(0.54f, 0.31f),
        }));

        sideA.Add(new(new Vector2[]
        {
            new(0.54f, 0.31f),
            new(0.84f, 0.44f),
            new(0.83f, 0.48f),
        }));

        List<Polygon2D> sideB = new();
        sideB.Add(new(new Vector2[]
        {
            new(-0.55f, 0.49f),
            new(0.63f, 1.03f),
            new(0.83f, 0.48f),
            new(0.54f, 0.31f),
            new(-0.29f, -0.17f),
        }));

        AssertMultipleCutWaysResults(polygon, cutSegment, sideA, sideB);
    }

    [Test]
    public void CutPolygonWithHoleAtLeft()
    {
        //      |
        //   +--x--+
        //    \    |
        //      x  |
        //     /   |
        //   +--x--+
        //      |
        //     \ /
        Polygon2D polygon = new Polygon2DEasyBuilder()
            .WorldPoint(Vector2.zero)
            .NextPoint(Vector2.down + Vector2.right)
            .NextPoint(Vector2.down + Vector2.left)
            .NextPoint(Vector2.right * 2f)
            .WorldPoint(new Vector2(2f, 0f))
            .Build();

        SimpleSegment2D cutSegment = new(
            position: new(1f, 0f),
            direction: Vector3.down,
            lengthForward: Mathf.Infinity,
            lengthBackward: Mathf.Infinity
        );

        List<Polygon2D> sideA = new();
        sideA.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(0f, 0f))
            .NextPoint(Vector2.down + Vector2.right)
            .NextPoint(Vector2.up)
            .Build()
        );
        sideA.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(1f, -1f))
            .NextPoint(Vector2.down + Vector2.left)
            .NextPoint(Vector2.right)
            .Build()
        );

        List<Polygon2D> sideB = new();
        sideB.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(1f, 0f))
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .WorldPoint(new Vector2(2f, 0f))
            .Build()
        );

        AssertMultipleCutWaysResults(
            polygon,
            cutSegment,
            expectedSideA: sideA,
            expectedSideB: sideB
        );
    }

    [Test]
    public void CutPolygonWithHoleAtRight()
    {
        //     / \
        //      |
        //   +--x--+
        //   |  | /
        //   |  x
        //   |  | \
        //   +--x--+
        //      |
        Polygon2D polygon = new Polygon2DEasyBuilder()
            .WorldPoint(Vector2.zero)
            .NextPoint(Vector2.right * 2f)
            .NextPoint(Vector2.down + Vector2.left)
            .NextPoint(Vector2.down + Vector2.right)
            .WorldPoint(new Vector2(0f, -2f))
            .Build();

        SimpleSegment2D cutSegment = new(
            position: new(1f, 0f),
            direction: Vector3.up,
            lengthForward: Mathf.Infinity,
            lengthBackward: Mathf.Infinity
        );

        List<Polygon2D> sideA = new();
        sideA.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(1f, -1f))
            .NextPoint(Vector2.down + Vector2.right)
            .NextPoint(Vector2.left)
            .Build()
        );
        sideA.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(1f, 0f))
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.down + Vector2.left)
            .Build()
        );

        List<Polygon2D> sideB = new();
        sideB.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(0f, 0f))
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.down)
            .WorldPoint(new Vector2(0f, -2f))
            .Build()
        );

        AssertMultipleCutWaysResults(
            polygon,
            cutSegment,
            expectedSideA: sideA,
            expectedSideB: sideB
        );
    }


    [Test]
    public void OneSideIsTheWholePolygon_When_MissesThePolygonAtRight()
    {
        Polygon2D square = new(new Vector2[]
        {
            new(-1f, 1f),
            new(1f, 1f),
            new(1f, -1f),
            new(-1f, -1f)
        });
        SimpleSegment2D cutSegment = new(
            position: new(10f, 0f),
            direction: Vector2.down,
            lengthForward: Mathf.Infinity,
            lengthBackward: Mathf.Infinity
        );

        AssertMultipleCutWaysResults(
            square,
            cutSegment,
            expectedSideA: new() { square },
            expectedSideB: new()
        );
    }


    [Test]
    public void OneSideIsTheWholePolygon_When_MissesThePolygonAtLeft()
    {
        Polygon2D square = new(new Vector2[]
        {
            new(-1f, 1f),
            new(1f, 1f),
            new(1f, -1f),
            new(-1f, -1f)
        });
        SimpleSegment2D cutSegment = new(
            position: new(10f, 0f),
            direction: Vector2.up,
            lengthForward: Mathf.Infinity,
            lengthBackward: Mathf.Infinity
        );

        AssertMultipleCutWaysResults(
            square,
            cutSegment,
            expectedSideA: new(),
            expectedSideB: new() { square }
        );
    }

    [Test]
    public void CutAPolygonWithTwoHills()
    {
        //      |
        //      x--+
        //      |  |
        //   +--x  |
        //   |     |
        //   +--x  |
        //      |  |
        //   +--x  |
        //   |     |
        //   +--x  |
        //      |  |
        //      x--+
        //      |
        //     \ /
        Polygon2D polygon = new Polygon2DEasyBuilder()
            .WorldPoint(Vector2.zero)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.left)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.left)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .WorldPoint(new Vector2(1f, 0f))
            .Build();

        List<Polygon2D> sideA = new();
        sideA.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(-1f, -1f))
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.up)
            .Build()
        );
        sideA.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(-1f, -3f))
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.up)
            .Build()
        );

        List<Polygon2D> sideB = new();
        sideB.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(0f, 0f))
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.up * 5f)
            .Build()
        );

        SimpleSegment2D cutSegment = new(
            position: new(0f, 0f),
            direction: Vector2.down,
            lengthForward: Mathf.Infinity,
            lengthBackward: Mathf.Infinity
        );

        AssertMultipleCutWaysResults(polygon, cutSegment, sideA, sideB);
    }

    [Test]
    public void CutAPolygonWithTwoHoles()
    {
        //      |
        //         +--+
        //         |  |
        //      x--+  |
        //      |     |
        //      x--+  |
        //         |  |
        //      x--+  |
        //      |     |
        //      x--+  |
        //         |  |
        //   +--x--+  |
        //   |        |
        //   +--x-----+
        //      |
        //     \ /
        Polygon2D polygon = new Polygon2DEasyBuilder()
            .WorldPoint(Vector2.zero)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.left)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.left)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.left * 2f)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right * 3f)
            .WorldPoint(new Vector2(1f, 0f))
            .Build();

        SimpleSegment2D cutSegment = new(
            position: new(-1f, 0f),
            direction: Vector2.down,
            lengthForward: Mathf.Infinity,
            lengthBackward: Mathf.Infinity
        );

        List<Polygon2D> sideA = new();
        sideA.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(-2f, -5f))
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.up)
            .Build()
        );

        List<Polygon2D> sideB = new();
        sideB.Add(new Polygon2DEasyBuilder()
            .WorldPoint(Vector2.zero)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.left)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.left)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.left)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right * 2f)
            .WorldPoint(new Vector2(1f, 0f))
            .Build()
        );

        AssertMultipleCutWaysResults(polygon, cutSegment, sideA, sideB);
    }

    [Test]
    public void CutAPolygonWithBigHole()
    {
        //      |
        //   +--x-----+
        //   |        |
        //   +--x--+  |
        //         |  |
        //      x--+  |
        //      |     |
        //   +--x     |
        //   |        |
        //   +--x-----+
        //      |
        //     \ /
        Polygon2D polygon = new Polygon2DEasyBuilder()
            .WorldPoint(Vector2.zero)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right * 2f)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.left)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.left)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right * 3f)
            .WorldPoint(new Vector2(3f, 0f))
            .Build();

        SimpleSegment2D cutSegment = new(
            position: new(1f, 0f),
            direction: Vector2.down,
            lengthForward: Mathf.Infinity,
            lengthBackward: Mathf.Infinity
        );

        List<Polygon2D> sideA = new();
        sideA.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(0f, 0f))
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.up)
            .Build()
        );
        sideA.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(0f, -3f))
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.up)
            .Build()
        );

        List<Polygon2D> sideB = new();
        sideB.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(1f, 0f))
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.left)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right * 2f)
            .WorldPoint(new Vector2(3f, 0f))
            .Build()
        );

        AssertMultipleCutWaysResults(polygon, cutSegment, sideA, sideB);
    }


    [Test]
    public void CutAPolygonWithCorners()
    {
        //      |
        //      x-----+
        //       \    |
        //   +--x--+  |
        //   |        |
        //   +--x--+  |
        //        /   |
        //      x     |
        //    /       |
        //   +        |
        //    \       |
        //      x     |
        //        \   |
        //   +--x--+  |
        //   |        |
        //   +--x--+  |
        //        /   |
        //      x-----+
        //      |
        //     \ /
        Polygon2D polygon = new Polygon2DEasyBuilder()
            .WorldPoint(Vector2.zero)
            .NextPoint(Vector2.down + Vector2.right)
            .NextPoint(Vector2.left * 2f)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right * 2f)
            .NextPoint(Vector2.down * 2f + Vector2.left * 2f)
            .NextPoint(Vector2.down * 2f + Vector2.right * 2f)
            .NextPoint(Vector2.left * 2f)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right * 2f)
            .NextPoint(Vector2.down + Vector2.left)
            .NextPoint(Vector2.right * 2f)
            .WorldPoint(new Vector2(2f, 0f))
            .Build();

        SimpleSegment2D cutSegment = new(
            position: new(0f, 0f),
            direction: Vector2.down,
            lengthForward: Mathf.Infinity,
            lengthBackward: Mathf.Infinity
        );

        List<Polygon2D> sideA = new();
        sideA.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(-1f, -1f))
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.up)
            .Build()
        );
        sideA.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(0f, -3f))
            .NextPoint(Vector2.down + Vector2.left)
            .NextPoint(Vector2.down + Vector2.right)
            .Build()
        );
        sideA.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(-1f, -6f))
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.up)
            .Build()
        );

        List<Polygon2D> sideB = new();
        sideB.Add(new Polygon2DEasyBuilder()
            .WorldPoint(Vector2.zero)
            .NextPoint(Vector2.down + Vector2.right)
            .NextPoint(Vector2.left)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.down + Vector2.left)
            .NextPoint(Vector2.down * 2f)
            .NextPoint(Vector2.down + Vector2.right)
            .NextPoint(Vector2.left)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.down + Vector2.left)
            .NextPoint(Vector2.right * 2f)
            .WorldPoint(new Vector2(2f, 0f))
            .Build()
        );

        AssertMultipleCutWaysResults(polygon, cutSegment, sideA, sideB);
    }

    [Test]
    public void CutAPolygonThatStartsOnSideBAndFinishesOnSideA()
    {
        //
        //      |
        //      x--+
        //      |  |
        //   +--x  |
        //   |     |
        //   +--x  |
        //      |  |
        //   +--x  |
        //   |     |
        //   |  x--+
        //   |  |
        //   +--x
        //      |
        //     \ /
        //
        Polygon2D polygon = new Polygon2DEasyBuilder()
            .WorldPoint(Vector2.zero)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.left)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.left)
            .NextPoint(Vector2.down * 2f)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.up)
            .NextPoint(Vector2.right)
            .WorldPoint(new Vector2(1f, 0f))
            .Build();

        SimpleSegment2D cutSegment = new(
            position: new(0f, 0f),
            direction: Vector2.down,
            lengthForward: Mathf.Infinity,
            lengthBackward: Mathf.Infinity
        );

        List<Polygon2D> sideA = new();
        sideA.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(-1f, -1f))
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.up)
            .Build()
        );
        sideA.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(-1f, -3f))
            .NextPoint(Vector2.down * 2f)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.up)
            .NextPoint(Vector2.up)
            .Build()
        );

        List<Polygon2D> sideB = new();
        sideB.Add(new Polygon2DEasyBuilder()
            .WorldPoint(Vector2.zero)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .WorldPoint(new Vector2(1f, 0f))
            .Build()
        );

        AssertMultipleCutWaysResults(polygon, cutSegment, sideA, sideB);
    }


    [Test]
    public void CutADoubleSpiralPolygon()
    {
        //               |
        //   +-----------x-----------+
        //   |                       |
        //   |  +--------x--------+  |
        //   |  |                 |  |
        //   |  |  +-----x-----+  |  |
        //   |  |  |           |  |  |
        //   |  |  |  +--x--+  |  |  |
        //   |  |  |  |     |  |  |  |
        //   |  |  |  +--x  |  |  |  |
        //   |  |  |     |  |  |  |  |
        //   |  |  +-----x  |  |  |  |
        //   |  |           |  |  |  |
        //   |  +--------x--+  |  |  |
        //   |                 |  |  |
        //   +-----------x-----+  |  |
        //                        |  |
        //   +-----------x-----+  |  |
        //   |                 |  |  |
        //   |  +--------x--+  |  |  |
        //   |  |           |  |  |  |
        //   |  |  +-----x  |  |  |  |
        //   |  |  |     |  |  |  |  |
        //   |  |  |  +--x  |  |  |  |
        //   |  |  |  |     |  |  |  |
        //   |  |  |  +--x--+  |  |  |
        //   |  |  |           |  |  |
        //   |  |  +-----x-----+  |  |
        //   |  |                 |  |
        //   |  +--------x--------+  |
        //   |                       |
        //   +-----------x-----------+
        //               |
        //              \ /
        //
        Polygon2D polygon = new Polygon2DEasyBuilder()
            .WorldPoint(Vector2.zero)
            .NextPoint(Vector2.down * 7f)
            .NextPoint(Vector2.right * 6f)
            .NextPoint(Vector2.up * 5f)
            .NextPoint(Vector2.left * 4f)
            .NextPoint(Vector2.down * 3f)
            .NextPoint(Vector2.right * 2f)
            .NextPoint(Vector2.up)
            .NextPoint(Vector2.left)
            .NextPoint(Vector2.up)
            .NextPoint(Vector2.right * 2f)
            .NextPoint(Vector2.down * 3f)
            .NextPoint(Vector2.left * 4f)
            .NextPoint(Vector2.up * 5f)
            .NextPoint(Vector2.right * 6f)
            .NextPoint(Vector2.down * 13f)
            .NextPoint(Vector2.left * 6f)
            .NextPoint(Vector2.up * 5f)
            .NextPoint(Vector2.right * 4f)
            .NextPoint(Vector2.down * 3f)
            .NextPoint(Vector2.left * 2f)
            .NextPoint(Vector2.up)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.up)
            .NextPoint(Vector2.left * 2f)
            .NextPoint(Vector2.down * 3f)
            .NextPoint(Vector2.right * 4f)
            .NextPoint(Vector2.up * 5f)
            .NextPoint(Vector2.left * 6f)
            .NextPoint(Vector2.down * 7f)
            .NextPoint(Vector2.right * 8f)
            .WorldPoint(new Vector2(8f, 0f))
            .Build();

        SimpleSegment2D cutSegment = new(
            position: new(4f, 0f),
            direction: Vector2.down,
            lengthForward: Mathf.Infinity,
            lengthBackward: Mathf.Infinity
        );
        List<Polygon2D> sideA = new();
        sideA.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(0f, 0f))
            .NextPoint(Vector2.down * 7f)
            .NextPoint(Vector2.right * 4f)
            .NextPoint(Vector2.up)
            .NextPoint(Vector2.left * 3f)
            .NextPoint(Vector2.up * 5f)
            .NextPoint(Vector2.right * 3f)
            .NextPoint(Vector2.up)
            .Build()
        );
        sideA.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(2f, -2f))
            .NextPoint(Vector2.down * 3f)
            .NextPoint(Vector2.right * 2f)
            .NextPoint(Vector2.up)
            .NextPoint(Vector2.left)
            .NextPoint(Vector2.up)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.up)
            .Build()
        );

        sideA.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(0f, -8f))
            .NextPoint(Vector2.down * 7f)
            .NextPoint(Vector2.right * 4f)
            .NextPoint(Vector2.up)
            .NextPoint(Vector2.left * 3f)
            .NextPoint(Vector2.up * 5f)
            .NextPoint(Vector2.right * 3f)
            .NextPoint(Vector2.up)
            .Build()
        );
        sideA.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(2f, -10f))
            .NextPoint(Vector2.down * 3f)
            .NextPoint(Vector2.right * 2f)
            .NextPoint(Vector2.up)
            .NextPoint(Vector2.left)
            .NextPoint(Vector2.up)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.up)
            .Build()
        );

        List<Polygon2D> sideB = new();
        sideB.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(4f, 0f))
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right * 3f)
            .NextPoint(Vector2.down * 13f)
            .NextPoint(Vector2.left * 3f)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right * 4f)
            .NextPoint(Vector2.up * 15f)
            .Build()
        );

        sideB.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(4f, -2f))
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.down * 3f)
            .NextPoint(Vector2.left)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right * 2f)
            .NextPoint(Vector2.up * 5f)
            .Build()
        );
        sideB.Add(new Polygon2DEasyBuilder()
            .WorldPoint(new Vector2(4f, -8f))
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right)
            .NextPoint(Vector2.down * 3f)
            .NextPoint(Vector2.left)
            .NextPoint(Vector2.down)
            .NextPoint(Vector2.right * 2f)
            .NextPoint(Vector2.up * 5f)
            .Build()
        );

        AssertCutResult(polygon, cutSegment, sideA, sideB);
    }
    private void AssertMultipleCutWaysResults(
        Polygon2D polygon,
        SimpleSegment2D cutSegment,
        List<Polygon2D> expectedSideA,
        List<Polygon2D> expectedSideB
    )
    {
        Polygon2D invPolygon = polygon.InverseVertexOrder();
        SimpleSegment2D invCutSegment = cutSegment.InverseDirection();
        var invExpectedSideA = expectedSideA.Select(p => p).Reverse().ToList();
        var invExpectedSideB = expectedSideB.Select(p => p).Reverse().ToList();

        AssertCutResult(polygon, cutSegment, expectedSideA, expectedSideB);
        AssertCutResult(polygon, invCutSegment, invExpectedSideB, invExpectedSideA);

        AssertCutResult(invPolygon, cutSegment, expectedSideA, expectedSideB);
        AssertCutResult(invPolygon, invCutSegment, invExpectedSideB, invExpectedSideA);
    }

    private void AssertCutResult(
        Polygon2D polygon,
        SimpleSegment2D cutSegment,
        List<Polygon2D> expectedSideA,
        List<Polygon2D> expectedSideB
    )
    {
        Polygon2DCutter cutter = new(polygon, cutSegment.Position, cutSegment.Direction);

        (List<Polygon2D> sideA, List<Polygon2D> sideB) result = cutter.Execute();
        Assert.That(result.sideA.Count, Is.EqualTo(expectedSideA.Count));
        Assert.That(result.sideB.Count, Is.EqualTo(expectedSideB.Count));

        if (expectedSideA.Count > 0)
        {
            for (int i = 0; i < expectedSideA.Count; i++)
            {
                AssertPolygonIsEqualTo(result.sideA[i], expectedSideA[i], $"Side A polygon {i} mismatch");
            }
        }

        if (expectedSideB.Count > 0)
        {
            for (int i = 0; i < expectedSideB.Count; i++)
            {
                AssertPolygonIsEqualTo(result.sideB[i], expectedSideB[i], $"Side B polygon {i} mismatch");
            }
        }
    }

    private void AssertPolygonIsEqualTo(Polygon2D actual, Polygon2D expected, string message = null)
    {
        string messagePrefix = message != null ? $"{message}: " : "";
        Assert.That(actual.Vertices.Length, Is.EqualTo(expected.Vertices.Length), $"{messagePrefix}Vertices count mismatch");
        int actualStartIndex = -1;
        for (int i = 0; i < expected.Vertices.Length; i++)
        {
            if (s_v2Comparer.Equals(expected.Vertices[0], actual.Vertices[i]))
            {
                actualStartIndex = i;
                break;
            }
        }

        if (actualStartIndex == -1)
        {
            Assert.Fail($"{messagePrefix}Could not find the first vertex of the expected polygon in the actual polygon");
        }

        bool isEqualBackwards = true;
        int backwardsIndexSimilarityDistance = 0;
        for (int i = 0; i < expected.Vertices.Length; i++)
        {
            int actualIndex = (actualStartIndex + actual.Vertices.Length - i) % actual.Vertices.Length;
            if (!s_v2Comparer.Equals(expected.Vertices[i], actual.Vertices[actualIndex]))
            {
                isEqualBackwards = false;
                break;
            }
            backwardsIndexSimilarityDistance++;
        }
        if (isEqualBackwards) return;

        bool isEqualForward = true;
        int forwardIndexSimilarityDistance = 0;
        for (int i = 0; i < expected.Vertices.Length; i++)
        {
            int actualIndex = (actualStartIndex + i) % actual.Vertices.Length;
            if (!s_v2Comparer.Equals(expected.Vertices[i], actual.Vertices[actualIndex]))
            {
                isEqualForward = false;
                break;
            }
            forwardIndexSimilarityDistance++;
        }
        if (isEqualForward) return;

        int actualCompIndex = forwardIndexSimilarityDistance >= backwardsIndexSimilarityDistance
            ? (actualStartIndex + forwardIndexSimilarityDistance) % actual.Vertices.Length
            : (actualStartIndex - backwardsIndexSimilarityDistance + actual.Vertices.Length) % actual.Vertices.Length;
        int expectedCompIndex = forwardIndexSimilarityDistance >= backwardsIndexSimilarityDistance
            ? forwardIndexSimilarityDistance
            : backwardsIndexSimilarityDistance;
        Assert.That(
            actual.Vertices[actualCompIndex],
            Is.EqualTo(expected.Vertices[expectedCompIndex]).Using(s_v2Comparer),
            $"{messagePrefix}Vertex mismatch actual node {actualCompIndex} should be equal to expected node {expectedCompIndex}"
        );
    }
}
