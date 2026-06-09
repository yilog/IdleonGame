using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleonGame.Map
{
    [CreateAssetMenu(menuName = "IdleonGame/Map/Map Scene Definition")]
    public sealed class MapSceneDefinition : ScriptableObject
    {
        [SerializeField] private string sceneId;
        [SerializeField] private string displayName;
        [SerializeField] private Vector2Int origin;
        [SerializeField] private Vector2Int size;
        [SerializeField] private List<MapTilemapLayerDefinition> tilemapLayers = new List<MapTilemapLayerDefinition>();
        [SerializeField] private List<MapSpawnPointData> spawnPoints = new List<MapSpawnPointData>();
        [SerializeField] private List<MapPortalData> portals = new List<MapPortalData>();

        public string SceneId => sceneId;
        public string DisplayName => displayName;
        public Vector2Int Origin => origin;
        public Vector2Int Size => size;
        public IReadOnlyList<MapTilemapLayerDefinition> TilemapLayers => tilemapLayers;
        public IReadOnlyList<MapSpawnPointData> SpawnPoints => spawnPoints;
        public IReadOnlyList<MapPortalData> Portals => portals;

#if UNITY_EDITOR
        public void EditorSetData(
            string newSceneId,
            string newDisplayName,
            Vector2Int newOrigin,
            Vector2Int newSize,
            List<MapTilemapLayerDefinition> newLayers,
            List<MapSpawnPointData> newSpawnPoints,
            List<MapPortalData> newPortals)
        {
            sceneId = newSceneId;
            displayName = newDisplayName;
            origin = newOrigin;
            size = newSize;
            tilemapLayers = newLayers;
            spawnPoints = newSpawnPoints;
            portals = newPortals;
        }
#endif
    }

    [Serializable]
    public sealed class MapTilemapLayerDefinition
    {
        public TilemapLayerType layerType;
        public string objectName;
        public int sortingOrder;
        public bool hasCollider;
    }

    [Serializable]
    public sealed class MapSpawnPointData
    {
        public string spawnPointId;
        public Vector2 position;
    }

    [Serializable]
    public sealed class MapPortalData
    {
        public string portalId;
        public Rect triggerArea;
        public string targetSceneId;
        public string targetSpawnPointId;
    }
}