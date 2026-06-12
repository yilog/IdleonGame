#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using IdleonGame.Core;
using IdleonGame.Map;
using IdleonGame.Monster;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace IdleonGame.Editor
{
    public static class AddTestMonsterSpawnPointToTilemapScene
    {
        private const string ScenePath = "Assets/_Project/Scenes/Maps/Test_Battle_Tilemap.unity";
        private const string MapDefinitionPath = "Assets/_Project/ScriptableObjects/Maps/TestBattleMap.asset";
        private const string MonsterDatabasePath = "Assets/_Project/Resources/MonsterDatabase.asset";
        private const string MonsterDefinitionPath = "Assets/_Project/ScriptableObjects/Monsters/TestWalkerMonster.asset";
        private const string MonsterTexturePath = "Assets/_Project/Art/Monsters/TestWalkerMonster.png";
        private const string SpawnTexturePath = "Assets/_Project/Tilemaps/Tiles/TestMonsterSpawnTile.png";
        private const string SpawnTilePath = "Assets/_Project/Tilemaps/Tiles/TestMonsterSpawnTile.asset";

        [MenuItem("IdleonGame/Setup/Add Test Monster Spawn Point To Tilemap Scene")]
        public static void AddSpawnPoint()
        {
            Directory.CreateDirectory("Assets/_Project/Art/Monsters");
            Directory.CreateDirectory("Assets/_Project/Resources");
            Directory.CreateDirectory("Assets/_Project/ScriptableObjects/Maps");
            Directory.CreateDirectory("Assets/_Project/ScriptableObjects/Monsters");
            Directory.CreateDirectory("Assets/_Project/Tilemaps/Tiles");

            var monsterDefinition = EnsureMonsterDefinition();
            EnsureMonsterDatabase(monsterDefinition);
            var spawnTile = EnsureSpawnTile();

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var grid = GameObject.Find("Grid");
            var groundObject = GameObject.Find("Tilemap_Ground");
            if (grid == null || groundObject == null)
            {
                Debug.LogError("Grid or Tilemap_Ground was not found in Test_Battle_Tilemap scene.");
                return;
            }

            var groundTilemap = groundObject.GetComponent<Tilemap>();
            var spawnLayer = GameObject.Find("Tilemap_MonsterSpawn");
            if (spawnLayer == null)
            {
                spawnLayer = new GameObject("Tilemap_MonsterSpawn");
                spawnLayer.transform.SetParent(grid.transform);
                spawnLayer.AddComponent<Tilemap>();
                spawnLayer.AddComponent<TilemapRenderer>();
            }

            var tilemap = spawnLayer.GetComponent<Tilemap>();
            tilemap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);
            tilemap.ClearAllTiles();
            tilemap.SetTile(new Vector3Int(-4, -2, 0), spawnTile);
            tilemap.CompressBounds();

            var mapLayer = spawnLayer.GetComponent<MapTilemapLayer>();
            if (mapLayer == null)
            {
                mapLayer = spawnLayer.AddComponent<MapTilemapLayer>();
            }

            mapLayer.Configure(TilemapLayerType.MonsterSpawn, GameRenderLayers.SortingOrders.TilemapMonsterSpawn, false);

            var spawnTilemap = spawnLayer.GetComponent<MonsterSpawnTilemap>();
            if (spawnTilemap == null)
            {
                spawnTilemap = spawnLayer.AddComponent<MonsterSpawnTilemap>();
            }

            spawnTilemap.Configure(groundTilemap);
            UpdateMapDefinition(grid.GetComponentsInChildren<MapTilemapLayer>(true));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            SyncProjectTilePalette.SyncPalette();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static MonsterDefinition EnsureMonsterDefinition()
        {
            var sprite = EnsureMonsterSprite();
            var definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(MonsterDefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<MonsterDefinition>();
                AssetDatabase.CreateAsset(definition, MonsterDefinitionPath);
            }

            definition.EditorSetData(
                "test_walker",
                "Test Walker",
                null,
                sprite,
                Vector2Int.one,
                20,
                0,
                0,
                0,
                MonsterAttackType.None,
                1.25f,
                0.35f,
                1f,
                2.5f,
                1.5f,
                3.5f,
                false,
                2f,
                definition.Drops);

            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static Sprite EnsureMonsterSprite()
        {
            if (!File.Exists(MonsterTexturePath))
            {
                var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                for (var y = 0; y < texture.height; y++)
                {
                    for (var x = 0; x < texture.width; x++)
                    {
                        var border = x == 0 || y == 0 || x == texture.width - 1 || y == texture.height - 1;
                        var eye = (x == 5 || x == 10) && y >= 9 && y <= 11;
                        var color = border
                            ? new Color32(50, 65, 55, 255)
                            : eye ? new Color32(245, 245, 210, 255) : new Color32(86, 174, 112, 255);
                        texture.SetPixel(x, y, color);
                    }
                }

                texture.Apply();
                File.WriteAllBytes(MonsterTexturePath, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(MonsterTexturePath);
            }

            var importer = AssetImporter.GetAtPath(MonsterTexturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(MonsterTexturePath);
        }

        private static void EnsureMonsterDatabase(MonsterDefinition monsterDefinition)
        {
            var database = AssetDatabase.LoadAssetAtPath<MonsterDatabase>(MonsterDatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<MonsterDatabase>();
                AssetDatabase.CreateAsset(database, MonsterDatabasePath);
            }

            database.EditorSetMonsters(new[] { monsterDefinition });
            EditorUtility.SetDirty(database);
        }

        private static MonsterSpawnTile EnsureSpawnTile()
        {
            var sprite = EnsureSpawnSprite();
            var tile = AssetDatabase.LoadAssetAtPath<MonsterSpawnTile>(SpawnTilePath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<MonsterSpawnTile>();
                tile.name = "TestMonsterSpawnTile";
                AssetDatabase.CreateAsset(tile, SpawnTilePath);
            }

            tile.EditorSetData("test_walker", 5f, 10, 0.5f, sprite);
            EditorUtility.SetDirty(tile);
            return tile;
        }

        private static Sprite EnsureSpawnSprite()
        {
            if (!File.Exists(SpawnTexturePath))
            {
                var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                for (var y = 0; y < texture.height; y++)
                {
                    for (var x = 0; x < texture.width; x++)
                    {
                        var border = x == 0 || y == 0 || x == texture.width - 1 || y == texture.height - 1;
                        var cross = x == 7 || x == 8 || y == 7 || y == 8;
                        var color = border
                            ? new Color32(70, 44, 88, 255)
                            : cross ? new Color32(255, 210, 84, 255) : new Color32(160, 82, 190, 180);
                        texture.SetPixel(x, y, color);
                    }
                }

                texture.Apply();
                File.WriteAllBytes(SpawnTexturePath, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(SpawnTexturePath);
            }

            var importer = AssetImporter.GetAtPath(SpawnTexturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(SpawnTexturePath);
        }

        private static void UpdateMapDefinition(MapTilemapLayer[] sceneLayers)
        {
            var mapDefinition = AssetDatabase.LoadAssetAtPath<MapSceneDefinition>(MapDefinitionPath);
            if (mapDefinition == null)
            {
                mapDefinition = ScriptableObject.CreateInstance<MapSceneDefinition>();
                AssetDatabase.CreateAsset(mapDefinition, MapDefinitionPath);
            }

            mapDefinition.EditorSetData(
                "test_battle_tilemap",
                "Test Battle Tilemap",
                new Vector2Int(-10, -3),
                new Vector2Int(21, 8),
                new List<MapTilemapLayerDefinition>
                {
                    new MapTilemapLayerDefinition { layerType = TilemapLayerType.Background, objectName = "Tilemap_Background", sortingOrder = GameRenderLayers.SortingOrders.TilemapBackground, hasCollider = false },
                    new MapTilemapLayerDefinition { layerType = TilemapLayerType.Ground, objectName = "Tilemap_Ground", sortingOrder = GameRenderLayers.SortingOrders.TilemapGround, hasCollider = true },
                    new MapTilemapLayerDefinition { layerType = TilemapLayerType.Decoration, objectName = "Tilemap_Decoration", sortingOrder = GameRenderLayers.SortingOrders.TilemapDecoration, hasCollider = false },
                    new MapTilemapLayerDefinition { layerType = TilemapLayerType.Collision, objectName = "Tilemap_Collision", sortingOrder = GameRenderLayers.SortingOrders.TilemapCollision, hasCollider = false },
                    new MapTilemapLayerDefinition { layerType = TilemapLayerType.MonsterSpawn, objectName = "Tilemap_MonsterSpawn", sortingOrder = GameRenderLayers.SortingOrders.TilemapMonsterSpawn, hasCollider = false }
                },
                new List<MapSpawnPointData>
                {
                    new MapSpawnPointData { spawnPointId = "default", position = new Vector2(-8f, -1.75f) }
                },
                new List<MapPortalData>());

            var controller = Object.FindObjectOfType<BattleMapController>();
            if (controller != null)
            {
                controller.EditorConfigure(mapDefinition, new BoundsInt(-10, -3, 0, 21, 8, 1), new List<MapTilemapLayer>(sceneLayers));
            }

            EditorUtility.SetDirty(mapDefinition);
        }
    }
}
#endif
