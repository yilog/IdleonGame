#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using IdleonGame.Items;
using UnityEditor;
using UnityEngine;

namespace IdleonGame.Editor
{
    public static class CreateMonsterPartDropItems
    {
        private const string TextureFolder = "Assets/_Project/Art/Items/MonsterParts";
        private const string ItemFolder = "Assets/_Project/ScriptableObjects/Items";
        private const string MonsterFolder = "Assets/_Project/ScriptableObjects/Monsters";
        private const string ItemDatabasePath = "Assets/_Project/Resources/ItemDatabase.asset";

        [MenuItem("IdleonGame/Items/Create Monster Part Drop Items")]
        public static void CreateAssets()
        {
            Directory.CreateDirectory(TextureFolder);
            Directory.CreateDirectory(ItemFolder);

            var ratPart = CreateMaterialItem(
                "RatPart.asset",
                "rat_part",
                "Rat Part",
                CreatePartIcon("Item_RatPart", new Color32(141, 103, 84, 255), new Color32(226, 189, 156, 255)));

            var slimePart = CreateMaterialItem(
                "SlimePart.asset",
                "slime_part",
                "Slime Part",
                CreatePartIcon("Item_SlimePart", new Color32(72, 188, 129, 255), new Color32(168, 247, 198, 255)));

            var mushroomPart = CreateMaterialItem(
                "MushroomPart.asset",
                "mushroom_part",
                "Mushroom Part",
                CreatePartIcon("Item_MushroomPart", new Color32(174, 78, 73, 255), new Color32(247, 180, 150, 255)));

            EnsureItemDatabaseContains(ratPart, slimePart, mushroomPart);
            SetMonsterDrops("Rat.asset", "rat_part", 1, 1, 0.5f);
            SetMonsterDrops("Slime.asset", "slime_part", 1, 1, 0.35f);
            SetMonsterDrops("Mushroom.asset", "mushroom_part", 1, 1, 0.3f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static ItemDefinition CreateMaterialItem(string assetName, string itemId, string displayName, Sprite icon)
        {
            var path = $"{ItemFolder}/{assetName}";
            var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ItemDefinition>();
                AssetDatabase.CreateAsset(item, path);
            }

            item.EditorSetData(itemId, displayName, icon, ItemType.Material, 999);
            EditorUtility.SetDirty(item);
            return item;
        }

        private static void SetMonsterDrops(string monsterAssetName, string itemId, int minCount, int maxCount, float chance)
        {
            var path = $"{MonsterFolder}/{monsterAssetName}";
            var monster = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (monster == null)
            {
                Debug.LogWarning($"Monster asset not found: {path}");
                return;
            }

            var serialized = new SerializedObject(monster);
            var drops = serialized.FindProperty("drops");
            drops.arraySize = 1;

            var entry = drops.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("itemId").stringValue = itemId;
            entry.FindPropertyRelative("minCount").intValue = Mathf.Max(1, minCount);
            entry.FindPropertyRelative("maxCount").intValue = Mathf.Max(minCount, maxCount);
            entry.FindPropertyRelative("dropChance").floatValue = Mathf.Clamp01(chance);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(monster);
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
                var item = itemsProperty.GetArrayElementAtIndex(i).objectReferenceValue as ItemDefinition;
                if (item != null && !string.IsNullOrEmpty(item.ItemId))
                {
                    itemsById[item.ItemId] = item;
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

        private static Sprite CreatePartIcon(string name, Color32 fill, Color32 highlight)
        {
            var texture = CreateClearTexture(48, 48);
            DrawEllipse(texture, 24, 23, 15, 11, fill);
            DrawEllipse(texture, 18, 29, 6, 4, highlight);
            DrawRect(texture, 16, 12, 16, 5, new Color32(82, 60, 55, 255));
            DrawRect(texture, 19, 17, 10, 5, fill);
            DrawRect(texture, 27, 28, 5, 4, highlight);
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
            if (!File.Exists(path))
            {
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(path);
            }

            Object.DestroyImmediate(texture);
            ConfigureTexture(path);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void DrawEllipse(Texture2D texture, int centerX, int centerY, int radiusX, int radiusY, Color color)
        {
            for (var y = centerY - radiusY; y <= centerY + radiusY; y++)
            {
                for (var x = centerX - radiusX; x <= centerX + radiusX; x++)
                {
                    var dx = (x - centerX) / (float)radiusX;
                    var dy = (y - centerY) / (float)radiusY;
                    if (dx * dx + dy * dy <= 1f)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }
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
