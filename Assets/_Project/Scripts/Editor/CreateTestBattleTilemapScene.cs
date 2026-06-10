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
    [InitializeOnLoad]
    public static class CreateTestBattleTilemapScene
    {
        private const string ScenePath = "Assets/_Project/Scenes/Maps/Test_Battle_Tilemap.unity";
        private const string TileTexturePath = "Assets/_Project/Tilemaps/Tiles/TestGroundTile.png";
        private const string TileAssetPath = "Assets/_Project/Tilemaps/Tiles/TestGroundTile.asset";
        private const string MonsterSpawnTexturePath = "Assets/_Project/Tilemaps/Tiles/TestMonsterSpawnTile.png";
        private const string MonsterSpawnTileAssetPath = "Assets/_Project/Tilemaps/Tiles/TestMonsterSpawnTile.asset";
        private const string MapDefinitionPath = "Assets/_Project/ScriptableObjects/Maps/TestBattleMap.asset";

        static CreateTestBattleTilemapScene()
        {
            EditorApplication.delayCall += CreateSceneIfMissing;
        }

        [MenuItem("IdleonGame/Setup/Create Test Battle Tilemap Scene")]
        public static void CreateScene()
        {
            Directory.CreateDirectory("Assets/_Project/Scenes/Maps");
            Directory.CreateDirectory("Assets/_Project/Tilemaps/Tiles");
            Directory.CreateDirectory("Assets/_Project/ScriptableObjects/Maps");

            var groundTile = CreateGroundTile();
            var monsterSpawnTile = CreateMonsterSpawnTile();
            var mapDefinition = CreateMapDefinition();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Test_Battle_Tilemap";

            CreateCamera();

            var mapRoot = new GameObject("BattleMap_Test");
            var controller = mapRoot.AddComponent<BattleMapController>();

            var gridObject = new GameObject("Grid");
            gridObject.transform.SetParent(mapRoot.transform);
            var grid = gridObject.AddComponent<Grid>();
            grid.cellSize = Vector3.one;
            grid.cellGap = Vector3.zero;
            grid.cellLayout = GridLayout.CellLayout.Rectangle;
            grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;

            var layers = new List<MapTilemapLayer>
            {
                CreateLayer(gridObject.transform, "Tilemap_Background", TilemapLayerType.Background, GameRenderLayers.SortingOrders.TilemapBackground, false),
                CreateLayer(gridObject.transform, "Tilemap_Ground", TilemapLayerType.Ground, GameRenderLayers.SortingOrders.TilemapGround, true),
                CreateLayer(gridObject.transform, "Tilemap_Decoration", TilemapLayerType.Decoration, GameRenderLayers.SortingOrders.TilemapDecoration, false),
                CreateLayer(gridObject.transform, "Tilemap_Collision", TilemapLayerType.Collision, GameRenderLayers.SortingOrders.TilemapCollision, false),
                CreateLayer(gridObject.transform, "Tilemap_MonsterSpawn", TilemapLayerType.MonsterSpawn, GameRenderLayers.SortingOrders.TilemapMonsterSpawn, false)
            };

            var groundLayer = layers.Find(layer => layer.LayerType == TilemapLayerType.Ground);
            var monsterSpawnLayer = layers.Find(layer => layer.LayerType == TilemapLayerType.MonsterSpawn);
            PaintFlatGround(groundLayer.Tilemap, groundTile);
            PaintMonsterSpawnPoint(monsterSpawnLayer.Tilemap, monsterSpawnTile);
            monsterSpawnLayer.gameObject.AddComponent<MonsterSpawnTilemap>().Configure(groundLayer.Tilemap);
            controller.EditorConfigure(mapDefinition, new BoundsInt(-10, -3, 0, 21, 8, 1), layers);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateSceneIfMissing()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += CreateSceneIfMissing;
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                CreateScene();
            }
        }

        private static Tile CreateGroundTile()
        {
            if (!File.Exists(TileTexturePath))
            {
                var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                for (var y = 0; y < texture.height; y++)
                {
                    for (var x = 0; x < texture.width; x++)
                    {
                        var color = y > 10 ? new Color32(82, 150, 76, 255) : new Color32(96, 62, 38, 255);
                        texture.SetPixel(x, y, color);
                    }
                }

                texture.Apply();
                File.WriteAllBytes(TileTexturePath, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(TileTexturePath);
            }

            var importer = AssetImporter.GetAtPath(TileTexturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            var tile = AssetDatabase.LoadAssetAtPath<Tile>(TileAssetPath);
            if (tile != null)
            {
                return tile;
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(TileTexturePath);
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = "TestGroundTile";
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.Sprite;
            AssetDatabase.CreateAsset(tile, TileAssetPath);
            return tile;
        }

        private static MonsterSpawnTile CreateMonsterSpawnTile()
        {
            if (!File.Exists(MonsterSpawnTexturePath))
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
                File.WriteAllBytes(MonsterSpawnTexturePath, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(MonsterSpawnTexturePath);
            }

            var importer = AssetImporter.GetAtPath(MonsterSpawnTexturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            var tile = AssetDatabase.LoadAssetAtPath<MonsterSpawnTile>(MonsterSpawnTileAssetPath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<MonsterSpawnTile>();
                tile.name = "TestMonsterSpawnTile";
                AssetDatabase.CreateAsset(tile, MonsterSpawnTileAssetPath);
            }

            tile.EditorSetData("test_walker", 5f, 10, 0.5f, AssetDatabase.LoadAssetAtPath<Sprite>(MonsterSpawnTexturePath));
            EditorUtility.SetDirty(tile);
            return tile;
        }

        private static MapSceneDefinition CreateMapDefinition()
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

            EditorUtility.SetDirty(mapDefinition);
            return mapDefinition;
        }

        private static MapTilemapLayer CreateLayer(Transform parent, string name, TilemapLayerType type, int sortingOrder, bool hasCollider)
        {
            var layerObject = new GameObject(name);
            layerObject.transform.SetParent(parent);
            var tilemap = layerObject.AddComponent<Tilemap>();
            tilemap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);
            layerObject.AddComponent<TilemapRenderer>();

            if (hasCollider)
            {
                var rigidbody2D = layerObject.AddComponent<Rigidbody2D>();
                rigidbody2D.bodyType = RigidbodyType2D.Static;
                var collider = layerObject.AddComponent<TilemapCollider2D>();
                collider.usedByComposite = true;
                layerObject.AddComponent<CompositeCollider2D>();
            }

            var mapLayer = layerObject.AddComponent<MapTilemapLayer>();
            mapLayer.Configure(type, sortingOrder, hasCollider);
            return mapLayer;
        }

        private static void PaintFlatGround(Tilemap groundTilemap, TileBase groundTile)
        {
            for (var x = -10; x <= 10; x++)
            {
                groundTilemap.SetTile(new Vector3Int(x, -3, 0), groundTile);
                groundTilemap.SetTile(new Vector3Int(x, -4, 0), groundTile);
            }

            groundTilemap.CompressBounds();
        }

        private static void PaintMonsterSpawnPoint(Tilemap spawnTilemap, TileBase spawnTile)
        {
            spawnTilemap.SetTile(new Vector3Int(-4, -2, 0), spawnTile);
            spawnTilemap.CompressBounds();
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(74, 106, 142, 255);
        }
    }
}
#endif
