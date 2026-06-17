#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using IdleonGame.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace IdleonGame.Editor
{
    public static class CreateAdditionalLevelPortals
    {
        private const string Level1_1ScenePath = "Assets/_Project/Scenes/Levels/level1_1.unity";
        private const string MapDefinitionPath = "Assets/_Project/ScriptableObjects/Maps/level1_1_Map.asset";
        private const string TilesFolder = "Assets/_Project/Tilemaps/Tiles";
        private const string PortalTexturePath = TilesFolder + "/TestPortalTile.png";

        private static readonly PortalSpec[] Portals =
        {
            new PortalSpec("portal_to_level1_3", "level1_3", new Vector3Int(-6, -2, 0)),
            new PortalSpec("portal_to_level2_1", "level2_1", new Vector3Int(-2, -2, 0)),
            new PortalSpec("portal_to_level2_2", "level2_2", new Vector3Int(2, -2, 0)),
            new PortalSpec("portal_to_level2_3", "level2_3", new Vector3Int(6, -2, 0))
        };

        [MenuItem("IdleonGame/Setup/Add Additional Level Portals")]
        public static void CreateAssets()
        {
            Directory.CreateDirectory(TilesFolder);
            var portalTiles = new List<MapPortalTile>();
            foreach (var portal in Portals)
            {
                portalTiles.Add(CreatePortalTile(portal));
            }

            AddPortalsToLevel1_1(portalTiles);
            UpdateMapDefinition();
            SyncProjectTilePalette.SyncPalette();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static MapPortalTile CreatePortalTile(PortalSpec portal)
        {
            var path = $"{TilesFolder}/TestPortalTo{ToPascalName(portal.TargetLevelId)}Tile.asset";
            var tile = AssetDatabase.LoadAssetAtPath<MapPortalTile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<MapPortalTile>();
                tile.name = Path.GetFileNameWithoutExtension(path);
                AssetDatabase.CreateAsset(tile, path);
            }

            tile.EditorSetData(portal.PortalId, portal.TargetLevelId, "default", true, LoadPortalSprite());
            EditorUtility.SetDirty(tile);
            return tile;
        }

        private static void AddPortalsToLevel1_1(IReadOnlyList<MapPortalTile> portalTiles)
        {
            var scene = EditorSceneManager.OpenScene(Level1_1ScenePath, OpenSceneMode.Single);
            var decorationObject = GameObject.Find("Tilemap_Decoration");
            if (decorationObject == null)
            {
                Debug.LogWarning($"Cannot add portals. Missing Tilemap_Decoration in {Level1_1ScenePath}");
                return;
            }

            var decorationTilemap = decorationObject.GetComponent<Tilemap>();
            if (decorationTilemap == null)
            {
                Debug.LogWarning($"Cannot add portals. Tilemap_Decoration has no Tilemap in {Level1_1ScenePath}");
                return;
            }

            if (decorationObject.GetComponent<MapPortalTilemap>() == null)
            {
                decorationObject.AddComponent<MapPortalTilemap>();
            }

            for (var i = 0; i < Portals.Length && i < portalTiles.Count; i++)
            {
                decorationTilemap.SetTile(Portals[i].Cell, portalTiles[i]);
            }

            decorationTilemap.CompressBounds();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void UpdateMapDefinition()
        {
            var mapDefinition = AssetDatabase.LoadAssetAtPath<MapSceneDefinition>(MapDefinitionPath);
            if (mapDefinition == null)
            {
                Debug.LogWarning($"Cannot update portals. Missing map definition: {MapDefinitionPath}");
                return;
            }

            var portals = new List<MapPortalData>(mapDefinition.Portals);
            foreach (var portal in Portals)
            {
                portals.RemoveAll(existing => existing.portalId == portal.PortalId);
                portals.Add(new MapPortalData
                {
                    portalId = portal.PortalId,
                    triggerArea = new Rect(portal.Cell.x - 0.5f, portal.Cell.y - 0.5f, 1f, 1f),
                    targetSceneId = portal.TargetLevelId,
                    targetSpawnPointId = "default"
                });
            }

            mapDefinition.EditorSetData(
                mapDefinition.SceneId,
                mapDefinition.DisplayName,
                mapDefinition.Origin,
                mapDefinition.Size,
                new List<MapTilemapLayerDefinition>(mapDefinition.TilemapLayers),
                new List<MapSpawnPointData>(mapDefinition.SpawnPoints),
                portals);
            EditorUtility.SetDirty(mapDefinition);
        }

        private static Sprite LoadPortalSprite()
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PortalTexturePath);
            if (sprite != null)
            {
                return sprite;
            }

            Debug.LogWarning($"Portal sprite not found: {PortalTexturePath}");
            return null;
        }

        private static string ToPascalName(string value)
        {
            return value.Replace("_", string.Empty).ToUpperInvariant();
        }

        private readonly struct PortalSpec
        {
            public PortalSpec(string portalId, string targetLevelId, Vector3Int cell)
            {
                PortalId = portalId;
                TargetLevelId = targetLevelId;
                Cell = cell;
            }

            public string PortalId { get; }
            public string TargetLevelId { get; }
            public Vector3Int Cell { get; }
        }
    }
}
#endif
