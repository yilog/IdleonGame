#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using IdleonGame.Cameras;
using IdleonGame.Combat;
using IdleonGame.Core;
using IdleonGame.Levels;
using IdleonGame.Map;
using IdleonGame.Monster;
using IdleonGame.Navigation;
using IdleonGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace IdleonGame.Editor
{
    public static class CreateBattleLevelSceneSetup
    {
        private const string BattleScenePath = "Assets/_Project/Scenes/Battle.unity";
        private const string LevelSceneFolder = "Assets/_Project/Scenes/Levels";
        private const string LevelDataFolder = "Assets/_Project/ScriptableObjects/Levels";
        private const string MapDataFolder = "Assets/_Project/ScriptableObjects/Maps";
        private const string LevelDatabasePath = "Assets/_Project/Resources/LevelDatabase.asset";
        private const string GroundTilePath = "Assets/_Project/Tilemaps/Tiles/TestGroundTile.asset";
        private const string RopeTilePath = "Assets/_Project/Tilemaps/Tiles/TestRopeTile.asset";
        private const string MonsterSpawnTilePath = "Assets/_Project/Tilemaps/Tiles/TestMonsterSpawnTile.asset";
        private const string BasicAttackPath = "Assets/_Project/ScriptableObjects/Skills/PlayerBasicAttack.asset";
        private const string ArrowAttackPath = "Assets/_Project/ScriptableObjects/Skills/PlayerArrowAttack.asset";
        private const string TestScenePath = "Assets/_Project/Scenes/Maps/Test_Battle_Tilemap.unity";

        [MenuItem("IdleonGame/Setup/Create Battle And Level Scenes")]
        public static void CreateAll()
        {
            EnsureFolders();
            EnsureLayers();

            var level1 = CreateLevel("level1_1", "Level 1-1", new Vector2(-8f, -1.75f), 0);
            var level2 = CreateLevel("level1_2", "Level 1-2", new Vector2(-8f, -1.75f), 1);
            CreateLevelDatabase(level1, level2);
            CreateBattleScene();
            SyncBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/_Project/Scenes");
            Directory.CreateDirectory(LevelSceneFolder);
            Directory.CreateDirectory(LevelDataFolder);
            Directory.CreateDirectory(MapDataFolder);
            Directory.CreateDirectory("Assets/_Project/Resources");
        }

        private static void CreateBattleScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Battle";

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(74, 106, 142, 255);
            cameraObject.AddComponent<MapCameraController>();

            var runtimeRoot = new GameObject("BattleRuntime");
            var battleController = runtimeRoot.AddComponent<BattleSceneController>();
            var transitionService = runtimeRoot.AddComponent<LevelSceneTransitionService>();
            var loadingOverlay = runtimeRoot.AddComponent<LoadingOverlay>();

            AssignBattleController(battleController);
            AssignTransitionService(transitionService, battleController, loadingOverlay);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, BattleScenePath);
        }

        private static LevelDefinition CreateLevel(string levelId, string displayName, Vector2 spawnPosition, int variant)
        {
            var scenePath = $"{LevelSceneFolder}/{levelId}.unity";
            var mapDefinition = CreateMapDefinition(levelId, displayName, spawnPosition);
            var groundTile = AssetDatabase.LoadAssetAtPath<TileBase>(GroundTilePath);
            var ropeTile = AssetDatabase.LoadAssetAtPath<TileBase>(RopeTilePath);
            var monsterSpawnTile = AssetDatabase.LoadAssetAtPath<TileBase>(MonsterSpawnTilePath);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = levelId;

            var mapRoot = new GameObject($"BattleMap_{levelId}");
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
            var decorationLayer = layers.Find(layer => layer.LayerType == TilemapLayerType.Decoration);
            var monsterSpawnLayer = layers.Find(layer => layer.LayerType == TilemapLayerType.MonsterSpawn);

            PaintGround(groundLayer.Tilemap, groundTile, variant);
            decorationLayer.gameObject.AddComponent<RopeTilemap>().Configure(ropeTile);
            PaintMonsterSpawnPoint(monsterSpawnLayer.Tilemap, monsterSpawnTile, variant);
            monsterSpawnLayer.gameObject.AddComponent<MonsterSpawnTilemap>().Configure(groundLayer.Tilemap);

            var navigation = new GameObject("TilemapNavigation");
            navigation.AddComponent<TilemapNavigationPathfinder>();

            controller.EditorConfigure(mapDefinition, new BoundsInt(-10, -4, 0, 21, 9, 1), layers);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);

            return CreateLevelDefinition(levelId, displayName, scenePath, spawnPosition);
        }

        private static MapSceneDefinition CreateMapDefinition(string levelId, string displayName, Vector2 spawnPosition)
        {
            var path = $"{MapDataFolder}/{levelId}_Map.asset";
            var mapDefinition = AssetDatabase.LoadAssetAtPath<MapSceneDefinition>(path);
            if (mapDefinition == null)
            {
                mapDefinition = ScriptableObject.CreateInstance<MapSceneDefinition>();
                AssetDatabase.CreateAsset(mapDefinition, path);
            }

            mapDefinition.EditorSetData(
                levelId,
                displayName,
                new Vector2Int(-10, -4),
                new Vector2Int(21, 9),
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
                    new MapSpawnPointData { spawnPointId = "default", position = spawnPosition }
                },
                new List<MapPortalData>());

            EditorUtility.SetDirty(mapDefinition);
            return mapDefinition;
        }

        private static LevelDefinition CreateLevelDefinition(string levelId, string displayName, string scenePath, Vector2 spawnPosition)
        {
            var path = $"{LevelDataFolder}/{levelId}.asset";
            var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
            if (level == null)
            {
                level = ScriptableObject.CreateInstance<LevelDefinition>();
                AssetDatabase.CreateAsset(level, path);
            }

            level.EditorSetData(levelId, displayName, levelId, scenePath, "default", spawnPosition);
            EditorUtility.SetDirty(level);
            return level;
        }

        private static void CreateLevelDatabase(LevelDefinition level1, LevelDefinition level2)
        {
            var database = AssetDatabase.LoadAssetAtPath<LevelDatabase>(LevelDatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<LevelDatabase>();
                AssetDatabase.CreateAsset(database, LevelDatabasePath);
            }

            database.EditorSetData(new[] { level1, level2 }, "level1_1");
            EditorUtility.SetDirty(database);
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

        private static void PaintGround(Tilemap groundTilemap, TileBase groundTile, int variant)
        {
            for (var x = -10; x <= 10; x++)
            {
                groundTilemap.SetTile(new Vector3Int(x, -3, 0), groundTile);
                groundTilemap.SetTile(new Vector3Int(x, -4, 0), groundTile);
            }

            if (variant == 1)
            {
                for (var x = 2; x <= 7; x++)
                {
                    groundTilemap.SetTile(new Vector3Int(x, 0, 0), groundTile);
                }
            }

            groundTilemap.CompressBounds();
        }

        private static void PaintMonsterSpawnPoint(Tilemap spawnTilemap, TileBase spawnTile, int variant)
        {
            spawnTilemap.SetTile(new Vector3Int(variant == 0 ? -4 : 4, -2, 0), spawnTile);
            spawnTilemap.CompressBounds();
        }

        private static void AssignBattleController(BattleSceneController battleController)
        {
            var serialized = new SerializedObject(battleController);
            serialized.FindProperty("basicAttack").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AttackDefinition>(BasicAttackPath);
            serialized.FindProperty("rangedAttack").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AttackDefinition>(ArrowAttackPath);
            serialized.FindProperty("initialPlayerPosition").vector2Value = new Vector2(-8f, -1.75f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignTransitionService(LevelSceneTransitionService transitionService, BattleSceneController battleController, LoadingOverlay loadingOverlay)
        {
            var serialized = new SerializedObject(transitionService);
            serialized.FindProperty("levelDatabase").objectReferenceValue = AssetDatabase.LoadAssetAtPath<LevelDatabase>(LevelDatabasePath);
            serialized.FindProperty("battleController").objectReferenceValue = battleController;
            serialized.FindProperty("loadingOverlay").objectReferenceValue = loadingOverlay;
            serialized.FindProperty("initialLevelId").stringValue = "level1_1";
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SyncBuildSettings()
        {
            var paths = new List<string>
            {
                BattleScenePath,
                $"{LevelSceneFolder}/level1_1.unity",
                $"{LevelSceneFolder}/level1_2.unity"
            };

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TestScenePath) != null)
            {
                paths.Add(TestScenePath);
            }

            EditorBuildSettings.scenes = paths.ConvertAll(path => new EditorBuildSettingsScene(path, true)).ToArray();
        }

        private static void EnsureLayers()
        {
            EnsureLayer(8, GameLayerNames.Player);
            EnsureLayer(9, GameLayerNames.Monster);
        }

        private static void EnsureLayer(int index, string layerName)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            var layer = layers.GetArrayElementAtIndex(index);
            if (string.IsNullOrEmpty(layer.stringValue) || layer.stringValue == layerName)
            {
                layer.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning($"Layer {index} is already set to {layer.stringValue}; expected {layerName}.");
            }
        }
    }
}
#endif
