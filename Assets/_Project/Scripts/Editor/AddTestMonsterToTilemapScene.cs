#if UNITY_EDITOR
using System.IO;
using IdleonGame.Character;
using IdleonGame.Core;
using IdleonGame.Monster;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace IdleonGame.Editor
{
    public static class AddTestMonsterToTilemapScene
    {
        private const string ScenePath = "Assets/_Project/Scenes/Maps/Test_Battle_Tilemap.unity";
        private const string MonsterTexturePath = "Assets/_Project/Art/Monsters/TestWalkerMonster.png";
        private const string MonsterDefinitionPath = "Assets/_Project/ScriptableObjects/Monsters/TestWalkerMonster.asset";

        [MenuItem("IdleonGame/Setup/Add Test Monster To Tilemap Scene")]
        public static void AddMonster()
        {
            Directory.CreateDirectory("Assets/_Project/Art/Monsters");
            Directory.CreateDirectory("Assets/_Project/ScriptableObjects/Monsters");

            var monsterSprite = CreateMonsterSprite();
            var definition = CreateMonsterDefinition(monsterSprite);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var existing = GameObject.Find("Monster_TestWalker");
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            var groundObject = GameObject.Find("Tilemap_Ground");
            if (groundObject == null)
            {
                Debug.LogError("Tilemap_Ground was not found in Test_Battle_Tilemap scene.");
                return;
            }

            var groundTilemap = groundObject.GetComponent<Tilemap>();
            var monster = new GameObject("Monster_TestWalker");
            monster.transform.position = new Vector3(-4f, -1.5f, 0f);

            var spriteRenderer = monster.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = monsterSprite;
            spriteRenderer.sortingOrder = GameRenderLayers.SortingOrders.Monster;

            var body = monster.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 3f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            var collider = monster.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.9f, 0.9f);

            monster.AddComponent<CharacterStats>().Configure(definition.MaxHealth, definition.MaxMana, definition.AttackPower, definition.Defense);
            var controller = monster.AddComponent<MonsterController>();
            controller.Configure(definition, groundTilemap);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static MonsterDefinition CreateMonsterDefinition(Sprite monsterSprite)
        {
            var definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(MonsterDefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<MonsterDefinition>();
                AssetDatabase.CreateAsset(definition, MonsterDefinitionPath);
            }

            definition.EditorSetData(
                "test_walker",
                "Test Walker",
                null,
                monsterSprite,
                Vector2Int.one,
                20,
                0,
                0,
                0,
                MonsterAttackType.None,
                1.25f,
                0.35f,
                1f,
                2.5f,
                1.5f,
                3.5f,
                false,
                2f,
                System.Array.Empty<MonsterDropEntry>());

            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static Sprite CreateMonsterSprite()
        {
            if (!File.Exists(MonsterTexturePath))
            {
                var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                for (var y = 0; y < texture.height; y++)
                {
                    for (var x = 0; x < texture.width; x++)
                    {
                        var border = x == 0 || y == 0 || x == texture.width - 1 || y == texture.height - 1;
                        var eye = (x == 5 || x == 10) && y >= 9 && y <= 11;
                        var color = border
                            ? new Color32(50, 65, 55, 255)
                            : eye ? new Color32(245, 245, 210, 255) : new Color32(86, 174, 112, 255);
                        texture.SetPixel(x, y, color);
                    }
                }

                texture.Apply();
                File.WriteAllBytes(MonsterTexturePath, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(MonsterTexturePath);
            }

            var importer = AssetImporter.GetAtPath(MonsterTexturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(MonsterTexturePath);
        }
    }
}
#endif
