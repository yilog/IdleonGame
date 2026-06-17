#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace IdleonGame.Editor
{
    public static class CreateAdditionalGroundTiles
    {
        private const string TilesFolder = "Assets/_Project/Tilemaps/Tiles";

        private static readonly GroundTileSpec[] Tiles =
        {
            new GroundTileSpec("StoneGroundTile", "StoneGroundTile.png", new Color32(92, 97, 105, 255), new Color32(141, 149, 160, 255), Pattern.Blocks),
            new GroundTileSpec("DirtGroundTile", "DirtGroundTile.png", new Color32(113, 77, 49, 255), new Color32(158, 111, 68, 255), Pattern.Dots),
            new GroundTileSpec("GrassGroundTile", "GrassGroundTile.png", new Color32(76, 133, 72, 255), new Color32(144, 198, 91, 255), Pattern.Grass),
            new GroundTileSpec("BrickGroundTile", "BrickGroundTile.png", new Color32(128, 63, 54, 255), new Color32(190, 94, 75, 255), Pattern.Bricks),
            new GroundTileSpec("MetalGroundTile", "MetalGroundTile.png", new Color32(73, 88, 100, 255), new Color32(172, 190, 201, 255), Pattern.Rivets)
        };

        [MenuItem("IdleonGame/Tilemap/Create Additional Ground Tiles")]
        public static void CreateAssets()
        {
            Directory.CreateDirectory(TilesFolder);
            foreach (var spec in Tiles)
            {
                var sprite = CreateSprite(spec);
                CreateTile(spec.TileName, sprite);
            }

            SyncProjectTilePalette.SyncPalette();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Sprite CreateSprite(GroundTileSpec spec)
        {
            var path = $"{TilesFolder}/{spec.TextureName}";
            var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            for (var y = 0; y < 16; y++)
            {
                for (var x = 0; x < 16; x++)
                {
                    texture.SetPixel(x, y, GetPixelColor(spec, x, y));
                }
            }

            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            ConfigureTexture(path);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Color32 GetPixelColor(GroundTileSpec spec, int x, int y)
        {
            var border = x == 0 || y == 0 || x == 15 || y == 15;
            if (border)
            {
                return Darken(spec.BaseColor);
            }

            switch (spec.Pattern)
            {
                case Pattern.Blocks:
                    return x == 8 || y == 8 ? spec.AccentColor : ((x + y) % 5 == 0 ? Lerp(spec.BaseColor, spec.AccentColor, 0.35f) : spec.BaseColor);
                case Pattern.Dots:
                    return (x + y * 3) % 7 == 0 ? spec.AccentColor : spec.BaseColor;
                case Pattern.Grass:
                    return y >= 12 || (y >= 9 && x % 3 == 0) ? spec.AccentColor : spec.BaseColor;
                case Pattern.Bricks:
                    var horizontalMortar = y == 5 || y == 10;
                    var verticalMortar = (y < 5 && x == 8) || (y >= 5 && y < 10 && (x == 4 || x == 12)) || (y >= 10 && x == 8);
                    return horizontalMortar || verticalMortar ? Darken(spec.BaseColor) : ((x + y) % 6 == 0 ? spec.AccentColor : spec.BaseColor);
                case Pattern.Rivets:
                    var rivet = (x == 4 || x == 11) && (y == 4 || y == 11);
                    return rivet || x == 8 || y == 8 ? spec.AccentColor : spec.BaseColor;
                default:
                    return spec.BaseColor;
            }
        }

        private static void CreateTile(string tileName, Sprite sprite)
        {
            var path = $"{TilesFolder}/{tileName}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.name = tileName;
                AssetDatabase.CreateAsset(tile, path);
            }

            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.Sprite;
            EditorUtility.SetDirty(tile);
        }

        private static void ConfigureTexture(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static Color32 Darken(Color32 color)
        {
            return new Color32((byte)(color.r * 0.65f), (byte)(color.g * 0.65f), (byte)(color.b * 0.65f), color.a);
        }

        private static Color32 Lerp(Color32 a, Color32 b, float t)
        {
            return new Color32(
                (byte)Mathf.RoundToInt(Mathf.Lerp(a.r, b.r, t)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(a.g, b.g, t)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(a.b, b.b, t)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(a.a, b.a, t)));
        }

        private enum Pattern
        {
            Blocks,
            Dots,
            Grass,
            Bricks,
            Rivets
        }

        private readonly struct GroundTileSpec
        {
            public GroundTileSpec(string tileName, string textureName, Color32 baseColor, Color32 accentColor, Pattern pattern)
            {
                TileName = tileName;
                TextureName = textureName;
                BaseColor = baseColor;
                AccentColor = accentColor;
                Pattern = pattern;
            }

            public string TileName { get; }
            public string TextureName { get; }
            public Color32 BaseColor { get; }
            public Color32 AccentColor { get; }
            public Pattern Pattern { get; }
        }
    }
}
#endif
