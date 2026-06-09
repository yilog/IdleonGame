using System.Collections.Generic;
using IdleonGame.Map;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace IdleonGame.Navigation
{
    [DisallowMultipleComponent]
    public sealed class TilemapNavigationPathfinder : MonoBehaviour
    {
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private RopeTilemap ropeTilemap;

        private readonly List<TilemapNavigationNode> neighbors = new List<TilemapNavigationNode>(8);

        private void Awake()
        {
            FindSceneReferences();
        }

        public bool TryFindPath(Vector3 startWorld, Vector3 clickedWorld, List<TilemapNavigationNode> result)
        {
            result.Clear();
            FindSceneReferences();

            if (groundTilemap == null || ropeTilemap == null)
            {
                return false;
            }

            var clickedGroundCell = groundTilemap.WorldToCell(clickedWorld);
            clickedGroundCell.z = 0;
            if (!groundTilemap.HasTile(clickedGroundCell))
            {
                return false;
            }

            var start = GetStartNode(startWorld);
            var goal = new TilemapNavigationNode(clickedGroundCell + Vector3Int.up, NavigationNodeKind.Stand);
            if (!IsValidNode(start) || !IsValidNode(goal))
            {
                return false;
            }

            return FindPath(start, goal, result);
        }

        public Vector3 GetNodeWorldPosition(TilemapNavigationNode node)
        {
            return groundTilemap.GetCellCenterWorld(node.Cell);
        }

        private TilemapNavigationNode GetStartNode(Vector3 startWorld)
        {
            var startCell = groundTilemap.WorldToCell(startWorld);
            if (ropeTilemap.HasRopeAtCell(startCell))
            {
                return new TilemapNavigationNode(startCell, NavigationNodeKind.Rope);
            }

            if (IsStandCell(startCell))
            {
                return new TilemapNavigationNode(startCell, NavigationNodeKind.Stand);
            }

            var below = startCell + Vector3Int.down;
            if (groundTilemap.HasTile(below))
            {
                return new TilemapNavigationNode(startCell, NavigationNodeKind.Stand);
            }

            return new TilemapNavigationNode(startCell, NavigationNodeKind.Stand);
        }

        private bool FindPath(TilemapNavigationNode start, TilemapNavigationNode goal, List<TilemapNavigationNode> result)
        {
            var open = new List<TilemapNavigationNode> { start };
            var cameFrom = new Dictionary<TilemapNavigationNode, TilemapNavigationNode>();
            var gScore = new Dictionary<TilemapNavigationNode, int> { [start] = 0 };
            var closed = new HashSet<TilemapNavigationNode>();

            while (open.Count > 0)
            {
                var currentIndex = GetLowestScoreIndex(open, goal, gScore);
                var current = open[currentIndex];
                open.RemoveAt(currentIndex);

                if (current == goal)
                {
                    ReconstructPath(current, cameFrom, result);
                    return true;
                }

                closed.Add(current);
                GetNeighbors(current, neighbors);

                foreach (var next in neighbors)
                {
                    if (closed.Contains(next))
                    {
                        continue;
                    }

                    var tentative = gScore[current] + 1;
                    if (gScore.TryGetValue(next, out var existing) && tentative >= existing)
                    {
                        continue;
                    }

                    cameFrom[next] = current;
                    gScore[next] = tentative;

                    if (!open.Contains(next))
                    {
                        open.Add(next);
                    }
                }
            }

            return false;
        }

        private int GetLowestScoreIndex(List<TilemapNavigationNode> open, TilemapNavigationNode goal, Dictionary<TilemapNavigationNode, int> gScore)
        {
            var bestIndex = 0;
            var bestScore = int.MaxValue;

            for (var i = 0; i < open.Count; i++)
            {
                var node = open[i];
                var score = gScore[node] + GetHeuristic(node, goal);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private static int GetHeuristic(TilemapNavigationNode a, TilemapNavigationNode b)
        {
            return Mathf.Abs(a.Cell.x - b.Cell.x) + Mathf.Abs(a.Cell.y - b.Cell.y);
        }

        private void ReconstructPath(TilemapNavigationNode current, Dictionary<TilemapNavigationNode, TilemapNavigationNode> cameFrom, List<TilemapNavigationNode> result)
        {
            result.Add(current);
            while (cameFrom.TryGetValue(current, out var previous))
            {
                current = previous;
                result.Add(current);
            }

            result.Reverse();
        }

        private void GetNeighbors(TilemapNavigationNode node, List<TilemapNavigationNode> output)
        {
            output.Clear();

            if (node.Kind == NavigationNodeKind.Stand)
            {
                AddStandNeighbor(node.Cell + Vector3Int.left, output);
                AddStandNeighbor(node.Cell + Vector3Int.right, output);
                AddOneLevelDropNeighbor(node.Cell + new Vector3Int(-1, -1, 0), output);
                AddOneLevelDropNeighbor(node.Cell + new Vector3Int(1, -1, 0), output);
                AddRopeNeighbor(node.Cell, output);
                AddRopeNeighbor(node.Cell + Vector3Int.down, output);
                return;
            }

            AddRopeNeighbor(node.Cell + Vector3Int.up, output);
            AddRopeNeighbor(node.Cell + Vector3Int.down, output);
            AddStandNeighbor(node.Cell, output);
            AddStandNeighbor(node.Cell + Vector3Int.up, output);
        }

        private void AddStandNeighbor(Vector3Int cell, List<TilemapNavigationNode> output)
        {
            var node = new TilemapNavigationNode(cell, NavigationNodeKind.Stand);
            if (IsValidNode(node))
            {
                output.Add(node);
            }
        }

        private void AddOneLevelDropNeighbor(Vector3Int cell, List<TilemapNavigationNode> output)
        {
            if (IsStandCell(cell) && !groundTilemap.HasTile(cell))
            {
                output.Add(new TilemapNavigationNode(cell, NavigationNodeKind.Stand));
            }
        }

        private void AddRopeNeighbor(Vector3Int cell, List<TilemapNavigationNode> output)
        {
            var node = new TilemapNavigationNode(cell, NavigationNodeKind.Rope);
            if (IsValidNode(node))
            {
                output.Add(node);
            }
        }

        private bool IsValidNode(TilemapNavigationNode node)
        {
            return node.Kind == NavigationNodeKind.Rope
                ? ropeTilemap.HasRopeAtCell(node.Cell)
                : IsStandCell(node.Cell);
        }

        private bool IsStandCell(Vector3Int cell)
        {
            return groundTilemap.HasTile(cell + Vector3Int.down) && !groundTilemap.HasTile(cell);
        }

        private void FindSceneReferences()
        {
            if (groundTilemap == null)
            {
                var groundObject = GameObject.Find("Tilemap_Ground");
                if (groundObject != null)
                {
                    groundTilemap = groundObject.GetComponent<Tilemap>();
                }
            }

            if (ropeTilemap == null)
            {
                ropeTilemap = Object.FindObjectOfType<RopeTilemap>();
            }
        }
    }
}
