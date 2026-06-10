#if UNITY_EDITOR
using System.IO;
using IdleonGame.Character;
using IdleonGame.Combat;
using IdleonGame.Core;
using IdleonGame.Items;
using IdleonGame.Monster;
using IdleonGame.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace IdleonGame.Editor
{
    public static class SetupTestCombatScene
    {
        private const string ScenePath = "Assets/_Project/Scenes/Maps/Test_Battle_Tilemap.unity";
        private const string BasicAttackPath = "Assets/_Project/ScriptableObjects/Skills/PlayerBasicAttack.asset";
        private const string TestItemTexturePath = "Assets/_Project/Art/Items/TestDropItem.png";
        private const string TestItemPath = "Assets/_Project/ScriptableObjects/Items/TestDropItem.asset";
        private const string ItemDatabasePath = "Assets/_Project/Resources/ItemDatabase.asset";
        private const string MonsterDatabasePath = "Assets/_Project/Resources/MonsterDatabase.asset";
        private const string TestMonsterTexturePath = "Assets/_Project/Art/Monsters/TestWalkerMonster.png";
        private const string TestMonsterPath = "Assets/_Project/ScriptableObjects/Monsters/TestWalkerMonster.asset";

        [MenuItem("IdleonGame/Setup/Setup Test Combat Scene")]
        public static void SetupCombat()
        {
            Directory.CreateDirectory("Assets/_Project/ScriptableObjects/Skills");
            Directory.CreateDirectory("Assets/_Project/ScriptableObjects/Items");
            Directory.CreateDirectory("Assets/_Project/Art/Items");
            Directory.CreateDirectory("Assets/_Project/Art/Monsters");
            Directory.CreateDirectory("Assets/_Project/Resources");
            EnsureLayer(8, GameLayerNames.Player);
            EnsureLayer(9, GameLayerNames.Monster);
            IgnorePlayerMonsterCollision();

            var attack = CreateBasicAttack();
            var testItem = CreateTestItem();
            CreateItemDatabase(testItem);
            var testMonster = SyncTestMonsterDefinition(testItem.ItemId);
            CreateMonsterDatabase(testMonster);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            ConfigurePlayer(attack);
            ConfigureMonster();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void IgnorePlayerMonsterCollision()
        {
            var playerLayer = LayerMask.NameToLayer(GameLayerNames.Player);
            var monsterLayer = LayerMask.NameToLayer(GameLayerNames.Monster);
            if (playerLayer >= 0 && monsterLayer >= 0)
            {
                Physics2D.IgnoreLayerCollision(playerLayer, monsterLayer, true);
            }
        }

        private static AttackDefinition CreateBasicAttack()
        {
            var attack = AssetDatabase.LoadAssetAtPath<AttackDefinition>(BasicAttackPath);
            if (attack == null)
            {
                attack = ScriptableObject.CreateInstance<AttackDefinition>();
                AssetDatabase.CreateAsset(attack, BasicAttackPath);
            }

            attack.EditorSetData(
                "player_basic_melee",
                "Basic Melee Attack",
                AttackSkillType.BasicMelee,
                8,
                0,
                0.45f,
                0.75f,
                new Vector2(1.1f, 0.85f));

            EditorUtility.SetDirty(attack);
            return attack;
        }

        private static ItemDefinition CreateTestItem()
        {
            var icon = CreateTestItemSprite();
            var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(TestItemPath);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ItemDefinition>();
                AssetDatabase.CreateAsset(item, TestItemPath);
            }

            item.EditorSetData("test_drop_item", "Test Drop Item", icon, ItemType.Material, 99);
            EditorUtility.SetDirty(item);
            return item;
        }

        private static Sprite CreateTestItemSprite()
        {
            if (!File.Exists(TestItemTexturePath))
            {
                var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                for (var y = 0; y < texture.height; y++)
                {
                    for (var x = 0; x < texture.width; x++)
                    {
                        var edge = x == 0 || y == 0 || x == texture.width - 1 || y == texture.height - 1;
                        var shine = x >= 4 && x <= 6 && y >= 9 && y <= 12;
                        var color = edge
                            ? new Color32(80, 54, 20, 255)
                            : shine ? new Color32(255, 232, 128, 255) : new Color32(210, 143, 46, 255);
                        texture.SetPixel(x, y, color);
                    }
                }

                texture.Apply();
                File.WriteAllBytes(TestItemTexturePath, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(TestItemTexturePath);
            }

            var importer = AssetImporter.GetAtPath(TestItemTexturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(TestItemTexturePath);
        }

        private static void CreateItemDatabase(ItemDefinition testItem)
        {
            var database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(ItemDatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<ItemDatabase>();
                AssetDatabase.CreateAsset(database, ItemDatabasePath);
            }

            database.EditorSetItems(new[] { testItem });
            EditorUtility.SetDirty(database);
        }

        private static MonsterDefinition SyncTestMonsterDefinition(string testItemId)
        {
            var monsterSprite = CreateTestMonsterSprite();
            var definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(TestMonsterPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<MonsterDefinition>();
                AssetDatabase.CreateAsset(definition, TestMonsterPath);
            }

            var drop = new MonsterDropEntry();
            drop.EditorSetData(testItemId, 1, 2, 1f);
            definition.EditorSetData(
                "test_walker",
                "Test Walker",
                monsterSprite,
                Vector2Int.one,
                20,
                0,
                0,
                0,
                MonsterAttackType.None,
                1.25f,
                false,
                2f,
                new[] { drop });

            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static Sprite CreateTestMonsterSprite()
        {
            if (!File.Exists(TestMonsterTexturePath))
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
                File.WriteAllBytes(TestMonsterTexturePath, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(TestMonsterTexturePath);
            }

            var importer = AssetImporter.GetAtPath(TestMonsterTexturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(TestMonsterTexturePath);
        }

        private static void CreateMonsterDatabase(MonsterDefinition testMonster)
        {
            var database = AssetDatabase.LoadAssetAtPath<MonsterDatabase>(MonsterDatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<MonsterDatabase>();
                AssetDatabase.CreateAsset(database, MonsterDatabasePath);
            }

            database.EditorSetMonsters(new[] { testMonster });
            EditorUtility.SetDirty(database);
        }

        private static void ConfigurePlayer(AttackDefinition attack)
        {
            var player = GameObject.Find("Player_TestBlock");
            if (player == null)
            {
                return;
            }

            var playerLayer = LayerMask.NameToLayer(GameLayerNames.Player);
            if (playerLayer >= 0)
            {
                player.layer = playerLayer;
            }

            var stats = player.GetComponent<CharacterStats>();
            if (stats == null)
            {
                stats = player.AddComponent<CharacterStats>();
            }

            stats.Configure(100, 50, 6, 1);

            var inventory = player.GetComponent<PlayerInventory>();
            if (inventory == null)
            {
                player.AddComponent<PlayerInventory>();
            }

            var attackComponent = player.GetComponent<PlayerAttack>();
            if (attackComponent == null)
            {
                attackComponent = player.AddComponent<PlayerAttack>();
            }

            attackComponent.Configure(attack, LayerMask.GetMask(GameLayerNames.Monster));
        }

        private static void ConfigureMonster()
        {
            var monster = GameObject.Find("Monster_TestWalker");
            if (monster == null)
            {
                return;
            }

            var monsterLayer = LayerMask.NameToLayer(GameLayerNames.Monster);
            if (monsterLayer >= 0)
            {
                monster.layer = monsterLayer;
            }

            var groundObject = GameObject.Find("Tilemap_Ground");
            var groundTilemap = groundObject != null ? groundObject.GetComponent<Tilemap>() : null;
            var definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(TestMonsterPath);

            var stats = monster.GetComponent<CharacterStats>();
            if (stats == null)
            {
                stats = monster.AddComponent<CharacterStats>();
            }

            stats.Configure(20, 0, 0, 0);

            var controller = monster.GetComponent<MonsterController>();
            if (controller != null && definition != null)
            {
                controller.Configure(definition, groundTilemap);
            }
        }

        private static void EnsureLayer(int index, string layerName)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            var layer = layers.GetArrayElementAtIndex(index);
            if (string.IsNullOrEmpty(layer.stringValue) || layer.stringValue == layerName)
            {
                layer.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning($"Layer {index} is already set to {layer.stringValue}; expected {layerName}.");
            }
        }
    }
}
#endif
