using IdleonGame.Levels;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace IdleonGame.Map
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class MapPortal : MonoBehaviour
    {
        [SerializeField] private string portalId;
        [SerializeField] private string targetLevelId;
        [SerializeField] private string targetSpawnPointId;
        [SerializeField] private bool isActive = true;
        [SerializeField] private Tilemap portalTilemap;
        [SerializeField] private Vector3Int portalCell;

        public string PortalId => portalId;
        public string TargetLevelId => targetLevelId;
        public string TargetSpawnPointId => targetSpawnPointId;
        public bool IsActive => isActive;
        public Vector3 WorldPosition => GetCharacterAnchorWorldPosition();
        public Vector3Int PortalCell => portalCell;

        public void Configure(
            string newPortalId,
            string newTargetLevelId,
            string newTargetSpawnPointId,
            bool newIsActive,
            Tilemap newPortalTilemap,
            Vector3Int newPortalCell)
        {
            portalId = newPortalId;
            targetLevelId = newTargetLevelId;
            targetSpawnPointId = newTargetSpawnPointId;
            isActive = newIsActive;
            portalTilemap = newPortalTilemap;
            portalCell = newPortalCell;
        }

        public bool IsInPortalCell(Vector3 worldPosition)
        {
            if (portalTilemap == null)
            {
                return Vector2.Distance(transform.position, worldPosition) <= 0.5f;
            }

            var cell = portalTilemap.WorldToCell(worldPosition);
            cell.z = portalCell.z;
            return cell == portalCell;
        }

        private Vector3 GetCharacterAnchorWorldPosition()
        {
            if (portalTilemap == null)
            {
                return transform.position;
            }

            var center = portalTilemap.GetCellCenterWorld(portalCell);
            return center + Vector3.down * (portalTilemap.layoutGrid.cellSize.y * 0.5f);
        }

        public bool TryActivate()
        {
            if (!isActive)
            {
                Debug.Log($"Portal {portalId} is inactive.");
                return false;
            }

            if (string.IsNullOrEmpty(targetLevelId))
            {
                Debug.LogWarning($"Portal {portalId} has no target level id.");
                return false;
            }

            var transitionService = LevelSceneTransitionService.Instance;
            if (transitionService == null)
            {
                Debug.LogWarning($"Portal {portalId} cannot find LevelSceneTransitionService.");
                return false;
            }

            transitionService.SwitchToLevel(targetLevelId, targetSpawnPointId);
            return true;
        }
    }
}
