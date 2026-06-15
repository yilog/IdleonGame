#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using IdleonGame.Data;
using IdleonGame.Talents;
using IdleonGame.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.Editor
{
    public static class CreateTalentSetup
    {
        private const string TextureFolder = "Assets/_Project/Art/UI/Talents";
        private const string TalentFolder = "Assets/_Project/ScriptableObjects/Talents";
        private const string PrefabFolder = "Assets/_Project/Resources/Prefabs/UI";
        private const string DatabasePath = "Assets/_Project/Resources/TalentDatabase.asset";
        private const string PrefabPath = PrefabFolder + "/UITalent.prefab";

        [MenuItem("IdleonGame/UI/Create Talent Setup")]
        public static void CreateAssets()
        {
            Directory.CreateDirectory(TextureFolder);
            Directory.CreateDirectory(TalentFolder);
            Directory.CreateDirectory(PrefabFolder);

            var panel = CreatePanelSprite("TalentPanel", 64, 64, new Color32(188, 166, 118, 255), new Color32(87, 68, 45, 255));
            var node = CreatePanelSprite("TalentNode", 64, 64, new Color32(47, 53, 62, 255), new Color32(199, 207, 216, 255));
            var nodeSelected = CreatePanelSprite("TalentNodeSelected", 64, 64, new Color32(67, 87, 112, 255), new Color32(255, 228, 91, 255));
            var tabActive = CreatePanelSprite("TalentTabActive", 64, 32, new Color32(102, 78, 44, 255), new Color32(238, 211, 145, 255));
            var tabInactive = CreatePanelSprite("TalentTabInactive", 64, 32, new Color32(67, 61, 53, 255), new Color32(147, 132, 103, 255));
            var button = CreatePanelSprite("TalentButton", 64, 42, new Color32(76, 170, 83, 255), new Color32(229, 255, 206, 255));
            var hpIcon = CreateIconSprite("Talent_MaxHealth", new Color32(224, 55, 91, 255), new Color32(255, 204, 222, 255), IconShape.Heart);
            var mpIcon = CreateIconSprite("Talent_MaxMana", new Color32(52, 119, 224, 255), new Color32(190, 226, 255, 255), IconShape.Droplet);
            var arrowIcon = CreateIconSprite("Talent_ArrowPower", new Color32(63, 115, 86, 255), new Color32(203, 255, 206, 255), IconShape.Arrow);

            var talents = new List<TalentDefinition>
            {
                CreateTalent(
                    "character_max_health",
                    "Health Booster",
                    "Increases Max HP.",
                    hpIcon,
                    TalentType.Character,
                    PlayerClassType.None,
                    100,
                    TalentEffectType.MaxHealth,
                    "Each level increases Max HP by 1 + level x 0.1."),
                CreateTalent(
                    "character_max_mana",
                    "Mana Booster",
                    "Increases Max MP.",
                    mpIcon,
                    TalentType.Character,
                    PlayerClassType.None,
                    100,
                    TalentEffectType.MaxMana,
                    "Each level increases Max MP by 1 + level x 0.1."),
                CreateTalent(
                    "archer_arrow_power",
                    "Arrow Power",
                    "Increases arrow skill damage.",
                    arrowIcon,
                    TalentType.Class,
                    PlayerClassType.Archer,
                    100,
                    TalentEffectType.ArrowDamagePercent,
                    "Each level increases arrow damage by 2%.")
            };

            CreateDatabase(talents);
            CreatePrefab(panel, node, nodeSelected, tabActive, tabInactive, button);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static TalentDefinition CreateTalent(
            string id,
            string name,
            string description,
            Sprite icon,
            TalentType type,
            PlayerClassType classType,
            int maxLevel,
            TalentEffectType effect,
            string upgradeDescription)
        {
            var path = $"{TalentFolder}/{id}.asset";
            var talent = AssetDatabase.LoadAssetAtPath<TalentDefinition>(path);
            if (talent == null)
            {
                talent = ScriptableObject.CreateInstance<TalentDefinition>();
                AssetDatabase.CreateAsset(talent, path);
            }

            talent.EditorSetData(id, name, description, icon, type, classType, maxLevel, effect, upgradeDescription);
            EditorUtility.SetDirty(talent);
            return talent;
        }

        private static void CreateDatabase(IEnumerable<TalentDefinition> talents)
        {
            var database = AssetDatabase.LoadAssetAtPath<TalentDatabase>(DatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<TalentDatabase>();
                AssetDatabase.CreateAsset(database, DatabasePath);
            }

            database.EditorSetData(talents);
            EditorUtility.SetDirty(database);
        }

        private static void CreatePrefab(Sprite panel, Sprite node, Sprite nodeSelected, Sprite tabActive, Sprite tabInactive, Sprite buttonSprite)
        {
            if (File.Exists(PrefabPath))
            {
                Debug.Log($"UITalent prefab already exists, skipped: {PrefabPath}");
                return;
            }

            var root = new GameObject("UITalent", typeof(RectTransform), typeof(Image), typeof(Canvas), typeof(GraphicRaycaster), typeof(UITalentWindowController));
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(560f, 640f);
            var rootImage = root.GetComponent<Image>();
            rootImage.sprite = panel;
            rootImage.type = Image.Type.Sliced;

            var title = CreateText("Title", root.transform, "TALENTS", 32, TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -30f), new Vector2(-90f, 48f));

            var closeButton = CreateButton("CloseButton", root.transform, "X", buttonSprite);
            SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -28f), new Vector2(42f, 38f));

            var characterTab = CreateButton("CharacterTab", root.transform, "Character Talents", tabActive);
            SetRect(characterTab.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(116f, -72f), new Vector2(210f, 38f));
            var classTab = CreateButton("ClassTab", root.transform, "Class Talents", tabInactive);
            SetRect(classTab.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(330f, -72f), new Vector2(210f, 38f));

            var listRoot = new GameObject("TalentListRoot", typeof(RectTransform), typeof(GridLayoutGroup));
            listRoot.transform.SetParent(root.transform, false);
            var listRect = listRoot.GetComponent<RectTransform>();
            SetRect(listRect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -195f), new Vector2(-60f, 210f));
            var grid = listRoot.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(76f, 86f);
            grid.spacing = new Vector2(10f, 10f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            grid.childAlignment = TextAnchor.UpperCenter;

            var detailPanel = CreateImage("DetailPanel", root.transform, panel);
            detailPanel.type = Image.Type.Sliced;
            SetRect(detailPanel.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 115f), new Vector2(-48f, 210f));

            var selectedIcon = CreateImage("SelectedIcon", detailPanel.transform, nodeSelected);
            SetRect(selectedIcon.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(58f, -58f), new Vector2(74f, 74f));

            var selectedName = CreateText("SelectedName", detailPanel.transform, string.Empty, 20, TextAnchor.MiddleLeft);
            SetRect(selectedName.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(176f, -34f), new Vector2(-210f, 30f));
            var selectedDesc = CreateText("SelectedDescription", detailPanel.transform, string.Empty, 16, TextAnchor.UpperLeft);
            SetRect(selectedDesc.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(176f, -86f), new Vector2(-210f, 62f));
            var nextEffect = CreateText("NextEffect", detailPanel.transform, string.Empty, 16, TextAnchor.MiddleLeft);
            SetRect(nextEffect.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(176f, 46f), new Vector2(-210f, 34f));
            var points = CreateText("Points", detailPanel.transform, "POINTS LEFT: 0", 16, TextAnchor.MiddleCenter);
            SetRect(points.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-86f, 28f), new Vector2(140f, 30f));

            var upgradeButton = CreateButton("UpgradeButton", detailPanel.transform, "UPGRADE", buttonSprite);
            SetRect(upgradeButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-86f, -56f), new Vector2(128f, 56f));

            var serialized = new SerializedObject(root.GetComponent<UITalentWindowController>());
            serialized.FindProperty("talentListRoot").objectReferenceValue = listRect;
            serialized.FindProperty("characterTabButton").objectReferenceValue = characterTab;
            serialized.FindProperty("classTabButton").objectReferenceValue = classTab;
            serialized.FindProperty("closeButton").objectReferenceValue = closeButton;
            serialized.FindProperty("selectedIconImage").objectReferenceValue = selectedIcon;
            serialized.FindProperty("selectedNameText").objectReferenceValue = selectedName;
            serialized.FindProperty("selectedDescriptionText").objectReferenceValue = selectedDesc;
            serialized.FindProperty("selectedNextEffectText").objectReferenceValue = nextEffect;
            serialized.FindProperty("pointsText").objectReferenceValue = points;
            serialized.FindProperty("upgradeButton").objectReferenceValue = upgradeButton;
            serialized.FindProperty("nodeBackgroundSprite").objectReferenceValue = node;
            serialized.FindProperty("selectedNodeBackgroundSprite").objectReferenceValue = nodeSelected;
            serialized.FindProperty("tabActiveSprite").objectReferenceValue = tabActive;
            serialized.FindProperty("tabInactiveSprite").objectReferenceValue = tabInactive;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            var image = obj.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            return image;
        }

        private static Button CreateButton(string name, Transform parent, string label, Sprite sprite)
        {
            var image = CreateImage(name, parent, sprite);
            var button = image.gameObject.AddComponent<Button>();
            var text = CreateText("Label", image.transform, label, 16, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return button;
        }

        private static Text CreateText(string name, Transform parent, string content, int fontSize, TextAnchor alignment)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);
            var text = obj.GetComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static Sprite CreatePanelSprite(string name, int width, int height, Color32 fill, Color32 border)
        {
            return CreateTexture(name, width, height, (x, y) =>
            {
                var edge = x < 4 || y < 4 || x >= width - 4 || y >= height - 4;
                return edge ? border : fill;
            });
        }

        private static Sprite CreateIconSprite(string name, Color32 fill, Color32 accent, IconShape shape)
        {
            const int size = 64;
            return CreateTexture(name, size, size, (x, y) =>
            {
                var dx = x - size / 2;
                var dy = y - size / 2;
                var radius = Mathf.Sqrt(dx * dx + dy * dy);
                if (radius > 28f)
                {
                    return new Color32(0, 0, 0, 0);
                }

                var mark = shape switch
                {
                    IconShape.Heart => (dy > -10 && Mathf.Pow(dx / 18f, 2f) + Mathf.Pow((dy - 5) / 14f, 2f) < 1f) || (dy <= 4 && Mathf.Abs(dx) < 18 - Mathf.Abs(dy)),
                    IconShape.Droplet => radius < 16f || (dy > 0 && Mathf.Abs(dx) < 15 - dy * 0.35f),
                    IconShape.Arrow => Mathf.Abs(dy) < 5 || (dx > 8 && Mathf.Abs(dy) < 18 - dx * 0.45f),
                    _ => false
                };

                return mark ? accent : fill;
            });
        }

        private static Sprite CreateTexture(string name, int width, int height, System.Func<int, int, Color32> pixel)
        {
            var path = $"{TextureFolder}/{name}.png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        texture.SetPixel(x, y, pixel(x, y));
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

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        private enum IconShape
        {
            Heart,
            Droplet,
            Arrow
        }
    }
}
#endif
