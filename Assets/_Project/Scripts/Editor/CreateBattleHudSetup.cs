#if UNITY_EDITOR
using System.IO;
using IdleonGame.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.Editor
{
    public static class CreateBattleHudSetup
    {
        private const string TextureFolder = "Assets/_Project/Art/UI/BattleHUD";
        private const string PrefabFolder = "Assets/_Project/Resources/Prefabs/UI";
        private const string PrefabPath = PrefabFolder + "/BattleHUD.prefab";

        [MenuItem("IdleonGame/UI/Create Battle HUD")]
        public static void CreateAssets()
        {
            Directory.CreateDirectory(TextureFolder);
            Directory.CreateDirectory(PrefabFolder);

            var panelSprite = CreatePanelSprite("HudPanel", 64, 64, new Color32(51, 56, 62, 255), new Color32(155, 164, 171, 255));
            var moduleSprite = CreatePanelSprite("HudModule", 48, 48, new Color32(36, 41, 47, 255), new Color32(118, 128, 138, 255));
            var buttonSprite = CreatePanelSprite("HudButton", 72, 72, new Color32(78, 84, 92, 255), new Color32(180, 188, 196, 255));
            var hpSprite = CreateBarSprite("HudBarHP", new Color32(241, 68, 82, 255), new Color32(255, 151, 160, 255));
            var mpSprite = CreateBarSprite("HudBarMP", new Color32(52, 119, 224, 255), new Color32(104, 180, 255, 255));
            var xpSprite = CreateBarSprite("HudBarXP", new Color32(248, 179, 44, 255), new Color32(255, 224, 98, 255));
            var emptyBarSprite = CreateBarSprite("HudBarEmpty", new Color32(18, 23, 29, 255), new Color32(48, 54, 62, 255));

            var noClassIcon = CreateIconSprite("ClassNone", new Color32(90, 98, 107, 255), new Color32(210, 216, 223, 255), IconShape.None);
            var warriorIcon = CreateIconSprite("ClassWarrior", new Color32(154, 92, 39, 255), new Color32(236, 174, 80, 255), IconShape.Cross);
            var archerIcon = CreateIconSprite("ClassArcher", new Color32(56, 119, 93, 255), new Color32(118, 223, 151, 255), IconShape.Diagonal);
            var mageIcon = CreateIconSprite("ClassMage", new Color32(89, 77, 178, 255), new Color32(180, 144, 255, 255), IconShape.Diamond);
            var autoIcon = CreateIconSprite("ButtonAuto", new Color32(198, 65, 92, 255), new Color32(255, 205, 214, 255), IconShape.Circle);
            var attackIcon = CreateIconSprite("ButtonAttack", new Color32(64, 126, 196, 255), new Color32(197, 228, 255, 255), IconShape.Diagonal);
            var inventoryIcon = CreateIconSprite("ButtonInventory", new Color32(132, 83, 53, 255), new Color32(225, 173, 121, 255), IconShape.Box);
            var talentIcon = CreateIconSprite("ButtonTalent", new Color32(75, 148, 91, 255), new Color32(201, 255, 192, 255), IconShape.Star);
            var mapIcon = CreateIconSprite("ButtonMap", new Color32(179, 148, 62, 255), new Color32(255, 230, 135, 255), IconShape.Diamond);

            CreatePrefab(panelSprite, moduleSprite, buttonSprite, emptyBarSprite, hpSprite, mpSprite, xpSprite,
                noClassIcon, warriorIcon, archerIcon, mageIcon, autoIcon, attackIcon, inventoryIcon, talentIcon, mapIcon);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreatePrefab(
            Sprite panelSprite,
            Sprite moduleSprite,
            Sprite buttonSprite,
            Sprite emptyBarSprite,
            Sprite hpSprite,
            Sprite mpSprite,
            Sprite xpSprite,
            Sprite noClassIcon,
            Sprite warriorIcon,
            Sprite archerIcon,
            Sprite mageIcon,
            Sprite autoIcon,
            Sprite attackIcon,
            Sprite inventoryIcon,
            Sprite talentIcon,
            Sprite mapIcon)
        {
            if (File.Exists(PrefabPath))
            {
                Debug.Log($"Battle HUD prefab already exists, skipped: {PrefabPath}");
                return;
            }

            var root = new GameObject("BattleHUD", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(BattleHudWindowController));
            Stretch(root.GetComponent<RectTransform>());

            var panel = CreateImage("BottomPanel", root.transform, panelSprite);
            var panelRect = panel.rectTransform;
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(0f, 112f);

            var layout = panel.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.spacing = 8f;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = false;
            layout.childControlHeight = true;
            layout.childControlWidth = true;

            var profile = CreateModule("ProfileModule", panel.transform, moduleSprite, 292f);
            var vitals = CreateModule("VitalsModule", panel.transform, moduleSprite, 560f);
            var menu = CreateModule("MenuModule", panel.transform, moduleSprite, 0f);
            var menuLayout = menu.gameObject.AddComponent<HorizontalLayoutGroup>();
            menuLayout.padding = new RectOffset(8, 8, 8, 8);
            menuLayout.spacing = 8f;
            menuLayout.childForceExpandWidth = false;
            menuLayout.childForceExpandHeight = true;
            menuLayout.childControlWidth = true;
            menuLayout.childControlHeight = true;

            var classIcon = CreateImage("ClassIcon", profile, archerIcon);
            classIcon.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            classIcon.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            classIcon.rectTransform.anchoredPosition = new Vector2(42f, 0f);
            classIcon.rectTransform.sizeDelta = new Vector2(58f, 58f);

            var nameText = CreateText("PlayerName", profile, "YILOG222", 19, TextAnchor.MiddleLeft);
            SetRect(nameText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(92f, -22f), new Vector2(-104f, 24f));
            var classText = CreateText("ClassName", profile, "ARCHER", 18, TextAnchor.MiddleLeft);
            SetRect(classText.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(92f, 0f), new Vector2(-104f, 24f));
            var levelText = CreateText("Level", profile, "LV. 1", 18, TextAnchor.MiddleLeft);
            SetRect(levelText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(92f, 22f), new Vector2(-104f, 24f));

            var hpBar = CreateStatusBar(vitals, "HP", hpSprite, emptyBarSprite, 29f, out var hpFill, out var hpText);
            var mpBar = CreateStatusBar(vitals, "MP", mpSprite, emptyBarSprite, 0f, out var mpFill, out var mpText);
            var xpBar = CreateStatusBar(vitals, "XP", xpSprite, emptyBarSprite, -29f, out var xpFill, out var xpText);
            hpBar.SetAsLastSibling();
            mpBar.SetAsLastSibling();
            xpBar.SetAsLastSibling();

            var autoButton = CreateMenuButton(menu, "AutoButton", "AUTO", autoIcon, buttonSprite, out var autoStateText);
            var attackButton = CreateMenuButton(menu, "AttackButton", "ATTACK", attackIcon, buttonSprite, out _);
            var inventoryButton = CreateMenuButton(menu, "InventoryButton", "ITEMS", inventoryIcon, buttonSprite, out _);
            var talentButton = CreateMenuButton(menu, "TalentButton", "TALENT", talentIcon, buttonSprite, out _);
            var mapButton = CreateMenuButton(menu, "MapButton", "MAP", mapIcon, buttonSprite, out _);

            var serialized = new SerializedObject(root.GetComponent<BattleHudWindowController>());
            serialized.FindProperty("playerNameText").objectReferenceValue = nameText;
            serialized.FindProperty("classIconImage").objectReferenceValue = classIcon;
            serialized.FindProperty("classNameText").objectReferenceValue = classText;
            serialized.FindProperty("levelText").objectReferenceValue = levelText;
            serialized.FindProperty("hpFillImage").objectReferenceValue = hpFill;
            serialized.FindProperty("hpText").objectReferenceValue = hpText;
            serialized.FindProperty("mpFillImage").objectReferenceValue = mpFill;
            serialized.FindProperty("mpText").objectReferenceValue = mpText;
            serialized.FindProperty("xpFillImage").objectReferenceValue = xpFill;
            serialized.FindProperty("xpText").objectReferenceValue = xpText;
            serialized.FindProperty("autoHuntButton").objectReferenceValue = autoButton;
            serialized.FindProperty("autoHuntText").objectReferenceValue = autoStateText;
            serialized.FindProperty("attackButton").objectReferenceValue = attackButton;
            serialized.FindProperty("inventoryButton").objectReferenceValue = inventoryButton;
            serialized.FindProperty("talentButton").objectReferenceValue = talentButton;
            serialized.FindProperty("mapButton").objectReferenceValue = mapButton;
            serialized.FindProperty("noClassIcon").objectReferenceValue = noClassIcon;
            serialized.FindProperty("warriorIcon").objectReferenceValue = warriorIcon;
            serialized.FindProperty("archerIcon").objectReferenceValue = archerIcon;
            serialized.FindProperty("mageIcon").objectReferenceValue = mageIcon;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
        }

        private static RectTransform CreateModule(string name, Transform parent, Sprite sprite, float width)
        {
            var image = CreateImage(name, parent, sprite);
            var layoutElement = image.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = width > 0f ? width : -1f;
            layoutElement.flexibleWidth = width > 0f ? 0f : 1f;
            return image.rectTransform;
        }

        private static RectTransform CreateStatusBar(Transform parent, string label, Sprite fillSprite, Sprite emptySprite, float y, out Image fillImage, out Text valueText)
        {
            var row = new GameObject(label + "Row", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rowRect = row.GetComponent<RectTransform>();
            SetRect(rowRect, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, y), new Vector2(-18f, 24f));

            var labelText = CreateText(label + "Label", row.transform, label, 18, TextAnchor.MiddleLeft);
            SetRect(labelText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(22f, 0f), new Vector2(44f, 0f));

            var empty = CreateImage(label + "Empty", row.transform, emptySprite);
            SetRect(empty.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(296f, 0f), new Vector2(-88f, 18f));

            fillImage = CreateImage(label + "Fill", empty.transform, fillSprite);
            Stretch(fillImage.rectTransform);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0;
            fillImage.fillAmount = 1f;

            valueText = CreateText(label + "Value", empty.transform, "0/0", 16, TextAnchor.MiddleCenter);
            Stretch(valueText.rectTransform);
            return rowRect;
        }

        private static Button CreateMenuButton(Transform parent, string name, string label, Sprite iconSprite, Sprite buttonSprite, out Text stateText)
        {
            var image = CreateImage(name, parent, buttonSprite);
            var button = image.gameObject.AddComponent<Button>();
            var layoutElement = image.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 86f;

            var icon = CreateImage("Icon", image.transform, iconSprite);
            SetRect(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(38f, 34f));
            icon.raycastTarget = false;

            var labelText = CreateText("Label", image.transform, label, 14, TextAnchor.MiddleCenter);
            SetRect(labelText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 14f), new Vector2(0f, 24f));
            stateText = CreateText("State", image.transform, string.Empty, 13, TextAnchor.MiddleCenter);
            SetRect(stateText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -13f), new Vector2(0f, 20f));
            stateText.color = Color.red;
            return button;
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            var image = obj.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            return image;
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
            return CreateTexture(name, width, height, (texture, x, y) =>
            {
                var edge = x < 4 || y < 4 || x >= width - 4 || y >= height - 4;
                var innerEdge = x < 8 || y < 8 || x >= width - 8 || y >= height - 8;
                if (edge)
                {
                    return border;
                }

                return innerEdge ? Lerp(fill, border, 0.35f) : fill;
            });
        }

        private static Sprite CreateBarSprite(string name, Color32 fill, Color32 highlight)
        {
            const int width = 96;
            const int height = 18;
            return CreateTexture(name, width, height, (texture, x, y) =>
            {
                var edge = x < 2 || y < 2 || x >= width - 2 || y >= height - 2;
                if (edge)
                {
                    return new Color32(10, 14, 18, 255);
                }

                return y > height / 2 ? highlight : fill;
            });
        }

        private static Sprite CreateIconSprite(string name, Color32 fill, Color32 accent, IconShape shape)
        {
            const int size = 64;
            return CreateTexture(name, size, size, (texture, x, y) =>
            {
                var dx = x - size / 2;
                var dy = y - size / 2;
                var radius = Mathf.Sqrt(dx * dx + dy * dy);
                var inDisc = radius < 25f;
                var edge = radius > 25f && radius < 29f;
                if (edge)
                {
                    return accent;
                }

                if (!inDisc)
                {
                    return new Color32(0, 0, 0, 0);
                }

                var mark = shape switch
                {
                    IconShape.Cross => Mathf.Abs(dx) < 5 || Mathf.Abs(dy) < 5,
                    IconShape.Diagonal => Mathf.Abs(dx - dy) < 5,
                    IconShape.Diamond => Mathf.Abs(dx) + Mathf.Abs(dy) < 18,
                    IconShape.Box => Mathf.Abs(dx) < 18 && Mathf.Abs(dy) < 14,
                    IconShape.Star => (Mathf.Abs(dx) < 4 && Mathf.Abs(dy) < 22) || (Mathf.Abs(dy) < 4 && Mathf.Abs(dx) < 22),
                    IconShape.Circle => radius < 12f,
                    _ => false
                };

                return mark ? accent : fill;
            });
        }

        private static Sprite CreateTexture(string name, int width, int height, System.Func<Texture2D, int, int, Color32> pixel)
        {
            var path = $"{TextureFolder}/{name}.png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        texture.SetPixel(x, y, pixel(texture, x, y));
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
            None,
            Cross,
            Diagonal,
            Diamond,
            Box,
            Star,
            Circle
        }
    }
}
#endif
