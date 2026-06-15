#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using IdleonGame.UI;
using IdleonGame.Upgrades;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.Editor
{
    public static class CreateUpgradeSetup
    {
        private const string TextureFolder = "Assets/_Project/Art/UI/Upgrades";
        private const string UpgradeFolder = "Assets/_Project/ScriptableObjects/Upgrades";
        private const string PrefabFolder = "Assets/_Project/Resources/Prefabs/UI";
        private const string DatabasePath = "Assets/_Project/Resources/UpgradeDatabase.asset";
        private const string PrefabPath = PrefabFolder + "/UIUpgrade.prefab";
        private const string ItemPrefabPath = PrefabFolder + "/UpgradeItem.prefab";

        [MenuItem("IdleonGame/UI/Create Upgrade Setup")]
        public static void CreateAssets()
        {
            Directory.CreateDirectory(TextureFolder);
            Directory.CreateDirectory(UpgradeFolder);
            Directory.CreateDirectory(PrefabFolder);

            var panel = CreatePanelSprite("UpgradePanel", 64, 64, new Color32(96, 63, 38, 255), new Color32(220, 159, 69, 255));
            var row = CreatePanelSprite("UpgradeRow", 64, 64, new Color32(76, 49, 32, 255), new Color32(145, 98, 55, 255));
            var button = CreatePanelSprite("UpgradeButton", 72, 42, new Color32(141, 88, 48, 255), new Color32(255, 197, 128, 255));
            var damageIcon = CreateIconSprite("Upgrade_BiggerDamage", new Color32(66, 76, 86, 255), new Color32(226, 226, 232, 255));

            var upgrade = CreateUpgrade(
                "bigger_damage",
                "Bigger Damage",
                damageIcon,
                "Increase player damage.",
                UpgradeEffectType.AttackDamage,
                1000,
                6 * CurrencyFormatter.CopperPerGold,
                2 * CurrencyFormatter.CopperPerGold);

            CreateDatabase(new[] { upgrade });
            var itemPrefab = CreateItemPrefab(row, button);
            CreatePrefab(panel, itemPrefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static UpgradeDefinition CreateUpgrade(
            string id,
            string name,
            Sprite icon,
            string description,
            UpgradeEffectType effect,
            int maxLevel,
            int baseCost,
            int costIncreasePerLevel)
        {
            var path = $"{UpgradeFolder}/{id}.asset";
            var upgrade = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(path);
            if (upgrade == null)
            {
                upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
                AssetDatabase.CreateAsset(upgrade, path);
            }

            upgrade.EditorSetData(id, name, icon, description, effect, maxLevel, baseCost, costIncreasePerLevel);
            EditorUtility.SetDirty(upgrade);
            return upgrade;
        }

        private static void CreateDatabase(IEnumerable<UpgradeDefinition> upgrades)
        {
            var database = AssetDatabase.LoadAssetAtPath<UpgradeDatabase>(DatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<UpgradeDatabase>();
                AssetDatabase.CreateAsset(database, DatabasePath);
            }

            database.EditorSetData(upgrades);
            EditorUtility.SetDirty(database);
        }

        private static UIUpgradeItemView CreateItemPrefab(Sprite row, Sprite buttonSprite)
        {
            if (!File.Exists(ItemPrefabPath))
            {
                var root = new GameObject("UpgradeItem", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(UIUpgradeItemView));
                var rect = root.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(680f, 96f);
                var image = root.GetComponent<Image>();
                image.sprite = row;
                image.type = Image.Type.Sliced;
                root.GetComponent<LayoutElement>().preferredHeight = 96f;

                var icon = CreateImage("Icon", root.transform, null);
                SetRect(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(42f, 0f), new Vector2(52f, 52f));

                var title = CreateText("Title", root.transform, "Upgrade Name", 18, TextAnchor.MiddleLeft);
                SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(176f, -21f), new Vector2(-330f, 26f));

                var effect = CreateText("Effect", root.transform, "+0 Damage", 15, TextAnchor.MiddleLeft);
                SetRect(effect.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(176f, -5f), new Vector2(-330f, 28f));

                var cost = CreateText("Cost", root.transform, "0G 0S 0C", 15, TextAnchor.MiddleCenter);
                SetRect(cost.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-220f, 2f), new Vector2(150f, 30f));

                var level = CreateText("Level", root.transform, "Lv 0", 17, TextAnchor.MiddleCenter);
                SetRect(level.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-62f, -18f), new Vector2(92f, 24f));

                var upgradeButton = CreateButton("UpgradeButton", root.transform, "UPGRADE", buttonSprite);
                SetRect(upgradeButton.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-62f, -10f), new Vector2(112f, 44f));

                var serialized = new SerializedObject(root.GetComponent<UIUpgradeItemView>());
                serialized.FindProperty("iconImage").objectReferenceValue = icon;
                serialized.FindProperty("nameText").objectReferenceValue = title;
                serialized.FindProperty("effectText").objectReferenceValue = effect;
                serialized.FindProperty("costText").objectReferenceValue = cost;
                serialized.FindProperty("levelText").objectReferenceValue = level;
                serialized.FindProperty("upgradeButton").objectReferenceValue = upgradeButton;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, ItemPrefabPath);
                Object.DestroyImmediate(root);
            }

            return AssetDatabase.LoadAssetAtPath<UIUpgradeItemView>(ItemPrefabPath);
        }

        private static void CreatePrefab(Sprite panel, UIUpgradeItemView itemPrefab)
        {
            var loadedExistingPrefab = File.Exists(PrefabPath);
            var root = loadedExistingPrefab
                ? PrefabUtility.LoadPrefabContents(PrefabPath)
                : new GameObject("UIUpgrade", typeof(RectTransform), typeof(Image), typeof(Canvas), typeof(GraphicRaycaster), typeof(UIUpgradeWindowController));
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(760f, 520f);
            var rootImage = root.GetComponent<Image>();
            rootImage.sprite = panel;
            rootImage.type = Image.Type.Sliced;

            var title = EnsureText(root.transform, "Title", "THE UPGRADE VAULT", 32, TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -36f), new Vector2(-120f, 52f));

            var closeButton = root.transform.Find("CloseButton")?.GetComponent<Button>();
            if (closeButton == null)
            {
                closeButton = CreateButton("CloseButton", root.transform, "X", AssetDatabase.LoadAssetAtPath<Sprite>($"{TextureFolder}/UpgradeButton.png"));
            }
            SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-34f, -34f), new Vector2(48f, 42f));

            var coinText = EnsureText(root.transform, "CoinText", "Your Coins: 0G 0S 0C", 17, TextAnchor.MiddleLeft);
            SetRect(coinText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(130f, -36f), new Vector2(210f, 34f));

            var listTransform = root.transform.Find("UpgradeListRoot");
            var listRoot = listTransform != null ? listTransform.gameObject : new GameObject("UpgradeListRoot", typeof(RectTransform), typeof(VerticalLayoutGroup));
            listRoot.transform.SetParent(root.transform, false);
            var listRect = listRoot.GetComponent<RectTransform>();
            SetRect(listRect, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, -46f), new Vector2(-56f, -120f));
            var layout = listRoot.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 10, 14);
            layout.spacing = 8f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            var serialized = new SerializedObject(root.GetComponent<UIUpgradeWindowController>());
            serialized.FindProperty("coinText").objectReferenceValue = coinText;
            serialized.FindProperty("upgradeListRoot").objectReferenceValue = listRect;
            serialized.FindProperty("closeButton").objectReferenceValue = closeButton;
            serialized.FindProperty("upgradeItemPrefab").objectReferenceValue = itemPrefab;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            if (loadedExistingPrefab)
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            else
            {
                Object.DestroyImmediate(root);
            }
        }

        private static Text EnsureText(Transform parent, string name, string content, int fontSize, TextAnchor alignment)
        {
            var existing = parent.Find(name)?.GetComponent<Text>();
            if (existing != null)
            {
                existing.text = content;
                existing.fontSize = fontSize;
                existing.alignment = alignment;
                return existing;
            }

            return CreateText(name, parent, content, fontSize, alignment);
        }

        private static Button CreateButton(string name, Transform parent, string label, Sprite sprite)
        {
            var image = CreateImage(name, parent, sprite);
            var button = image.gameObject.AddComponent<Button>();
            var text = CreateText("Label", image.transform, label, 16, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return button;
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

        private static Sprite CreateIconSprite(string name, Color32 fill, Color32 accent)
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

                var handle = Mathf.Abs(dx + dy) < 5 && dx < 14 && dy > -20;
                var head = dx > 4 && dx < 24 && dy < -2 && dy > -22;
                return handle || head ? accent : fill;
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
    }
}
#endif
