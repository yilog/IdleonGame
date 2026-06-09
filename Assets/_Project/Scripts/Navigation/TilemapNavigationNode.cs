using System;
using UnityEngine;

namespace IdleonGame.Navigation
{
    [Serializable]
    public readonly struct TilemapNavigationNode : IEquatable<TilemapNavigationNode>
    {
        public TilemapNavigationNode(Vector3Int cell, NavigationNodeKind kind)
        {
            Cell = cell;
            Kind = kind;
        }

        public Vector3Int Cell { get; }
        public NavigationNodeKind Kind { get; }

        public bool Equals(TilemapNavigationNode other)
        {
            return Cell == other.Cell && Kind == other.Kind;
        }

        public override bool Equals(object obj)
        {
            return obj is TilemapNavigationNode other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Cell.GetHashCode() * 397) ^ (int)Kind;
            }
        }

        public static bool operator ==(TilemapNavigationNode left, TilemapNavigationNode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TilemapNavigationNode left, TilemapNavigationNode right)
        {
            return !left.Equals(right);
        }
    }
}
