#if UNITY_EDITOR
using System.IO;
using IdleonGame.Character;
using IdleonGame.Combat;
using IdleonGame.Core;
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

        [MenuItem("IdleonGame/Setup/Setup Test Combat Scene")]
        public static void SetupCombat()
        {
            Directory.CreateDirectory("Assets/_Project/ScriptableObjects/Skills");
            EnsureLayer(8, GameLayerNames.Player);
            EnsureLayer(9, GameLayerNames.Monster);
            IgnorePlayerMonsterCollision();

            var attack = CreateBasicAttack();
            SyncTestMonsterDefinition();
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


        private static void SyncTestMonsterDefinition()
        {
            var definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>("Assets/_Project/ScriptableObjects/Monsters/TestWalkerMonster.asset");
            if (definition == null)
            {
                return;
            }

            definition.EditorSetData(
                "test_walker",
                "Test Walker",
                Vector2Int.one,
                20,
                0,
                0,
                0,
                MonsterAttackType.None,
                1.25f,
                false,
                2f);

            EditorUtility.SetDirty(definition);
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
            var definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>("Assets/_Project/ScriptableObjects/Monsters/TestWalkerMonster.asset");

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