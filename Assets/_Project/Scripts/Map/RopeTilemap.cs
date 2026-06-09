using UnityEngine;
using UnityEngine.Tilemaps;

namespace IdleonGame.Map
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Tilemap))]
    public sealed class RopeTilemap : MonoBehaviour
    {
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private TileBase ropeTile;

        public TileBase RopeTile => ropeTile;

        private void Reset()
        {
            tilemap = GetComponent<Tilemap>();
        }

        private void Awake()
        {
            if (tilemap == null)
            {
                tilemap = GetComponent<Tilemap>();
            }
        }

        public void Configure(TileBase tile)
        {
            tilemap = GetComponent<Tilemap>();
            ropeTile = tile;
        }

        public bool HasRopeAtWorldPosition(Vector2 worldPosition)
        {
            if (tilemap == null || ropeTile == null)
            {
                return false;
            }

            var cell = tilemap.WorldToCell(worldPosition);
            return tilemap.GetTile(cell) == ropeTile;
        }

        public bool HasRopeNearBounds(Bounds bounds)
        {
            var center = new Vector2(bounds.center.x, bounds.center.y);
            var upper = new Vector2(bounds.center.x, bounds.center.y + bounds.extents.y * 0.35f);
            var lower = new Vector2(bounds.center.x, bounds.center.y - bounds.extents.y * 0.35f);
            return HasRopeAtWorldPosition(center) || HasRopeAtWorldPosition(upper) || HasRopeAtWorldPosition(lower);
        }
    }
}