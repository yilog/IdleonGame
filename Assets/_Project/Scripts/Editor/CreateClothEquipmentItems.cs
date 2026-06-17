#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using IdleonGame.Items;
using UnityEditor;
using UnityEngine;

namespace IdleonGame.Editor
{
    public static class CreateClothEquipmentItems
    {
        private const string TextureFolder = "Assets/_Project/Art/UI/Inventory";
        private const string ItemFolder = "Assets/_Project/ScriptableObjects/Items";
        private const string ItemDatabasePath = "Assets/_Project/Resources/ItemDatabase.asset";

        [MenuItem("IdleonGame/Items/Create Cloth Equipment Items")]
        public static void CreateAssets()
        {
            Directory.CreateDirectory(TextureFolder);
            Directory.CreateDirectory(ItemFolder);

            var clothShirtIcon = CreateShirtIcon("Equip_CoarseClothShirt", new Color32(137, 105, 75, 255), new Color32(218, 189, 142, 255));
            var clothPantsIcon = CreatePantsIcon("Equip_CoarseClothPants", new Color32(104, 91, 78, 255), new Color32(199, 171, 126, 255));

            var clothShirt = CreateEquipmentItem(
                "CoarseClothShirt.asset",
                "coarse_cloth_shirt",
                "粗布衣",
                clothShirtIcon,
                EquipmentSlotType.Top,
                defense: 10);

            var clothPants = CreateEquipmentItem(
                "CoarseClothPants.asset",
                "coarse_cloth_pants",
                "粗布裤",
                clothPantsIcon,
                EquipmentSlotType.Pants,
                defense: 8);

            EnsureItemDatabaseContains(clothShirt, clothPants);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static ItemDefinition CreateEquipmentItem(string assetName, string itemId, string displayName, Sprite icon, EquipmentSlotType slot, int defense)
        {
            var path = $"{ItemFolder}/{assetName}";
            var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ItemDefinition>();
                AssetDatabase.CreateAsset(item, path);
            }

            item.EditorSetEquipmentData(itemId, displayName, icon, slot, 0, defense, 0, 0);
            EditorUtility.SetDirty(item);
            return item;
        }

        private static void EnsureItemDatabaseContains(params ItemDefinition[] newItems)
        {
            var database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(ItemDatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<ItemDatabase>();
                AssetDatabase.CreateAsset(database, ItemDatabasePath);
            }

            var serialized = new SerializedObject(database);
            var itemsProperty = serialized.FindProperty("items");
            var itemsById = new Dictionary<string, ItemDefinition>();
            for (var i = 0; i < itemsProperty.arraySize; i++)
            {
                var current = itemsProperty.GetArrayElementAtIndex(i).objectReferenceValue as ItemDefinition;
                if (current != null && !string.IsNullOrEmpty(current.ItemId))
                {
                    itemsById[current.ItemId] = current;
                }
            }

            foreach (var item in newItems)
            {
                if (item != null && !string.IsNullOrEmpty(item.ItemId))
                {
                    itemsById[item.ItemId] = item;
                }
            }

            database.EditorSetItems(new List<ItemDefinition>(itemsById.Values).ToArray());
            EditorUtility.SetDirty(database);
        }

        private static Sprite CreateShirtIcon(string name, Color32 fill, Color32 trim)
        {
            var texture = CreateClearTexture(48, 48);
            DrawRect(texture, 14, 12, 20, 25, fill);
            DrawRect(texture, 8, 24, 8, 12, fill);
            DrawRect(texture, 32, 24, 8, 12, fill);
            DrawRect(texture, 16, 32, 16, 4, trim);
            DrawRect(texture, 14, 12, 20, 3, trim);
            DrawRect(texture, 12, 20, 24, 3, new Color32(91, 70, 54, 255));
            return SaveSprite(name, texture);
        }

        private static Sprite CreatePantsIcon(string name, Color32 fill, Color32 trim)
        {
            var texture = CreateClearTexture(48, 48);
            DrawRect(texture, 15, 12, 8, 26, fill);
            DrawRect(texture, 25, 12, 8, 26, fill);
            DrawRect(texture, 14, 34, 20, 5, trim);
            DrawRect(texture, 15, 12, 8, 3, trim);
            DrawRect(texture, 25, 12, 8, 3, trim);
            DrawRect(texture, 23, 18, 2, 18, new Color32(76, 64, 56, 255));
            return SaveSprite(name, texture);
        }

        private static Texture2D CreateClearTexture(int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            return texture;
        }

        private static Sprite SaveSprite(string name, Texture2D texture)
        {
            var path = $"{TextureFolder}/{name}.png";
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            ConfigureTexture(path);
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

        private static void ConfigureTexture(string path)
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
            importer.SaveAndReimport();
        }
    }
}
#endif
