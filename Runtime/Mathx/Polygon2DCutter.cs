using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SensenToolkit.Internal;

namespace SensenToolkit
{
    public class Polygon2DCutter
    {
        private SimpleSegment2D _cutSegment;
        private Polygon2D _polygon;
        private CutGraph _graph;
        private List<CutPolygonBuilder> _allPolygons = new();
        private Dictionary<(CutGraphNode, bool), int> _cutNodesMissingUsageCount = new();
        private HashSet<(CutGraphNode, bool)> _nodesCantBeUsedAgain = new();

        public Polygon2DCutter(Polygon2D polygon, Vector2 origin, Vector2 direction)
        {
            _polygon = polygon;
            _cutSegment = new SimpleSegment2D(
                position: origin,
                direction: direction,
                lengthForward: Mathf.Infinity,
                lengthBackward: Mathf.Infinity
            );
            _graph = CutGraphBuilder.Build(polygon, _cutSegment);
        }

        public (List<Polygon2D>, List<Polygon2D>) Execute()
        {
            try
            {
                return ActualExecute();
            }
            catch (Exception e)
            {
                Debug.Log($"[Polygon2DCutter] Error: {e.Message}");
                Debug.Log($"[Polygon2DCutter] How to rebuild...");
                Debug.Log($"Polygon: {_polygon.BuildSpec()}");
                Debug.Log($"CutSegment: {_cutSegment.BuildSpec()}");
                Debug.LogError($"[Polygon2DCutter] --- Error End ---");
                throw e;
            }
        }

        private (List<Polygon2D>, List<Polygon2D>) ActualExecute()
        {
            if (_graph.AllNodes.Length <= 2) throw new InvalidOperationException("Polygon must have at least 3 vertices");
            if (_graph.CutNodes.Length == 0)
            {
                List<Polygon2D> sideA = new();
                List<Polygon2D> sideB = new();
                (_graph.AllNodes[0].IsSideA ? sideA : sideB).Add(_polygon);
                return (sideA, sideB);
            }

            bool anyNodeIsInSideA = false;
            bool anyNodeIsInSideB = false;
            foreach (CutGraphNode node in _graph.CutNodes)
            {
                anyNodeIsInSideA = anyNodeIsInSideA || node.IsSideA;
                anyNodeIsInSideB = anyNodeIsInSideB || node.IsSideB;
                if (anyNodeIsInSideA && anyNodeIsInSideB) break;
            }
            if (!anyNodeIsInSideB) return (new List<Polygon2D> { _polygon }, new());
            if (!anyNodeIsInSideA) return (new(), new List<Polygon2D> { _polygon });

            foreach (CutGraphNode cutNode in _graph.CutNodes)
            {
                MountPolygon(
                    firstCutNode: cutNode,
                    isSideA: true
                );

                MountPolygon(
                    firstCutNode: cutNode,
                    isSideA: false
                );
            }


            List<Polygon2D> sideAPolygons = new();
            List<Polygon2D> sideBPolygons = new();
            foreach (CutPolygonBuilder mounting in _allPolygons)
            {
                Polygon2D polygon = mounting.BuildPolygon();
                List<Polygon2D> polygonsList = mounting.IsSideA
                    ? sideAPolygons
                    : sideBPolygons;
                polygonsList.Add(polygon);
            }

            return (sideAPolygons, sideBPolygons);
        }

        private void MountPolygon(CutGraphNode firstCutNode, bool isSideA)
        {
            // Debug.Log($"MountPolygon: {firstCutNode.Position} isSideA:{isSideA}");
            if (_nodesCantBeUsedAgain.Contains((firstCutNode, isSideA))) return;

            bool isSideB = !isSideA;
            if (isSideA && !firstCutNode.IsSideA) return;
            if (isSideB && !firstCutNode.IsSideB) return;

            CutPolygonBuilder polygon = new();
            polygon.IsSideA = isSideA;
            _allPolygons.Add(polygon);

            CutGraphNode lastVisitedNode = null;
            CutGraphNode node = firstCutNode;
            SafeLoop safeLoop = new(maxIterations: 1000);
            safeLoop.Reset();
            do
            {
                // Debug.Log($"Node: {node.Position} isSideA:{node.IsSideA} isSideB:{node.IsSideB} isCut:{node.IsCutIntersection}");
                polygon.AddAtEnd(node);
                if (node.IsCutIntersection)
                {
                    if (!_cutNodesMissingUsageCount.TryGetValue((node, isSideA), out int missingUsageCount))
                    {
                        int maxUsageCount = 1;
                        if (node.IsAVertexThatWasCut && node.IsSideA && node.IsSideB)
                        {
                            maxUsageCount = Mathf.Max(1, isSideA ? node.SideABranchCount : node.SideBBranchCount);
                        }

                        missingUsageCount = maxUsageCount;
                    }
                    missingUsageCount = Mathf.Max(0, missingUsageCount - 1);
                    if (missingUsageCount == 0) _nodesCantBeUsedAgain.Add((node, isSideA));
                    _cutNodesMissingUsageCount[(node, isSideA)] = missingUsageCount;
                }
                else
                {
                    _nodesCantBeUsedAgain.Add((node, isSideA));
                }

                CutGraphNode nodeBkp = node;
                node = GetNextPolygonNode(node, firstCutNode, lastVisitedNode, isSideA);
                lastVisitedNode = nodeBkp;
                safeLoop.Count();
            } while (node != firstCutNode);
        }

        private CutGraphNode GetNextPolygonNode(
            CutGraphNode node,
            CutGraphNode firstVisitedNode,
            CutGraphNode lastVisitedNode,
            bool isSideA
        )
        {
            CutGraphNode nextNode = node.NextNode;
            CutGraphNode prevNode = node.PreviousNode;
            CutGraphNode crossCutPrevNode = node.PreviousCrossingCutNode;
            CutGraphNode crossCutNextNode = node.NextCrossingCutNode;

            bool isCutNode = node.IsCutIntersection && lastVisitedNode != null;

            if (isCutNode)
            {
                if (CheckIsValid(crossCutPrevNode, firstVisitedNode, lastVisitedNode, isSideA))
                {
                    return crossCutPrevNode;
                }

                if (CheckIsValid(crossCutNextNode, firstVisitedNode, lastVisitedNode, isSideA))
                {
                    return crossCutNextNode;
                }
            }

            if (CheckIsValid(nextNode, firstVisitedNode, lastVisitedNode, isSideA)
                && !IsCutSegment(node, nextNode)
            ) return nextNode;

            if (CheckIsValid(prevNode, firstVisitedNode, lastVisitedNode, isSideA)
                && !IsCutSegment(node, prevNode)
            ) return prevNode;

            if (!isCutNode)
            {
                if (CheckIsValid(crossCutPrevNode, firstVisitedNode, lastVisitedNode, isSideA))
                {
                    return crossCutPrevNode;
                }
                if (CheckIsValid(crossCutNextNode, firstVisitedNode, lastVisitedNode, isSideA))
                {
                    return crossCutNextNode;
                }
            }
            throw new InvalidOperationException($"No next polygon node found for {node.Position} in side {(isSideA ? "A" : "B")}");
        }

        private bool IsCutSegment(CutGraphNode a, CutGraphNode b)
        {
            return (a.NextNode == b || a.PreviousNode == b) && (a.IsCutIntersection && b.IsCutIntersection);
        }

        private bool CheckIsValid(
            CutGraphNode node,
            CutGraphNode firstVisitedNode,
            CutGraphNode lastVisitedNode,
            bool isSideA
        )
        {
            if (node == null
                || node == lastVisitedNode // Can't ever go back
            ) return false;

            // if is closing the polygon
            if (node == firstVisitedNode) return true;

            return CheckSide(node, isSideA)
                && !_nodesCantBeUsedAgain.Contains((node, isSideA));
        }


        private bool CheckSide(CutGraphNode node, bool isSideA)
        {
            if (isSideA && node.IsSideA) return true;
            if (!isSideA && node.IsSideB) return true;
            return false;
        }
    }
}
