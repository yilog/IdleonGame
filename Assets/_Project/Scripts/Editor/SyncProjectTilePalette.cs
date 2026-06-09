#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace IdleonGame.Editor
{
    public static class SyncProjectTilePalette
    {
        private const string PalettePath = "Assets/_Project/Tilemaps/Palettes/New Palette.prefab";
        private const string TilesFolder = "Assets/_Project/Tilemaps/Tiles";

        [MenuItem("IdleonGame/Tilemap/Sync Project Tile Palette")]
        public static void SyncPalette()
        {
            Directory.CreateDirectory(TilesFolder);
            Directory.CreateDirectory("Assets/_Project/Tilemaps/Palettes");

            var paletteRoot = PrefabUtility.LoadPrefabContents(PalettePath);
            if (paletteRoot == null)
            {
                Debug.LogError($"Palette prefab was not found: {PalettePath}");
                return;
            }

            try
            {
                var tilemap = paletteRoot.GetComponentInChildren<Tilemap>();
                if (tilemap == null)
                {
                    var layer = new GameObject("Layer1");
                    layer.transform.SetParent(paletteRoot.transform);
                    tilemap = layer.AddComponent<Tilemap>();
                    layer.AddComponent<TilemapRenderer>();
                }

                tilemap.ClearAllTiles();

                var tileGuids = AssetDatabase.FindAssets("t:TileBase", new[] { TilesFolder });
                var tiles = tileGuids
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .OrderBy(path => path)
                    .Select(path => AssetDatabase.LoadAssetAtPath<TileBase>(path))
                    .Where(tile => tile != null)
                    .ToArray();

                for (var i = 0; i < tiles.Length; i++)
                {
                    tilemap.SetTile(new Vector3Int(i, 0, 0), tiles[i]);
                }

                tilemap.CompressBounds();
                PrefabUtility.SaveAsPrefabAsset(paletteRoot, PalettePath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(paletteRoot);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif