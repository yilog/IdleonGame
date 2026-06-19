using IdleonGame.Core;
using IdleonGame.Data;
using IdleonGame.Levels;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace IdleonGame.Map
{
    [DisallowMultipleComponent]
    public sealed class MapPortalUnlockRequirementView : MonoBehaviour
    {
        private const string RequirementPrefabPath = "Prefabs/Map/PortalUnlockRequirementView";

        [SerializeField] private Color lockedTileColor = new(0.45f, 0.45f, 0.45f, 1f);
        [SerializeField] private Vector3 viewOffset = new(0f, 0.72f, 0f);
        [SerializeField] private MapPortalUnlockRequirementPrefabView viewPrefab;

        private MapPortal portal;
        private Tilemap portalTilemap;
        private Vector3Int portalCell;
        private MapPortalUnlockRequirementPrefabView viewInstance;
        private bool subscribed;

        public void Configure(MapPortal newPortal, Tilemap newPortalTilemap, Vector3Int newPortalCell)
        {
            portal = newPortal;
            portalTilemap = newPortalTilemap;
            portalCell = newPortalCell;
            EnsureView();
            Subscribe();
            Refresh();
        }

        private void OnEnable()
        {
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (subscribed || PlayerRuntimeDataService.Instance == null)
            {
                return;
            }

            PlayerRuntimeDataService.Instance.MonsterKillRecorded += OnMonsterKillRecorded;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || PlayerRuntimeDataService.Instance == null)
            {
                subscribed = false;
                return;
            }

            PlayerRuntimeDataService.Instance.MonsterKillRecorded -= OnMonsterKillRecorded;
            subscribed = false;
        }

        private void OnMonsterKillRecorded(string monsterId, int count)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (portal == null)
            {
                portal = GetComponent<MapPortal>();
            }

            var runtimeData = PlayerRuntimeDataService.EnsureExists();
            var targetLevel = LevelDatabase.Find(portal != null ? portal.TargetLevelId : null);
            var shouldShowRequirement = LevelUnlockRequirementQuery.TryGetRemainingKillRequirement(
                targetLevel,
                runtimeData.Data,
                out _,
                out var remainingCount);
            SetTileLocked(shouldShowRequirement);

            if (viewInstance != null)
            {
                viewInstance.gameObject.SetActive(shouldShowRequirement);
            }

            if (viewInstance != null)
            {
                viewInstance.SetRemainingCount(remainingCount);
            }
        }

        private void SetTileLocked(bool lockedByRequirement)
        {
            if (portalTilemap == null)
            {
                return;
            }

            var locked = lockedByRequirement || (portal != null && !portal.IsActive);
            portalTilemap.SetTileFlags(portalCell, TileFlags.None);
            portalTilemap.SetColor(portalCell, locked ? lockedTileColor : Color.white);
        }

        private void EnsureView()
        {
            if (viewInstance != null)
            {
                return;
            }

            if (viewPrefab == null)
            {
                viewPrefab = Resources.Load<MapPortalUnlockRequirementPrefabView>(RequirementPrefabPath);
            }

            if (viewPrefab == null)
            {
                Debug.LogWarning($"Portal unlock requirement prefab not found at Resources/{RequirementPrefabPath}.");
                return;
            }

            viewInstance = Instantiate(viewPrefab, transform);
            viewInstance.name = "UnlockRequirementView";
            viewInstance.transform.localPosition = viewOffset;
            viewInstance.SetSortingOrder(GameRenderLayers.SortingOrders.PortalRequirement);
            viewInstance.gameObject.SetActive(false);
        }
    }
}
