using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace IdleonGame.Map
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Tilemap))]
    public sealed class MapPortalTilemap : MonoBehaviour
    {
        private const string RuntimeRootName = "RuntimePortals";

        [SerializeField] private Tilemap tilemap;
        [SerializeField] private Vector2 portalColliderSize = new(0.85f, 0.95f);

        private readonly List<GameObject> runtimePortals = new();

        private void Awake()
        {
            if (tilemap == null)
            {
                tilemap = GetComponent<Tilemap>();
            }

            RebuildPortals();
        }

        public void RebuildPortals()
        {
            ClearRuntimePortals();
            if (tilemap == null)
            {
                return;
            }

            var root = new GameObject(RuntimeRootName);
            root.transform.SetParent(transform, false);
            runtimePortals.Add(root);

            tilemap.CompressBounds();
            foreach (var cell in tilemap.cellBounds.allPositionsWithin)
            {
                var portalTile = tilemap.GetTile<MapPortalTile>(cell);
                if (portalTile == null)
                {
                    continue;
                }

                CreateRuntimePortal(root.transform, cell, portalTile);
            }
        }

        private void CreateRuntimePortal(Transform parent, Vector3Int cell, MapPortalTile portalTile)
        {
            var portalObject = new GameObject($"Portal_{portalTile.PortalId}_{cell.x}_{cell.y}");
            portalObject.transform.SetParent(parent, false);
            portalObject.transform.position = tilemap.GetCellCenterWorld(cell);

            var collider = portalObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = portalColliderSize;

            var portal = portalObject.AddComponent<MapPortal>();
            portal.Configure(
                string.IsNullOrEmpty(portalTile.PortalId) ? $"{cell.x}_{cell.y}" : portalTile.PortalId,
                portalTile.TargetLevelId,
                portalTile.TargetSpawnPointId,
                portalTile.IsActive,
                tilemap,
                cell);
        }

        private void ClearRuntimePortals()
        {
            for (var i = runtimePortals.Count - 1; i >= 0; i--)
            {
                if (runtimePortals[i] != null)
                {
                    Destroy(runtimePortals[i]);
                }
            }

            runtimePortals.Clear();
        }
    }
}
