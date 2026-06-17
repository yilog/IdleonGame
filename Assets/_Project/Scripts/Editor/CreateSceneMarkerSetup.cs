using System.IO;
using IdleonGame.Core;
using IdleonGame.Markers;
using UnityEditor;
using UnityEngine;

namespace IdleonGame.Editor
{
    public static class CreateSceneMarkerSetup
    {
        private const string ArtFolder = "Assets/_Project/Art/Markers";
        private const string PrefabFolder = "Assets/_Project/Resources/Prefabs/SceneMarkers";
        private const string AttackTexturePath = ArtFolder + "/AttackTargetMarker.png";
        private const string MoveTexturePath = ArtFolder + "/MoveTargetMarker.png";
        private const string AttackPrefabPath = PrefabFolder + "/AttackTargetMarker.prefab";
        private const string MovePrefabPath = PrefabFolder + "/MoveTargetMarker.prefab";

        [MenuItem("IdleonGame/Setup/Create Scene Marker Setup")]
        public static void CreateAssets()
        {
            Directory.CreateDirectory(ArtFolder);
            Directory.CreateDirectory(PrefabFolder);

            CreateAttackTargetTexture();
            CreateMoveTargetTexture();
            AssetDatabase.Refresh();

            ConfigureTextureImporter(AttackTexturePath);
            ConfigureTextureImporter(MoveTexturePath);

            CreateMarkerPrefab(
                AttackPrefabPath,
                "AttackTargetMarker",
                AssetDatabase.LoadAssetAtPath<Sprite>(AttackTexturePath),
                new Vector3(0f, 1.25f, 0f));

            CreateMarkerPrefab(
                MovePrefabPath,
                "MoveTargetMarker",
                AssetDatabase.LoadAssetAtPath<Sprite>(MoveTexturePath),
                new Vector3(0f, 0.18f, 0f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateAttackTargetTexture()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Clear(texture);

            var red = new Color32(230, 54, 58, 255);
            var yellow = new Color32(255, 226, 80, 255);
            DrawDiamond(texture, 32, 26, 22, red);
            DrawDiamond(texture, 32, 26, 14, yellow);
            DrawRect(texture, 29, 38, 6, 16, red);
            DrawTriangle(texture, 22, 48, 42, 48, 32, 60, red);

            texture.Apply();
            File.WriteAllBytes(AttackTexturePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        private static void CreateMoveTargetTexture()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Clear(texture);

            var blue = new Color32(65, 156, 255, 210);
            var white = new Color32(220, 244, 255, 255);
            DrawRing(texture, 32, 32, 22, 16, blue);
            DrawRing(texture, 32, 32, 13, 10, white);
            DrawRect(texture, 30, 8, 4, 48, white);
            DrawRect(texture, 8, 30, 48, 4, white);

            texture.Apply();
            File.WriteAllBytes(MoveTexturePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        private static void CreateMarkerPrefab(string path, string objectName, Sprite sprite, Vector3 worldOffset)
        {
            var root = File.Exists(path)
                ? PrefabUtility.LoadPrefabContents(path)
                : new GameObject(objectName, typeof(SceneMarkerView), typeof(SpriteRenderer));

            root.name = objectName;
            var marker = root.GetComponent<SceneMarkerView>();
            if (marker == null)
            {
                marker = root.AddComponent<SceneMarkerView>();
            }

            var renderer = root.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = root.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = sprite;
            renderer.sortingOrder = GameRenderLayers.SortingOrders.SceneMarker;

            var serialized = new SerializedObject(marker);
            serialized.FindProperty("worldOffset").vector3Value = worldOffset;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void ConfigureTextureImporter(string path)
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

        private static void Clear(Texture2D texture)
        {
            for (var y = 0; y < texture.height; y++)
            {
                for (var x = 0; x < texture.width; x++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        private static void DrawRect(Texture2D texture, int left, int bottom, int width, int height, Color color)
        {
            for (var y = bottom; y < bottom + height; y++)
            {
                for (var x = left; x < left + width; x++)
                {
                    SetPixel(texture, x, y, color);
                }
            }
        }

        private static void DrawDiamond(Texture2D texture, int centerX, int centerY, int radius, Color color)
        {
            for (var y = centerY - radius; y <= centerY + radius; y++)
            {
                for (var x = centerX - radius; x <= centerX + radius; x++)
                {
                    if (Mathf.Abs(x - centerX) + Mathf.Abs(y - centerY) <= radius)
                    {
                        SetPixel(texture, x, y, color);
                    }
                }
            }
        }

        private static void DrawTriangle(Texture2D texture, int x1, int y1, int x2, int y2, int x3, int y3, Color color)
        {
            var minX = Mathf.Min(x1, Mathf.Min(x2, x3));
            var maxX = Mathf.Max(x1, Mathf.Max(x2, x3));
            var minY = Mathf.Min(y1, Mathf.Min(y2, y3));
            var maxY = Mathf.Max(y1, Mathf.Max(y2, y3));

            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    if (IsInsideTriangle(x, y, x1, y1, x2, y2, x3, y3))
                    {
                        SetPixel(texture, x, y, color);
                    }
                }
            }
        }

        private static void DrawRing(Texture2D texture, int centerX, int centerY, int outerRadius, int innerRadius, Color color)
        {
            var outerSquared = outerRadius * outerRadius;
            var innerSquared = innerRadius * innerRadius;
            for (var y = centerY - outerRadius; y <= centerY + outerRadius; y++)
            {
                for (var x = centerX - outerRadius; x <= centerX + outerRadius; x++)
                {
                    var dx = x - centerX;
                    var dy = y - centerY;
                    var distanceSquared = dx * dx + dy * dy;
                    if (distanceSquared <= outerSquared && distanceSquared >= innerSquared)
                    {
                        SetPixel(texture, x, y, color);
                    }
                }
            }
        }

        private static bool IsInsideTriangle(int x, int y, int x1, int y1, int x2, int y2, int x3, int y3)
        {
            var d1 = Sign(x, y, x1, y1, x2, y2);
            var d2 = Sign(x, y, x2, y2, x3, y3);
            var d3 = Sign(x, y, x3, y3, x1, y1);
            var hasNegative = d1 < 0 || d2 < 0 || d3 < 0;
            var hasPositive = d1 > 0 || d2 > 0 || d3 > 0;
            return !(hasNegative && hasPositive);
        }

        private static int Sign(int x, int y, int x1, int y1, int x2, int y2)
        {
            return (x - x2) * (y1 - y2) - (x1 - x2) * (y - y2);
        }

        private static void SetPixel(Texture2D texture, int x, int y, Color color)
        {
            if (x < 0 || y < 0 || x >= texture.width || y >= texture.height)
            {
                return;
            }

            texture.SetPixel(x, y, color);
        }
    }
}
