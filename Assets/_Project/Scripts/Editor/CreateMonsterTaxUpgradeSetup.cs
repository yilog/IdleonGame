#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using IdleonGame.Upgrades;
using UnityEditor;
using UnityEngine;

namespace IdleonGame.Editor
{
    public static class CreateMonsterTaxUpgradeSetup
    {
        private const string TextureFolder = "Assets/_Project/Art/UI/Upgrades";
        private const string UpgradeFolder = "Assets/_Project/ScriptableObjects/Upgrades";
        private const string DatabasePath = "Assets/_Project/Resources/UpgradeDatabase.asset";
        private const string UpgradeId = "monster_tax";
        private const string UpgradeAssetPath = UpgradeFolder + "/" + UpgradeId + ".asset";

        [MenuItem("IdleonGame/UI/Create Monster Tax Upgrade")]
        public static void CreateAssets()
        {
            Directory.CreateDirectory(TextureFolder);
            Directory.CreateDirectory(UpgradeFolder);

            var icon = CreateMonsterTaxIcon();
            var monsterTax = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(UpgradeAssetPath);
            if (monsterTax == null)
            {
                monsterTax = ScriptableObject.CreateInstance<UpgradeDefinition>();
                AssetDatabase.CreateAsset(monsterTax, UpgradeAssetPath);
            }

            monsterTax.EditorSetData(
                UpgradeId,
                "Monster Tax",
                icon,
                "Increase coins dropped by monsters.",
                UpgradeEffectType.MonsterCurrencyDropPercent,
                1000,
                5,
                0);
            EditorUtility.SetDirty(monsterTax);

            AddToDatabase(monsterTax);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void AddToDatabase(UpgradeDefinition monsterTax)
        {
            var database = AssetDatabase.LoadAssetAtPath<UpgradeDatabase>(DatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<UpgradeDatabase>();
                AssetDatabase.CreateAsset(database, DatabasePath);
            }

            var upgrades = new List<UpgradeDefinition>();
            foreach (var upgrade in database.Upgrades)
            {
                if (upgrade != null && upgrade.UpgradeId != UpgradeId)
                {
                    upgrades.Add(upgrade);
                }
            }

            upgrades.Add(monsterTax);
            database.EditorSetData(upgrades);
            EditorUtility.SetDirty(database);
        }

        private static Sprite CreateMonsterTaxIcon()
        {
            var path = $"{TextureFolder}/Upgrade_MonsterTax.png";
            if (!File.Exists(path))
            {
                const int size = 64;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point
                };

                for (var y = 0; y < size; y++)
                {
                    for (var x = 0; x < size; x++)
                    {
                        texture.SetPixel(x, y, new Color32(0, 0, 0, 0));
                    }
                }

                DrawCoin(texture, 32, 34, 22, new Color32(222, 172, 55, 255), new Color32(255, 228, 112, 255));
                DrawCoin(texture, 22, 22, 12, new Color32(181, 118, 47, 255), new Color32(242, 183, 84, 255));
                DrawCoin(texture, 44, 20, 10, new Color32(190, 198, 207, 255), new Color32(240, 244, 248, 255));
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

        private static void DrawCoin(Texture2D texture, int centerX, int centerY, int radius, Color32 edge, Color32 fill)
        {
            var outer = radius * radius;
            var innerRadius = Mathf.Max(1, radius - 4);
            var inner = innerRadius * innerRadius;
            for (var y = centerY - radius; y <= centerY + radius; y++)
            {
                for (var x = centerX - radius; x <= centerX + radius; x++)
                {
                    if (x < 0 || y < 0 || x >= texture.width || y >= texture.height)
                    {
                        continue;
                    }

                    var dx = x - centerX;
                    var dy = y - centerY;
                    var distance = dx * dx + dy * dy;
                    if (distance <= outer)
                    {
                        texture.SetPixel(x, y, distance <= inner ? fill : edge);
                    }
                }
            }
        }
    }
}
#endif
