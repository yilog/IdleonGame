#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IdleonGame.Items;
using IdleonGame.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.Editor
{
    public static class CreateInventoryEquipmentUISetup
    {
        private const string TextureFolder = "Assets/_Project/Art/UI/Inventory";
        private const string PrefabFolder = "Assets/_Project/Resources/Prefabs/UI";
        private const string ItemFolder = "Assets/_Project/ScriptableObjects/Items";
        private const string ItemDatabasePath = "Assets/_Project/Resources/ItemDatabase.asset";
        private const string InventoryPrefabPath = PrefabFolder + "/UIInventory.prefab";
        private const string EquipmentPrefabPath = PrefabFolder + "/UIEquipment.prefab";
        private const string TestArmorPath = ItemFolder + "/TestTopArmor.asset";

        [MenuItem("IdleonGame/UI/Create Inventory Equipment Setup")]
        public static void CreateAssets()
        {
            Directory.CreateDirectory(TextureFolder);
            Directory.CreateDirectory(PrefabFolder);
            Directory.CreateDirectory(ItemFolder);

            var panel = CreatePanelSprite("InventoryPanel", 64, 64, new Color32(68, 55, 47, 255), new Color32(197, 156, 93, 255));
            var slot = CreatePanelSprite("InventorySlot", 48, 48, new Color32(37, 42, 47, 255), new Color32(133, 148, 160, 255));
            var hatSlot = CreateSlotSprite("SlotHat", new Color32(95, 79, 146, 255));
            var topSlot = CreateSlotSprite("SlotTop", new Color32(77, 122, 171, 255));
            var pantsSlot = CreateSlotSprite("SlotPants", new Color32(75, 105, 83, 255));
            var weaponSlot = CreateSlotSprite("SlotWeapon", new Color32(153, 86, 55, 255));
            var ringSlot = CreateSlotSprite("SlotRing", new Color32(178, 151, 67, 255));
            var shoesSlot = CreateSlotSprite("SlotShoes", new Color32(104, 78, 55, 255));
            var armorIcon = CreateEquipmentIcon("Equip_TestTopArmor", new Color32(78, 127, 184, 255), new Color32(211, 230, 248, 255));

            var armor = CreateTestArmor(armorIcon);
            EnsureItemDatabaseContains(armor);
            CreateInventoryPrefab(panel, slot);
            CreateEquipmentPrefab(panel, hatSlot, topSlot, pantsSlot, weaponSlot, ringSlot, shoesSlot);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static ItemDefinition CreateTestArmor(Sprite icon)
        {
            var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(TestArmorPath);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ItemDefinition>();
                AssetDatabase.CreateAsset(item, TestArmorPath);
            }

            item.EditorSetEquipmentData("test_top_armor", "Test Top Armor", icon, EquipmentSlotType.Top, 0, 5, 0, 0);
            EditorUtility.SetDirty(item);
            return item;
        }

        private static void EnsureItemDatabaseContains(ItemDefinition item)
        {
            var database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(ItemDatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<ItemDatabase>();
                AssetDatabase.CreateAsset(database, ItemDatabasePath);
            }

            var serialized = new SerializedObject(database);
            var itemsProperty = serialized.FindProperty("items");
            var items = new List<ItemDefinition>();
            for (var i = 0; i < itemsProperty.arraySize; i++)
            {
                var current = itemsProperty.GetArrayElementAtIndex(i).objectReferenceValue as ItemDefinition;
                if (current != null && current.ItemId != item.ItemId)
                {
                    items.Add(current);
                }
            }

            items.Add(item);
            database.EditorSetItems(items.ToArray());
            EditorUtility.SetDirty(database);
        }

        private static void CreateInventoryPrefab(Sprite panelSprite, Sprite slotSprite)
        {
            var root = CreateWindowRoot("UIInventory", 430, 360, new Vector2(250f, 20f), panelSprite);
            var controller = EnsureComponent<UIInventoryWindowController>(root);
            var closeButton = CreateButton(root.transform, "CloseButton", "X", new Vector2(190, 145), new Vector2(34, 28), panelSprite);
            var title = CreateText(root.transform, "Title", "INVENTORY", new Vector2(0, 145), new Vector2(220, 28), 20, TextAnchor.MiddleCenter);
            title.raycastTarget = false;

            var slotsRoot = new GameObject("Slots", typeof(RectTransform));
            slotsRoot.transform.SetParent(root.transform, false);
            var slotsRect = slotsRoot.GetComponent<RectTransform>();
            slotsRect.anchorMin = new Vector2(0.5f, 0.5f);
            slotsRect.anchorMax = new Vector2(0.5f, 0.5f);
            slotsRect.sizeDelta = new Vector2(336, 240);
            slotsRect.anchoredPosition = new Vector2(0, -15);

            for (var i = 0; i < 24; i++)
            {
                var slotObject = new GameObject($"Slot_{i:00}", typeof(RectTransform));
                slotObject.transform.SetParent(slotsRoot.transform, false);
                var rect = slotObject.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(48, 48);
                rect.anchoredPosition = new Vector2(-140 + (i % 6) * 56, 84 - (i / 6) * 56);
                var background = slotObject.AddComponent<Image>();
                background.sprite = slotSprite;
                background.type = Image.Type.Sliced;

                var icon = CreateImage(slotObject.transform, "Icon", null, Vector2.zero, new Vector2(38, 38));
                var count = CreateText(slotObject.transform, "Count", string.Empty, new Vector2(12, -15), new Vector2(30, 18), 14, TextAnchor.LowerRight);
                var view = slotObject.AddComponent<UIInventorySlotView>();
                var serialized = new SerializedObject(view);
                serialized.FindProperty("iconImage").objectReferenceValue = icon;
                serialized.FindProperty("countText").objectReferenceValue = count;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            var ghost = CreateImage(root.transform, "DragGhost", null, Vector2.zero, new Vector2(42, 42));
            ghost.enabled = false;
            ghost.raycastTarget = false;

            var rootSerialized = new SerializedObject(controller);
            rootSerialized.FindProperty("closeButton").objectReferenceValue = closeButton;
            rootSerialized.FindProperty("slotRoot").objectReferenceValue = slotsRoot.transform;
            rootSerialized.FindProperty("dragGhostImage").objectReferenceValue = ghost;
            rootSerialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, InventoryPrefabPath);
            Object.DestroyImmediate(root);
        }

        private static void CreateEquipmentPrefab(Sprite panelSprite, Sprite hat, Sprite top, Sprite pants, Sprite weapon, Sprite ring, Sprite shoes)
        {
            var root = CreateWindowRoot("UIEquipment", 300, 360, new Vector2(-260f, 20f), panelSprite);
            var controller = EnsureComponent<UIEquipmentWindowController>(root);
            var closeButton = CreateButton(root.transform, "CloseButton", "X", new Vector2(125, 145), new Vector2(34, 28), panelSprite);
            var title = CreateText(root.transform, "Title", "EQUIPMENT", new Vector2(0, 145), new Vector2(180, 28), 20, TextAnchor.MiddleCenter);
            title.raycastTarget = false;

            var slotsRoot = new GameObject("Slots", typeof(RectTransform));
            slotsRoot.transform.SetParent(root.transform, false);
            slotsRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(260, 260);
            CreateEquipmentSlot(slotsRoot.transform, "HatSlot", EquipmentSlotType.Hat, hat, new Vector2(-72, 78));
            CreateEquipmentSlot(slotsRoot.transform, "TopSlot", EquipmentSlotType.Top, top, new Vector2(-72, 12));
            CreateEquipmentSlot(slotsRoot.transform, "PantsSlot", EquipmentSlotType.Pants, pants, new Vector2(-72, -54));
            CreateEquipmentSlot(slotsRoot.transform, "WeaponSlot", EquipmentSlotType.Weapon, weapon, new Vector2(72, 78));
            CreateEquipmentSlot(slotsRoot.transform, "RingSlot", EquipmentSlotType.Ring, ring, new Vector2(72, 12));
            CreateEquipmentSlot(slotsRoot.transform, "ShoesSlot", EquipmentSlotType.Shoes, shoes, new Vector2(72, -54));

            var serialized = new SerializedObject(controller);
            serialized.FindProperty("closeButton").objectReferenceValue = closeButton;
            serialized.FindProperty("slotRoot").objectReferenceValue = slotsRoot.transform;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, EquipmentPrefabPath);
            Object.DestroyImmediate(root);
        }

        private static void CreateEquipmentSlot(Transform parent, string name, EquipmentSlotType type, Sprite backgroundSprite, Vector2 position)
        {
            var slotObject = new GameObject(name, typeof(RectTransform));
            slotObject.transform.SetParent(parent, false);
            var rect = slotObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(58, 58);
            rect.anchoredPosition = position;
            var background = slotObject.AddComponent<Image>();
            background.sprite = backgroundSprite;
            background.type = Image.Type.Sliced;

            var icon = CreateImage(slotObject.transform, "Icon", null, Vector2.zero, new Vector2(42, 42));
            var label = CreateText(slotObject.transform, "Label", type.ToString().ToUpperInvariant(), new Vector2(0, -38), new Vector2(82, 18), 11, TextAnchor.MiddleCenter);
            var view = slotObject.AddComponent<UIEquipmentSlotView>();
            var serialized = new SerializedObject(view);
            serialized.FindProperty("slotType").enumValueIndex = (int)type;
            serialized.FindProperty("iconImage").objectReferenceValue = icon;
            serialized.FindProperty("labelText").objectReferenceValue = label;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateWindowRoot(string name, float width, float height, Vector2 anchoredPosition, Sprite panelSprite)
        {
            var root = new GameObject(name, typeof(RectTransform));
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = anchoredPosition;
            var image = root.AddComponent<Image>();
            image.sprite = panelSprite;
            image.type = Image.Type.Sliced;
            return root;
        }

        private static Button CreateButton(Transform parent, string name, string text, Vector2 position, Vector2 size, Sprite sprite)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform));
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            var image = buttonObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            var button = buttonObject.AddComponent<Button>();
            CreateText(buttonObject.transform, "Text", text, Vector2.zero, size, 16, TextAnchor.MiddleCenter);
            return button;
        }

        private static Image CreateImage(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size)
        {
            var imageObject = new GameObject(name, typeof(RectTransform));
            imageObject.transform.SetParent(parent, false);
            var rect = imageObject.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            var image = imageObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        private static Text CreateText(Transform parent, string name, string value, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            var text = textObject.AddComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static Sprite CreatePanelSprite(string name, int width, int height, Color32 fill, Color32 border)
        {
            var path = $"{TextureFolder}/{name}.png";
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var isBorder = x < 4 || y < 4 || x >= width - 4 || y >= height - 4;
                    texture.SetPixel(x, y, isBorder ? border : fill);
                }
            }

            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            ConfigureTexture(path, true);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite CreateSlotSprite(string name, Color32 fill)
        {
            return CreatePanelSprite(name, 48, 48, fill, new Color32(225, 214, 178, 255));
        }

        private static Sprite CreateEquipmentIcon(string name, Color32 fill, Color32 trim)
        {
            var path = $"{TextureFolder}/{name}.png";
            var texture = new Texture2D(48, 48, TextureFormat.RGBA32, false);
            for (var y = 0; y < 48; y++)
            {
                for (var x = 0; x < 48; x++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            DrawRect(texture, 14, 12, 20, 24, fill);
            DrawRect(texture, 10, 26, 8, 10, fill);
            DrawRect(texture, 30, 26, 8, 10, fill);
            DrawRect(texture, 16, 30, 16, 4, trim);
            DrawRect(texture, 14, 12, 20, 3, trim);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            ConfigureTexture(path, false);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void DrawRect(Texture2D texture, int left, int bottom, int width, int height, Color color)
        {
            for (var y = bottom; y < bottom + height; y++)
            {
                for (var x = left; x < left + width; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }

        private static void ConfigureTexture(string path, bool sliced)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            if (sliced)
            {
                importer.spriteBorder = new Vector4(4, 4, 4, 4);
            }

            importer.SaveAndReimport();
        }

        private static T EnsureComponent<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }
    }
}
#endif
