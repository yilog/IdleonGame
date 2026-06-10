#if UNITY_EDITOR
using System.IO;
using IdleonGame.Character;
using IdleonGame.Core;
using IdleonGame.Items;
using IdleonGame.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IdleonGame.Editor
{
    public static class AddTestPlayerToTilemapScene
    {
        private const string ScenePath = "Assets/_Project/Scenes/Maps/Test_Battle_Tilemap.unity";
        private const string PlayerTexturePath = "Assets/_Project/Art/Characters/TestPlayerBlock.png";

        [MenuItem("IdleonGame/Setup/Add Test Player To Tilemap Scene")]
        public static void AddPlayer()
        {
            Directory.CreateDirectory("Assets/_Project/Art/Characters");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var existing = GameObject.Find("Player_TestBlock");
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            var player = new GameObject("Player_TestBlock");
            player.transform.position = new Vector3(-8f, -1.5f, 0f);

            var spriteRenderer = player.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreatePlayerSprite();
            spriteRenderer.sortingOrder = GameRenderLayers.SortingOrders.Player;

            var body = player.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 3f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            var collider = player.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.9f, 0.95f);

            player.AddComponent<CharacterStats>().Configure(100, 50, 6, 1);
            player.AddComponent<PlayerInventory>();
            player.AddComponent<PlayerClimb>();
            player.AddComponent<PlayerMovement>();
            player.AddComponent<PlayerAutoNavigator>();
            player.AddComponent<PlayerAttack>();
            player.AddComponent<PlayerController>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Sprite CreatePlayerSprite()
        {
            if (!File.Exists(PlayerTexturePath))
            {
                var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                for (var y = 0; y < texture.height; y++)
                {
                    for (var x = 0; x < texture.width; x++)
                    {
                        var border = x == 0 || y == 0 || x == texture.width - 1 || y == texture.height - 1;
                        texture.SetPixel(x, y, border ? new Color32(23, 45, 80, 255) : new Color32(66, 135, 245, 255));
                    }
                }

                texture.Apply();
                File.WriteAllBytes(PlayerTexturePath, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(PlayerTexturePath);
            }

            var importer = AssetImporter.GetAtPath(PlayerTexturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(PlayerTexturePath);
        }
    }
}
#endif