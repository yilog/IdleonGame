#if UNITY_EDITOR
using System.IO;
using IdleonGame.Character;
using IdleonGame.Core;
using IdleonGame.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace IdleonGame.Editor
{
    public static class CreatePlayerPresentationAssets
    {
        private const string CharacterName = "Archer";
        private const string FramesRoot = "Assets/_Project/Art/Characters/Archer/Frames";
        private const string AnimationFolder = "Assets/_Project/Resources/Animations/Archer";
        private const string AnimatorControllerPath = AnimationFolder + "/Archer.controller";
        private const string PrefabFolder = "Assets/_Project/Resources/Prefabs/Characters";
        private const string ArcherPrefabPath = PrefabFolder + "/Archer.prefab";
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

        [MenuItem("IdleonGame/Setup/Create Player Presentation Assets")]
        public static void CreateAssets()
        {
            EnsureFolders();
            CreateFrameReadme();
            ConfigureArcherFrameImporters();

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(AnimatorControllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(AnimatorControllerPath);
            }

            EnsureAnimatorParameters(controller);
            EnsureAnimatorStates(controller);
            EnsureArcherPrefab(controller);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(FramesRoot);
            Directory.CreateDirectory(AnimationFolder);
            Directory.CreateDirectory(PrefabFolder);

            foreach (var action in Actions)
            {
                Directory.CreateDirectory($"{FramesRoot}/{action}");
            }
        }

        private static void CreateFrameReadme()
        {
            var path = $"{FramesRoot}/README.md";
            if (File.Exists(path))
            {
                return;
            }

            File.WriteAllText(
                path,
                "Put Archer sprite sequence frames in the matching action folder.\n"
                + "Folders: Idle, Run, Death, Attack, GetHit, Jump, Fall.\n"
                + "Import every frame as Sprite with Pivot set to Bottom Center so the character origin is at the feet.\n"
                + "The generated AnimationClips are empty placeholders; drag the frames into each clip in Unity.\n");
        }

        private static void EnsureAnimatorParameters(AnimatorController controller)
        {
            EnsureParameter(controller, PlayerAnimator.Parameters.MoveSpeed, AnimatorControllerParameterType.Float);
            EnsureParameter(controller, PlayerAnimator.Parameters.VerticalSpeed, AnimatorControllerParameterType.Float);
            EnsureParameter(controller, PlayerAnimator.Parameters.IsGrounded, AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, PlayerAnimator.Parameters.IsDead, AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, PlayerAnimator.Parameters.Attack, AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, PlayerAnimator.Parameters.GetHit, AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, PlayerAnimator.Parameters.Death, AnimatorControllerParameterType.Trigger);
        }

        private static void EnsureParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
        {
            foreach (var parameter in controller.parameters)
            {
                if (parameter.name == name)
                {
                    return;
                }
            }

            controller.AddParameter(name, type);
        }

        private static void EnsureAnimatorStates(AnimatorController controller)
        {
            var stateMachine = controller.layers[0].stateMachine;
            foreach (var action in Actions)
            {
                var clip = EnsureAnimationClip(action);
                var state = FindState(stateMachine, action);
                if (state == null)
                {
                    state = stateMachine.AddState(action);
                }

                state.motion = clip;
                if (action == PlayerAnimator.States.Idle)
                {
                    stateMachine.defaultState = state;
                }
            }
        }

        private static AnimationClip EnsureAnimationClip(string action)
        {
            var path = $"{AnimationFolder}/{CharacterName}_{action}.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip
                {
                    frameRate = 12f
                };
                AssetDatabase.CreateAsset(clip, path);
            }

            return clip;
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

        private static void EnsureArcherPrefab(RuntimeAnimatorController controller)
        {
            var prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(ArcherPrefabPath);
            if (prefabRoot == null)
            {
                var created = new GameObject(CharacterName);
                EnsurePresentationComponents(created, controller);
                PrefabUtility.SaveAsPrefabAsset(created, ArcherPrefabPath);
                Object.DestroyImmediate(created);
                return;
            }

            var contents = PrefabUtility.LoadPrefabContents(ArcherPrefabPath);
            try
            {
                EnsurePresentationComponents(contents, controller);
                PrefabUtility.SaveAsPrefabAsset(contents, ArcherPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static void EnsurePresentationComponents(GameObject root, RuntimeAnimatorController controller)
        {
            var spriteRenderer = root.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = root.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sortingOrder = GameRenderLayers.SortingOrders.Player;
            spriteRenderer.transform.localPosition = Vector3.zero;
            spriteRenderer.transform.localScale = Vector3.one;

            var animator = root.GetComponent<Animator>();
            if (animator == null)
            {
                animator = root.AddComponent<Animator>();
            }

            animator.runtimeAnimatorController = controller;

            if (root.GetComponent<PlayerAnimator>() == null)
            {
                root.AddComponent<PlayerAnimator>();
            }
        }

        private static void ConfigureArcherFrameImporters()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { FramesRoot });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                if (importer.spriteImportMode != SpriteImportMode.Multiple)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                }

                importer.spritePivot = CharacterAnchor2D.BottomCenterPivot;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;

                if (importer.spriteImportMode == SpriteImportMode.Multiple)
                {
                    var spritesheet = importer.spritesheet;
                    for (var i = 0; i < spritesheet.Length; i++)
                    {
                        spritesheet[i].alignment = (int)SpriteAlignment.Custom;
                        spritesheet[i].pivot = CharacterAnchor2D.BottomCenterPivot;
                    }

                    importer.spritesheet = spritesheet;
                }

                importer.SaveAndReimport();
            }
        }
    }
}
#endif
