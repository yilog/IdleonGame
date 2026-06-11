using UnityEngine;
using UnityEngine.Tilemaps;

namespace IdleonGame.Map
{
    [CreateAssetMenu(fileName = "MapPortalTile", menuName = "IdleonGame/Tilemap/Map Portal Tile")]
    public sealed class MapPortalTile : Tile
    {
        [SerializeField] private string portalId;
        [SerializeField] private string targetLevelId;
        [SerializeField] private string targetSpawnPointId = "default";
        [SerializeField] private bool isActive = true;
        [SerializeField] private Color inactiveColor = new(0.45f, 0.45f, 0.45f, 1f);

        public string PortalId => portalId;
        public string TargetLevelId => targetLevelId;
        public string TargetSpawnPointId => targetSpawnPointId;
        public bool IsActive => isActive;

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);
            if (!isActive)
            {
                tileData.color = inactiveColor;
            }
        }

#if UNITY_EDITOR
        public void EditorSetData(
            string newPortalId,
            string newTargetLevelId,
            string newTargetSpawnPointId,
            bool newIsActive,
            Sprite tileSprite)
        {
            portalId = newPortalId;
            targetLevelId = newTargetLevelId;
            targetSpawnPointId = newTargetSpawnPointId;
            isActive = newIsActive;
            sprite = tileSprite;
            colliderType = Tile.ColliderType.None;
        }
#endif
    }
}
