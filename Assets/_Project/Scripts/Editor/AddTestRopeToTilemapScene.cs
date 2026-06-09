#if UNITY_EDITOR
using System.IO;
using IdleonGame.Map;
using IdleonGame.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace IdleonGame.Editor
{
    public static class AddTestRopeToTilemapScene
    {
        private const string ScenePath = "Assets/_Project/Scenes/Maps/Test_Battle_Tilemap.unity";
        private const string GroundTilePath = "Assets/_Project/Tilemaps/Tiles/TestGroundTile.asset";
        private const string RopeTexturePath = "Assets/_Project/Tilemaps/Tiles/TestRopeTile.png";
        private const string RopeTilePath = "Assets/_Project/Tilemaps/Tiles/TestRopeTile.asset";

        [MenuItem("IdleonGame/Setup/Add Test Rope To Tilemap Scene")]
        public static void AddRope()
        {
            Directory.CreateDirectory("Assets/_Project/Tilemaps/Tiles");

            var ropeTile = CreateRopeTile();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var decorationObject = GameObject.Find("Tilemap_Decoration");
            var groundObject = GameObject.Find("Tilemap_Ground");

            if (decorationObject == null || groundObject == null)
            {
                Debug.LogError("Tilemap_Decoration or Tilemap_Ground was not found in Test_Battle_Tilemap scene.");
                return;
            }

            var decorationTilemap = decorationObject.GetComponent<Tilemap>();
            var groundTilemap = groundObject.GetComponent<Tilemap>();
            var groundTile = AssetDatabase.LoadAssetAtPath<TileBase>(GroundTilePath);

            for (var y = -2; y <= 3; y++)
            {
                decorationTilemap.SetTile(new Vector3Int(0, y, 0), ropeTile);
            }

            if (groundTile != null)
            {
                for (var x = -3; x <= 3; x++)
                {
                    groundTilemap.SetTile(new Vector3Int(x, 1, 0), groundTile);
                }
            }

            decorationTilemap.CompressBounds();
            groundTilemap.CompressBounds();

            var ropeTilemap = decorationObject.GetComponent<RopeTilemap>();
            if (ropeTilemap == null)
            {
                ropeTilemap = decorationObject.AddComponent<RopeTilemap>();
            }

            ropeTilemap.Configure(ropeTile);

            var player = GameObject.Find("Player_TestBlock");
            if (player != null)
            {
                if (player.GetComponent<PlayerClimb>() == null)
                {
                    player.AddComponent<PlayerClimb>();
                }

                if (player.GetComponent<PlayerController>() == null)
                {
                    player.AddComponent<PlayerController>();
                }

                if (player.GetComponent<PlayerMovement>() == null)
                {
                    player.AddComponent<PlayerMovement>();
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Tile CreateRopeTile()
        {
            if (!File.Exists(RopeTexturePath))
            {
                var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                for (var y = 0; y < texture.height; y++)
                {
                    for (var x = 0; x < texture.width; x++)
                    {
                        var isRope = x == 7 || x == 8 || (y % 5 == 0 && x >= 5 && x <= 10);
                        texture.SetPixel(x, y, isRope ? new Color32(138, 88, 44, 255) : new Color32(0, 0, 0, 0));
                    }
                }

                texture.Apply();
                File.WriteAllBytes(RopeTexturePath, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(RopeTexturePath);
            }

            var importer = AssetImporter.GetAtPath(RopeTexturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            var tile = AssetDatabase.LoadAssetAtPath<Tile>(RopeTilePath);
            if (tile != null)
            {
                return tile;
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(RopeTexturePath);
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = "TestRopeTile";
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.None;
            AssetDatabase.CreateAsset(tile, RopeTilePath);
            return tile;
        }
    }
}
#endif