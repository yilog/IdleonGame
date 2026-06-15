#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using IdleonGame.Items;
using UnityEditor;
using UnityEngine;

namespace IdleonGame.Editor
{
    public static class CreateCurrencyDropSetup
    {
        private const string TextureFolder = "Assets/_Project/Resources/Items/Currency";
        private const string ItemFolder = "Assets/_Project/ScriptableObjects/Items";
        private const string CurrencyItemPath = ItemFolder + "/CurrencyCoin.asset";
        private const string ItemDatabasePath = "Assets/_Project/Resources/ItemDatabase.asset";

        [MenuItem("IdleonGame/Items/Create Currency Drop Setup")]
        public static void CreateAssets()
        {
            Directory.CreateDirectory(TextureFolder);
            Directory.CreateDirectory(ItemFolder);

            var copper = CreateCoinSprite("CoinCopper", new Color32(166, 91, 41, 255), new Color32(255, 178, 96, 255));
            CreateCoinSprite("CoinSilver", new Color32(122, 139, 151, 255), new Color32(232, 242, 247, 255));
            CreateCoinSprite("CoinGold", new Color32(218, 151, 32, 255), new Color32(255, 229, 109, 255));

            var currencyItem = AssetDatabase.LoadAssetAtPath<ItemDefinition>(CurrencyItemPath);
            if (currencyItem == null)
            {
                currencyItem = ScriptableObject.CreateInstance<ItemDefinition>();
                AssetDatabase.CreateAsset(currencyItem, CurrencyItemPath);
            }

            currencyItem.EditorSetData(WorldItemDropper.CurrencyItemId, "Coins", copper, ItemType.Currency, 999999);
            EditorUtility.SetDirty(currencyItem);
            EnsureItemDatabaseContains(currencyItem);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureItemDatabaseContains(ItemDefinition currencyItem)
        {
            var database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(ItemDatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<ItemDatabase>();
                AssetDatabase.CreateAsset(database, ItemDatabasePath);
            }

            var items = new List<ItemDefinition>();
            var serialized = new SerializedObject(database);
            var array = serialized.FindProperty("items");
            for (var i = 0; i < array.arraySize; i++)
            {
                var item = array.GetArrayElementAtIndex(i).objectReferenceValue as ItemDefinition;
                if (item != null && item.ItemId != currencyItem.ItemId)
                {
                    items.Add(item);
                }
            }

            items.Add(currencyItem);
            database.EditorSetItems(items.ToArray());
            EditorUtility.SetDirty(database);
        }

        private static Sprite CreateCoinSprite(string name, Color32 fill, Color32 highlight)
        {
            const int size = 32;
            var path = $"{TextureFolder}/{name}.png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                for (var y = 0; y < size; y++)
                {
                    for (var x = 0; x < size; x++)
                    {
                        var dx = x - size / 2;
                        var dy = y - size / 2;
                        var radius = Mathf.Sqrt(dx * dx + dy * dy);
                        if (radius > 14f)
                        {
                            texture.SetPixel(x, y, new Color32(0, 0, 0, 0));
                            continue;
                        }

                        if (radius > 11.5f)
                        {
                            texture.SetPixel(x, y, highlight);
                            continue;
                        }

                        var stripe = Mathf.Abs(dx + dy) < 2f;
                        texture.SetPixel(x, y, stripe ? highlight : fill);
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
                importer.spritePixelsPerUnit = 32;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
#endif
