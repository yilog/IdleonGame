#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using IdleonGame.Levels;
using IdleonGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IdleonGame.Editor
{
    public static class CreateUIMapAndLevelSetup
    {
        private const string UiTextureFolder = "Assets/_Project/Art/UI/Map";
        private const string UiPrefabFolder = "Assets/_Project/Resources/Prefabs/UI";
        private const string UiMapPrefabPath = UiPrefabFolder + "/UIMap.prefab";
        private const string LevelAssetFolder = "Assets/_Project/ScriptableObjects/Levels";
        private const string LevelSceneFolder = "Assets/_Project/Scenes/Levels";
        private const string LevelDatabasePath = "Assets/_Project/Resources/LevelDatabase.asset";
        private static readonly string[] FixedLevelIds =
        {
            "level1_1",
            "level1_2",
            "level1_3",
            "level2_1",
            "level2_2",
            "level2_3"
        };

        private static readonly Vector2[] FixedLevelNodePositions =
        {
            new(120f, -92f),
            new(290f, -92f),
            new(460f, -92f),
            new(120f, -210f),
            new(290f, -210f),
            new(460f, -210f)
        };

        [MenuItem("IdleonGame/UI/Create UIMap And Level Setup")]
        public static void CreateAssets()
        {
            Directory.CreateDirectory(UiTextureFolder);
            Directory.CreateDirectory(UiPrefabFolder);
            Directory.CreateDirectory(LevelAssetFolder);
            Directory.CreateDirectory(LevelSceneFolder);

            var mapBackground = CreateTexture("MapBackground", 720, 420, new Color32(59, 138, 186, 255), new Color32(65, 170, 98, 255));
            var panel = CreateTexture("MapPanel", 32, 32, new Color32(30, 34, 42, 240), new Color32(55, 62, 72, 255));
            var unlockedNode = CreateTexture("MapNodeUnlocked", 96, 64, new Color32(217, 165, 47, 255), new Color32(255, 225, 103, 255));
            var lockedNode = CreateTexture("MapNodeLocked", 96, 64, new Color32(83, 87, 92, 255), new Color32(140, 144, 150, 255));
            var currentNode = CreateTexture("MapNodeCurrent", 104, 72, new Color32(136, 57, 214, 255), new Color32(238, 114, 255, 255));
            var playerMarker = CreateTexture("MapPlayerMarker", 32, 32, new Color32(246, 221, 92, 255), new Color32(214, 76, 48, 255));

            CreateUIMapPrefab(panel, mapBackground, unlockedNode, lockedNode, currentNode, playerMarker);
            EnsureUIMapFixedNodes(unlockedNode, playerMarker);
            CreateLevelScenesAndDefinitions();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("IdleonGame/UI/Ensure UIMap Fixed Nodes")]
        public static void EnsureUIMapFixedNodes()
        {
            var unlockedNode = AssetDatabase.LoadAssetAtPath<Sprite>($"{UiTextureFolder}/MapNodeUnlocked.png");
            var playerMarker = AssetDatabase.LoadAssetAtPath<Sprite>($"{UiTextureFolder}/MapPlayerMarker.png");
            EnsureUIMapFixedNodes(unlockedNode, playerMarker);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Sprite CreateTexture(string name, int width, int height, Color32 primary, Color32 secondary)
        {
            var path = $"{UiTextureFolder}/{name}.png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var border = x < 3 || y < 3 || x >= width - 3 || y >= height - 3;
                        var checker = ((x / 16) + (y / 16)) % 2 == 0;
                        texture.SetPixel(x, y, border ? secondary : checker ? primary : Lerp(primary, secondary, 0.25f));
                    }
                }

                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);
            }

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Color32 Lerp(Color32 a, Color32 b, float t)
        {
            return new Color32(
                (byte)Mathf.Lerp(a.r, b.r, t),
                (byte)Mathf.Lerp(a.g, b.g, t),
                (byte)Mathf.Lerp(a.b, b.b, t),
                (byte)Mathf.Lerp(a.a, b.a, t));
        }

        private static void CreateUIMapPrefab(Sprite panelSprite, Sprite backgroundSprite, Sprite unlockedNode, Sprite lockedNode, Sprite currentNode, Sprite playerMarker)
        {
            if (File.Exists(UiMapPrefabPath))
            {
                Debug.Log($"UIMap prefab already exists, skipped: {UiMapPrefabPath}");
                return;
            }

            var root = new GameObject("UIMap", typeof(RectTransform), typeof(Image), typeof(Canvas), typeof(GraphicRaycaster), typeof(UIMapWindowController));
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(820f, 500f);
            var rootImage = root.GetComponent<Image>();
            rootImage.sprite = panelSprite;
            rootImage.type = Image.Type.Sliced;
            rootImage.color = Color.white;

            var header = CreateRect("Header", root.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -26f), new Vector2(-20f, 42f));
            var title = CreateText("Title", header, "Current Map", 22, TextAnchor.MiddleCenter);

            var closeButton = CreateButton("CloseButton", header, "X", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-28f, 0f), new Vector2(36f, 30f));
            var body = CreateRect("Body", root.transform, Vector2.zero, Vector2.one, new Vector2(0f, -16f), new Vector2(-28f, -70f));
            var mapImageObject = new GameObject("MapBackground", typeof(RectTransform), typeof(Image));
            mapImageObject.transform.SetParent(body, false);
            var mapImage = mapImageObject.GetComponent<Image>();
            mapImage.sprite = backgroundSprite;
            mapImage.type = Image.Type.Sliced;
            mapImage.raycastTarget = false;
            Stretch(mapImage.rectTransform);

            var nodeRoot = CreateRect("NodeRoot", body, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var controller = root.GetComponent<UIMapWindowController>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("nodeRoot").objectReferenceValue = nodeRoot;
            serialized.FindProperty("closeButton").objectReferenceValue = closeButton;
            serialized.FindProperty("titleText").objectReferenceValue = title;
            serialized.FindProperty("unlockedNodeSprite").objectReferenceValue = unlockedNode;
            serialized.FindProperty("lockedNodeSprite").objectReferenceValue = lockedNode;
            serialized.FindProperty("currentNodeSprite").objectReferenceValue = currentNode;
            serialized.FindProperty("playerMarkerSprite").objectReferenceValue = playerMarker;
            SetFixedLevelIds(serialized);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EnsureFixedNodeObjects(nodeRoot, unlockedNode, playerMarker);
            PrefabUtility.SaveAsPrefabAsset(root, UiMapPrefabPath);
            Object.DestroyImmediate(root);
        }

        private static void EnsureUIMapFixedNodes(Sprite unlockedNode, Sprite playerMarker)
        {
            if (!File.Exists(UiMapPrefabPath))
            {
                Debug.LogWarning($"UIMap prefab does not exist yet: {UiMapPrefabPath}");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(UiMapPrefabPath);
            try
            {
                var controller = root.GetComponent<UIMapWindowController>();
                var nodeRootTransform = root.transform.Find("Body/NodeRoot");
                var nodeRoot = nodeRootTransform as RectTransform;
                if (controller == null || nodeRoot == null)
                {
                    Debug.LogWarning("UIMap prefab is missing UIMapWindowController or Body/NodeRoot.");
                    return;
                }

                EnsureFixedNodeObjects(nodeRoot, unlockedNode, playerMarker);

                var serialized = new SerializedObject(controller);
                serialized.FindProperty("nodeRoot").objectReferenceValue = nodeRoot;
                SetFixedLevelIds(serialized);
                serialized.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, UiMapPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void EnsureFixedNodeObjects(RectTransform nodeRoot, Sprite unlockedNode, Sprite playerMarker)
        {
            for (var i = 0; i < FixedLevelIds.Length; i++)
            {
                var levelId = FixedLevelIds[i];
                var node = nodeRoot.Find($"{levelId}_Node") as RectTransform;
                if (node == null)
                {
                    node = CreateMapNode($"{levelId}_Node", nodeRoot, unlockedNode, playerMarker);
                }

                node.anchorMin = new Vector2(0f, 1f);
                node.anchorMax = new Vector2(0f, 1f);
                node.pivot = new Vector2(0.5f, 0.5f);
                node.sizeDelta = new Vector2(118f, 76f);
                node.anchoredPosition = FixedLevelNodePositions[i];
            }
        }

        private static RectTransform CreateMapNode(string name, Transform parent, Sprite nodeSprite, Sprite playerMarker)
        {
            var nodeObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            nodeObject.transform.SetParent(parent, false);
            var node = nodeObject.GetComponent<RectTransform>();

            var image = nodeObject.GetComponent<Image>();
            image.sprite = nodeSprite;
            image.type = Image.Type.Sliced;
            image.raycastTarget = true;

            var button = nodeObject.GetComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;

            var label = CreateText("Label", nodeObject.transform, name.Replace("_Node", string.Empty), 16, TextAnchor.MiddleCenter);
            label.rectTransform.offsetMin = new Vector2(6f, 6f);
            label.rectTransform.offsetMax = new Vector2(-6f, -6f);

            var markerObject = new GameObject("PlayerMarker", typeof(RectTransform), typeof(Image));
            markerObject.transform.SetParent(nodeObject.transform, false);
            var marker = markerObject.GetComponent<Image>();
            marker.sprite = playerMarker;
            marker.raycastTarget = false;
            markerObject.SetActive(false);

            var markerRect = markerObject.GetComponent<RectTransform>();
            markerRect.anchorMin = new Vector2(0.5f, 1f);
            markerRect.anchorMax = new Vector2(0.5f, 1f);
            markerRect.pivot = new Vector2(0.5f, 0f);
            markerRect.sizeDelta = new Vector2(28f, 28f);
            markerRect.anchoredPosition = new Vector2(0f, 6f);

            return node;
        }

        private static void SetFixedLevelIds(SerializedObject serialized)
        {
            var fixedLevelIds = serialized.FindProperty("fixedLevelIds");
            fixedLevelIds.arraySize = FixedLevelIds.Length;
            for (var i = 0; i < FixedLevelIds.Length; i++)
            {
                fixedLevelIds.GetArrayElementAtIndex(i).stringValue = FixedLevelIds[i];
            }
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            return rect;
        }

        private static Text CreateText(string name, Transform parent, string content, int fontSize, TextAnchor alignment)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var text = obj.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            Stretch(text.rectTransform);
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            obj.GetComponent<Image>().color = new Color32(103, 54, 54, 255);
            CreateText("Label", obj.transform, label, 18, TextAnchor.MiddleCenter);
            return obj.GetComponent<Button>();
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void CreateLevelScenesAndDefinitions()
        {
            var levels = new List<LevelDefinition>();
            var ids = new[] { "level1_1", "level1_2", "level1_3", "level2_1", "level2_2", "level2_3" };
            var displayNames = new[] { "Level 1-1", "Level 1-2", "Level 1-3", "Level 2-1", "Level 2-2", "Level 2-3" };

            for (var i = 0; i < ids.Length; i++)
            {
                var levelId = ids[i];
                var scenePath = $"{LevelSceneFolder}/{levelId}.unity";
                if (!File.Exists(scenePath))
                {
                    var sourceScene = i % 2 == 0 ? $"{LevelSceneFolder}/level1_1.unity" : $"{LevelSceneFolder}/level1_2.unity";
                    AssetDatabase.CopyAsset(sourceScene, scenePath);
                }

                var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>($"{LevelAssetFolder}/{levelId}.asset");
                if (level == null)
                {
                    level = ScriptableObject.CreateInstance<LevelDefinition>();
                    AssetDatabase.CreateAsset(level, $"{LevelAssetFolder}/{levelId}.asset");
                }

                LevelUnlockCondition[] conditions = null;
                if (levelId == "level2_1")
                {
                    var condition = new LevelUnlockCondition();
                    condition.EditorSetKillMonsterCondition("rat", 3);
                    conditions = new[] { condition };
                }

                level.EditorSetData(levelId, displayNames[i], levelId, scenePath, "default", new Vector2(-8f, -2f), conditions);
                EditorUtility.SetDirty(level);
                levels.Add(level);
            }

            var database = AssetDatabase.LoadAssetAtPath<LevelDatabase>(LevelDatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<LevelDatabase>();
                AssetDatabase.CreateAsset(database, LevelDatabasePath);
            }

            database.EditorSetData(levels, "level1_1");
            EditorUtility.SetDirty(database);
            EnsureBuildSettings(ids);
        }

        private static void EnsureBuildSettings(string[] levelIds)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var levelId in levelIds)
            {
                var scenePath = $"{LevelSceneFolder}/{levelId}.unity";
                var exists = scenes.Exists(scene => scene.path == scenePath);
                if (!exists)
                {
                    scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                }
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
