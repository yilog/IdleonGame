#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IdleonGame.Character;
using IdleonGame.Combat;
using IdleonGame.Core;
using IdleonGame.Data;
using IdleonGame.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace IdleonGame.Editor
{
    public static class CreateWarriorPlayerSetup
    {
        private static readonly string[] Actions =
        {
            PlayerAnimator.States.Idle,
            PlayerAnimator.States.Run,
            PlayerAnimator.States.Death,
            PlayerAnimator.States.Attack,
            PlayerAnimator.States.GetHit,
            PlayerAnimator.States.Jump,
            PlayerAnimator.States.Fall
        };

        private const string WarriorFramesRoot = "Assets/_Project/Art/Characters/Warrior/Frames";
        private const string WarriorAnimationFolder = "Assets/_Project/Resources/Animations/Warrior";
        private const string WarriorControllerPath = WarriorAnimationFolder + "/Warrior.controller";
        private const string CharacterPrefabFolder = "Assets/_Project/Resources/Prefabs/Characters";
        private const string WarriorPrefabPath = CharacterPrefabFolder + "/Warrior.prefab";
        private const string ArcherPrefabPath = CharacterPrefabFolder + "/Archer.prefab";
        private const string ArcherControllerPath = "Assets/_Project/Resources/Animations/Archer/Archer.controller";
        private const string ArcherIdleSpritePath = "Assets/_Project/Art/Characters/Archer/Frames/Idle.png";
        private const string ArcherBasicAttackPath = "Assets/_Project/ScriptableObjects/Skills/PlayerBasicAttack.asset";
        private const string ArcherArrowAttackPath = "Assets/_Project/ScriptableObjects/Skills/PlayerArrowAttack.asset";
        private const string WarriorSlashPath = "Assets/_Project/ScriptableObjects/Skills/PlayerWarriorSlash.asset";
        private const string PresentationFolder = "Assets/_Project/ScriptableObjects/PlayerClasses";
        private const string WarriorPresentationPath = PresentationFolder + "/WarriorPresentation.asset";
        private const string ArcherPresentationPath = PresentationFolder + "/ArcherPresentation.asset";
        private const string PresentationDatabasePath = "Assets/_Project/Resources/PlayerClassPresentationDatabase.asset";
        private const string SkillSetFolder = "Assets/_Project/ScriptableObjects/PlayerClassSkills";
        private const string ArcherSkillSetPath = SkillSetFolder + "/ArcherSkillSet.asset";
        private const string WarriorSkillSetPath = SkillSetFolder + "/WarriorSkillSet.asset";
        private const string SkillDatabasePath = "Assets/_Project/Resources/PlayerClassSkillDatabase.asset";

        [MenuItem("IdleonGame/Setup/Create Warrior Player Setup")]
        public static void CreateAssets()
        {
            Directory.CreateDirectory(WarriorFramesRoot);
            Directory.CreateDirectory(WarriorAnimationFolder);
            Directory.CreateDirectory(CharacterPrefabFolder);
            Directory.CreateDirectory(PresentationFolder);
            Directory.CreateDirectory(SkillSetFolder);
            Directory.CreateDirectory(Path.GetDirectoryName(WarriorSlashPath));

            var sprites = CreateWarriorSprites();
            var clips = CreateAnimationClips(sprites);
            var controller = CreateAnimatorController(clips);
            var warriorPrefab = CreateWarriorPrefab(sprites[PlayerAnimator.States.Idle], controller);
            var warriorPresentation = CreatePresentation(
                WarriorPresentationPath,
                PlayerClassType.Warrior,
                "Warrior",
                warriorPrefab,
                controller,
                sprites[PlayerAnimator.States.Idle]);

            var archerPresentation = CreatePresentation(
                ArcherPresentationPath,
                PlayerClassType.Archer,
                "Archer",
                AssetDatabase.LoadAssetAtPath<GameObject>(ArcherPrefabPath),
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ArcherControllerPath),
                AssetDatabase.LoadAssetAtPath<Sprite>(ArcherIdleSpritePath));

            UpdatePresentationDatabase(archerPresentation, warriorPresentation);
            CreateSkillConfiguration();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Dictionary<string, Sprite> CreateWarriorSprites()
        {
            var sprites = new Dictionary<string, Sprite>();
            foreach (var action in Actions)
            {
                var path = $"{WarriorFramesRoot}/{action}.png";
                CreateWarriorFrame(path, action);
                AssetDatabase.ImportAsset(path);
                ConfigureTextureImporter(path);
                sprites[action] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }

            CreateReadme();
            return sprites;
        }

        private static Dictionary<string, AnimationClip> CreateAnimationClips(IReadOnlyDictionary<string, Sprite> sprites)
        {
            var clips = new Dictionary<string, AnimationClip>();
            foreach (var action in Actions)
            {
                var path = $"{WarriorAnimationFolder}/Warrior_{action}.anim";
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null)
                {
                    clip = new AnimationClip();
                    AssetDatabase.CreateAsset(clip, path);
                }

                clip.name = $"Warrior_{action}";
                clip.frameRate = 12f;
                var binding = new EditorCurveBinding
                {
                    path = "view",
                    type = typeof(SpriteRenderer),
                    propertyName = "m_Sprite"
                };

                var sprite = sprites[action];
                var frames = action == PlayerAnimator.States.Idle || action == PlayerAnimator.States.Run
                    ? new[]
                    {
                        new ObjectReferenceKeyframe { time = 0f, value = sprite },
                        new ObjectReferenceKeyframe { time = 0.12f, value = sprite }
                    }
                    : new[]
                    {
                        new ObjectReferenceKeyframe { time = 0f, value = sprite }
                    };

                AnimationUtility.SetObjectReferenceCurve(clip, binding, frames);
                EditorUtility.SetDirty(clip);
                clips[action] = clip;
            }

            return clips;
        }

        private static AnimatorController CreateAnimatorController(IReadOnlyDictionary<string, AnimationClip> clips)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(WarriorControllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(WarriorControllerPath);
            }

            if (controller.layers == null || controller.layers.Length == 0)
            {
                controller.AddLayer("Base Layer");
            }

            EnsureParameter(controller, PlayerAnimator.Parameters.MoveSpeed, AnimatorControllerParameterType.Float);
            EnsureParameter(controller, PlayerAnimator.Parameters.VerticalSpeed, AnimatorControllerParameterType.Float);
            EnsureParameter(controller, PlayerAnimator.Parameters.IsGrounded, AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, PlayerAnimator.Parameters.IsDead, AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, PlayerAnimator.Parameters.Attack, AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, PlayerAnimator.Parameters.GetHit, AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, PlayerAnimator.Parameters.Death, AnimatorControllerParameterType.Trigger);

            var stateMachine = controller.layers[0].stateMachine;
            foreach (var action in Actions)
            {
                var state = FindState(stateMachine, action) ?? stateMachine.AddState(action);
                state.motion = clips[action];
                if (action == PlayerAnimator.States.Idle)
                {
                    stateMachine.defaultState = state;
                }
            }

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static GameObject CreateWarriorPrefab(Sprite idleSprite, RuntimeAnimatorController controller)
        {
            GameObject root = null;
            var loadedPrefabContents = File.Exists(WarriorPrefabPath);
            try
            {
                root = loadedPrefabContents
                    ? PrefabUtility.LoadPrefabContents(WarriorPrefabPath)
                    : new GameObject("Warrior", typeof(Animator));

                root.name = "Warrior";
                var animator = root.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = root.AddComponent<Animator>();
                }

                animator.runtimeAnimatorController = controller;

                var view = root.transform.Find("view");
                if (view == null)
                {
                    var viewObject = new GameObject("view");
                    viewObject.transform.SetParent(root.transform, false);
                    view = viewObject.transform;
                }

                view.localPosition = Vector3.zero;
                view.localScale = new Vector3(2f, 2f, 1f);

                var spriteRenderer = view.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    spriteRenderer = view.gameObject.AddComponent<SpriteRenderer>();
                }

                spriteRenderer.sprite = idleSprite;
                spriteRenderer.sortingOrder = GameRenderLayers.SortingOrders.Player;

                PrefabUtility.SaveAsPrefabAsset(root, WarriorPrefabPath);
            }
            finally
            {
                if (root != null)
                {
                    if (loadedPrefabContents)
                    {
                        PrefabUtility.UnloadPrefabContents(root);
                    }
                    else
                    {
                        Object.DestroyImmediate(root);
                    }
                }
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(WarriorPrefabPath);
        }

        private static PlayerClassPresentationDefinition CreatePresentation(
            string path,
            PlayerClassType classType,
            string displayName,
            GameObject prefab,
            RuntimeAnimatorController controller,
            Sprite previewSprite)
        {
            var definition = AssetDatabase.LoadAssetAtPath<PlayerClassPresentationDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<PlayerClassPresentationDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            definition.EditorSetData(classType, displayName, prefab, controller, previewSprite);
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void UpdatePresentationDatabase(params PlayerClassPresentationDefinition[] definitions)
        {
            var database = AssetDatabase.LoadAssetAtPath<PlayerClassPresentationDatabase>(PresentationDatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<PlayerClassPresentationDatabase>();
                AssetDatabase.CreateAsset(database, PresentationDatabasePath);
            }

            database.EditorSetPresentations(definitions.Where(definition => definition != null).ToArray());
            EditorUtility.SetDirty(database);
        }

        private static void CreateSkillConfiguration()
        {
            var archerBasic = AssetDatabase.LoadAssetAtPath<AttackDefinition>(ArcherBasicAttackPath);
            var archerArrow = AssetDatabase.LoadAssetAtPath<AttackDefinition>(ArcherArrowAttackPath);
            var warriorSlash = AssetDatabase.LoadAssetAtPath<AttackDefinition>(WarriorSlashPath);
            if (warriorSlash == null)
            {
                warriorSlash = ScriptableObject.CreateInstance<AttackDefinition>();
                AssetDatabase.CreateAsset(warriorSlash, WarriorSlashPath);
            }

            warriorSlash.EditorSetData(
                "player_warrior_slash",
                "Warrior Slash",
                AttackSkillType.BasicMelee,
                8,
                0,
                1.2f,
                0.9f,
                new Vector2(1.35f, 0.95f),
                startup: 0.15f,
                active: 0.08f,
                recovery: 0.25f);
            EditorUtility.SetDirty(warriorSlash);

            var archerSkillSet = CreateSkillSet(ArcherSkillSetPath, PlayerClassType.Archer, archerBasic, archerArrow);
            var warriorSkillSet = CreateSkillSet(WarriorSkillSetPath, PlayerClassType.Warrior, warriorSlash, warriorSlash);
            var database = AssetDatabase.LoadAssetAtPath<PlayerClassSkillDatabase>(SkillDatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<PlayerClassSkillDatabase>();
                AssetDatabase.CreateAsset(database, SkillDatabasePath);
            }

            database.EditorSetSkillSets(new[] { archerSkillSet, warriorSkillSet });
            EditorUtility.SetDirty(database);
        }

        private static PlayerClassSkillDefinition CreateSkillSet(
            string path,
            PlayerClassType classType,
            AttackDefinition basicAttack,
            AttackDefinition secondaryAttack)
        {
            var skillSet = AssetDatabase.LoadAssetAtPath<PlayerClassSkillDefinition>(path);
            if (skillSet == null)
            {
                skillSet = ScriptableObject.CreateInstance<PlayerClassSkillDefinition>();
                AssetDatabase.CreateAsset(skillSet, path);
            }

            skillSet.EditorSetData(classType, basicAttack, secondaryAttack);
            EditorUtility.SetDirty(skillSet);
            return skillSet;
        }

        private static void CreateWarriorFrame(string path, string action)
        {
            const int width = 64;
            const int height = 64;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Clear(texture);

            var armor = new Color32(96, 118, 135, 255);
            var armorLight = new Color32(175, 196, 210, 255);
            var trim = new Color32(198, 112, 43, 255);
            var skin = new Color32(230, 178, 128, 255);
            var dark = new Color32(34, 40, 48, 255);
            var sword = new Color32(225, 230, 235, 255);

            var yOffset = action == PlayerAnimator.States.Jump ? 4 : action == PlayerAnimator.States.Fall ? -3 : 0;
            var xOffset = action == PlayerAnimator.States.Run ? 2 : 0;

            DrawRect(texture, 25 + xOffset, 30 + yOffset, 14, 14, skin);
            DrawRect(texture, 22 + xOffset, 20 + yOffset, 20, 16, armor);
            DrawRect(texture, 24 + xOffset, 24 + yOffset, 16, 4, armorLight);
            DrawRect(texture, 21 + xOffset, 36 + yOffset, 22, 4, trim);
            DrawRect(texture, 25 + xOffset, 14 + yOffset, 5, 8, dark);
            DrawRect(texture, 35 + xOffset, 14 + yOffset, 5, 8, dark);
            DrawRect(texture, 16 + xOffset, 20 + yOffset, 8, 4, armor);
            DrawRect(texture, 41 + xOffset, 20 + yOffset, 8, 4, armor);

            if (action == PlayerAnimator.States.Attack)
            {
                DrawRect(texture, 45, 26 + yOffset, 3, 22, sword);
                DrawRect(texture, 42, 25 + yOffset, 9, 3, trim);
            }
            else
            {
                DrawRect(texture, 47 + xOffset, 18 + yOffset, 3, 20, sword);
                DrawRect(texture, 44 + xOffset, 17 + yOffset, 9, 3, trim);
            }

            if (action == PlayerAnimator.States.Death)
            {
                DrawRect(texture, 22, 18, 24, 8, armor);
                DrawRect(texture, 24, 27, 16, 4, skin);
            }

            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        private static void CreateReadme()
        {
            var path = $"{WarriorFramesRoot}/README.md";
            if (File.Exists(path))
            {
                return;
            }

            File.WriteAllText(
                path,
                "Put Warrior sprite sequence frames in the matching action folder or replace the placeholder PNG files.\n"
                + "Actions: Idle, Run, Death, Attack, GetHit, Jump, Fall.\n"
                + "Sprites should use Bottom Center pivot so the character origin stays at the feet.\n");
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
            importer.spritePivot = CharacterAnchor2D.BottomCenterPivot;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static void EnsureParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
        {
            if (controller.parameters.Any(parameter => parameter.name == name))
            {
                return;
            }

            controller.AddParameter(name, type);
        }

        private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
        {
            foreach (var child in stateMachine.states)
            {
                if (child.state != null && child.state.name == stateName)
                {
                    return child.state;
                }
            }

            return null;
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
                    if (x < 0 || y < 0 || x >= texture.width || y >= texture.height)
                    {
                        continue;
                    }

                    texture.SetPixel(x, y, color);
                }
            }
        }
    }
}
#endif
