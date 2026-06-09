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

        public Tilemap Tilemap => tilemap;
        public TileBase RopeTile => ropeTile;
        public BoundsInt CellBounds => tilemap != null ? tilemap.cellBounds : new BoundsInt();

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

        public Vector3Int WorldToCell(Vector2 worldPosition)
        {
            return tilemap.WorldToCell(worldPosition);
        }

        public Vector3 GetCellCenterWorld(Vector3Int cell)
        {
            return tilemap.GetCellCenterWorld(cell);
        }

        public bool HasRopeAtCell(Vector3Int cell)
        {
            if (tilemap == null || ropeTile == null)
            {
                return false;
            }

            return tilemap.GetTile(cell) == ropeTile;
        }

        public bool HasRopeAtWorldPosition(Vector2 worldPosition)
        {
            if (tilemap == null)
            {
                return false;
            }

            return HasRopeAtCell(tilemap.WorldToCell(worldPosition));
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
