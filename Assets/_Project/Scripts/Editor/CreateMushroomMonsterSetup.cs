#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IdleonGame.Core;
using IdleonGame.Monster;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace IdleonGame.Editor
{
    public static class CreateMushroomMonsterSetup
    {
        private static readonly string[] AnimationNames =
        {
            MonsterAnimator.States.Idle,
            MonsterAnimator.States.Run,
            MonsterAnimator.States.Death,
            MonsterAnimator.States.Attack,
            MonsterAnimator.States.GetHit,
            MonsterAnimator.States.Jump,
            MonsterAnimator.States.Fall
        };

        private const string MonsterId = "mushroom";
        private const string MonsterName = "Mushroom";
        private const string ArtFolder = "Assets/_Project/Art/Monsters/Mushroom/Frames";
        private const string AnimationFolder = "Assets/_Project/Resources/Animations/Monsters/Mushroom";
        private const string PrefabPath = "Assets/_Project/Resources/Prefabs/Monsters/Mushroom.prefab";
        private const string DefinitionPath = "Assets/_Project/ScriptableObjects/Monsters/Mushroom.asset";
        private const string MonsterDatabasePath = "Assets/_Project/Resources/MonsterDatabase.asset";
        private const string SpawnTexturePath = "Assets/_Project/Tilemaps/Tiles/MushroomMonsterSpawnTile.png";
        private const string SpawnTilePath = "Assets/_Project/Tilemaps/Tiles/MushroomMonsterSpawnTile.asset";

        [MenuItem("IdleonGame/Setup/Create Mushroom Monster Setup")]
        public static void CreateAssets()
        {
            Directory.CreateDirectory(ArtFolder);
            Directory.CreateDirectory(AnimationFolder);
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            Directory.CreateDirectory(Path.GetDirectoryName(DefinitionPath));

            var frameSprites = CreateFrameSprites();
            var clips = CreateAnimationClips(frameSprites);
            var controller = CreateAnimatorController(clips);
            var prefab = CreatePrefab(frameSprites[MonsterAnimator.States.Idle], controller);
            var definition = CreateDefinition(prefab, frameSprites[MonsterAnimator.States.Idle]);
            UpdateMonsterDatabase(definition);
            CreateSpawnTile();
            SyncProjectTilePalette.SyncPalette();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Dictionary<string, Sprite> CreateFrameSprites()
        {
            var sprites = new Dictionary<string, Sprite>();
            foreach (var animationName in AnimationNames)
            {
                var path = $"{ArtFolder}/{animationName}.png";
                CreateMushroomFrame(path, animationName);
                AssetDatabase.ImportAsset(path);
                ConfigureTextureImporter(path, 64);
                sprites[animationName] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }

            return sprites;
        }

        private static Dictionary<string, AnimationClip> CreateAnimationClips(IReadOnlyDictionary<string, Sprite> frameSprites)
        {
            var clips = new Dictionary<string, AnimationClip>();
            foreach (var animationName in AnimationNames)
            {
                var path = $"{AnimationFolder}/Mushroom_{animationName}.anim";
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null)
                {
                    clip = new AnimationClip();
                    AssetDatabase.CreateAsset(clip, path);
                }

                clip.name = $"Mushroom_{animationName}";
                clip.frameRate = 12f;

                var binding = new EditorCurveBinding
                {
                    path = "view",
                    type = typeof(SpriteRenderer),
                    propertyName = "m_Sprite"
                };

                var sprite = frameSprites[animationName];
                var frames = animationName == MonsterAnimator.States.Run || animationName == MonsterAnimator.States.Idle
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
                clips[animationName] = clip;
            }

            return clips;
        }

        private static AnimatorController CreateAnimatorController(IReadOnlyDictionary<string, AnimationClip> clips)
        {
            var controllerPath = $"{AnimationFolder}/Mushroom.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            }

            EnsureParameter(controller, MonsterAnimator.Parameters.MoveSpeed, AnimatorControllerParameterType.Float);
            EnsureParameter(controller, MonsterAnimator.Parameters.VerticalSpeed, AnimatorControllerParameterType.Float);
            EnsureParameter(controller, MonsterAnimator.Parameters.IsGrounded, AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, MonsterAnimator.Parameters.IsDead, AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, MonsterAnimator.Parameters.Attack, AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, MonsterAnimator.Parameters.GetHit, AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, MonsterAnimator.Parameters.Death, AnimatorControllerParameterType.Trigger);

            if (controller.layers == null || controller.layers.Length == 0)
            {
                controller.AddLayer("Base Layer");
            }

            var stateMachine = controller.layers[0].stateMachine;
            foreach (var animationName in AnimationNames)
            {
                var state = FindState(stateMachine, animationName) ?? stateMachine.AddState(animationName);
                state.motion = clips[animationName];
                if (animationName == MonsterAnimator.States.Idle)
                {
                    stateMachine.defaultState = state;
                }
            }

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static GameObject CreatePrefab(Sprite idleSprite, RuntimeAnimatorController controller)
        {
            GameObject root = null;
            var loadedPrefabContents = File.Exists(PrefabPath);
            try
            {
                root = loadedPrefabContents
                    ? PrefabUtility.LoadPrefabContents(PrefabPath)
                    : new GameObject(MonsterName, typeof(Animator));

                root.name = MonsterName;
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
                view.localScale = new Vector3(2.5f, 2.5f, 1f);

                var spriteRenderer = view.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    spriteRenderer = view.gameObject.AddComponent<SpriteRenderer>();
                }

                spriteRenderer.sprite = idleSprite;
                spriteRenderer.sortingOrder = GameRenderLayers.SortingOrders.Monster;

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
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

            return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        }

        private static MonsterDefinition CreateDefinition(GameObject prefab, Sprite sprite)
        {
            var definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(DefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<MonsterDefinition>();
                AssetDatabase.CreateAsset(definition, DefinitionPath);
            }

            var drop = new MonsterDropEntry();
            drop.EditorSetData("test_drop_item", 1, 1, 0.3f);
            definition.EditorSetData(
                MonsterId,
                MonsterName,
                prefab,
                sprite,
                Vector2Int.one,
                45,
                0,
                4,
                1,
                140d,
                MonsterAttackType.Contact,
                1.6f,
                0.5f,
                1.2f,
                2.8f,
                1.0f,
                2.5f,
                false,
                2f,
                new[] { drop },
                0.8f,
                8,
                18);

            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void UpdateMonsterDatabase(MonsterDefinition mushroomDefinition)
        {
            var database = AssetDatabase.LoadAssetAtPath<MonsterDatabase>(MonsterDatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<MonsterDatabase>();
                AssetDatabase.CreateAsset(database, MonsterDatabasePath);
            }

            var serialized = new SerializedObject(database);
            var monstersProperty = serialized.FindProperty("monsters");
            var monsters = new List<MonsterDefinition>();
            for (var i = 0; i < monstersProperty.arraySize; i++)
            {
                var monster = monstersProperty.GetArrayElementAtIndex(i).objectReferenceValue as MonsterDefinition;
                if (monster != null && monster.MonsterId != MonsterId)
                {
                    monsters.Add(monster);
                }
            }

            monsters.Add(mushroomDefinition);
            database.EditorSetMonsters(monsters.ToArray());
            EditorUtility.SetDirty(database);
        }

        private static void CreateSpawnTile()
        {
            CreateSpawnTexture();
            AssetDatabase.ImportAsset(SpawnTexturePath);
            ConfigureTextureImporter(SpawnTexturePath, 64);

            var tile = AssetDatabase.LoadAssetAtPath<MonsterSpawnTile>(SpawnTilePath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<MonsterSpawnTile>();
                tile.name = "MushroomMonsterSpawnTile";
                AssetDatabase.CreateAsset(tile, SpawnTilePath);
            }

            tile.EditorSetData(MonsterId, 5f, 10, 0.5f, AssetDatabase.LoadAssetAtPath<Sprite>(SpawnTexturePath));
            EditorUtility.SetDirty(tile);
        }

        private static void CreateMushroomFrame(string path, string state)
        {
            const int width = 64;
            const int height = 64;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Clear(texture);

            var cap = new Color32(188, 62, 65, 255);
            var capShadow = new Color32(124, 42, 48, 255);
            var spots = new Color32(244, 218, 164, 255);
            var stem = new Color32(219, 185, 132, 255);
            var stemShadow = new Color32(154, 116, 82, 255);
            var eye = new Color32(45, 31, 31, 255);

            var offsetY = state == MonsterAnimator.States.Jump ? 4 : state == MonsterAnimator.States.Fall ? -3 : 0;
            var lean = state == MonsterAnimator.States.Run ? 2 : state == MonsterAnimator.States.GetHit ? -3 : 0;
            var squash = state == MonsterAnimator.States.Attack ? 3 : 0;

            DrawEllipse(texture, 32 + lean, 36 + offsetY, 23 + squash, 12, capShadow);
            DrawEllipse(texture, 32 + lean, 39 + offsetY, 25 + squash, 14, cap);
            DrawEllipse(texture, 23 + lean, 42 + offsetY, 4, 3, spots);
            DrawEllipse(texture, 32 + lean, 47 + offsetY, 5, 3, spots);
            DrawEllipse(texture, 43 + lean, 40 + offsetY, 4, 3, spots);

            DrawRect(texture, 23 + lean, 14 + offsetY, 18, 20, stemShadow);
            DrawRect(texture, 25 + lean, 17 + offsetY, 14, 20, stem);
            DrawEllipse(texture, 32 + lean, 16 + offsetY, 14, 5, stemShadow);
            DrawEllipse(texture, 32 + lean, 19 + offsetY, 12, 5, stem);
            DrawRect(texture, 27 + lean, 28 + offsetY, 3, 4, eye);
            DrawRect(texture, 36 + lean, 28 + offsetY, 3, 4, eye);

            if (state == MonsterAnimator.States.Death)
            {
                DrawRect(texture, 25 + lean, 28 + offsetY, 7, 2, eye);
                DrawRect(texture, 35 + lean, 28 + offsetY, 7, 2, eye);
            }

            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        private static void CreateSpawnTexture()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Clear(texture);
            DrawRect(texture, 6, 6, 52, 52, new Color32(93, 50, 45, 190));
            DrawRect(texture, 9, 9, 46, 46, new Color32(196, 73, 78, 235));
            DrawEllipse(texture, 32, 39, 20, 11, new Color32(188, 62, 65, 255));
            DrawRect(texture, 25, 18, 14, 20, new Color32(219, 185, 132, 255));
            DrawEllipse(texture, 25, 42, 4, 3, new Color32(244, 218, 164, 255));
            DrawEllipse(texture, 38, 43, 4, 3, new Color32(244, 218, 164, 255));
            texture.Apply();
            File.WriteAllBytes(SpawnTexturePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        private static void ConfigureTextureImporter(string path, float pixelsPerUnit)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static void EnsureParameter(AnimatorController controller, string parameterName, AnimatorControllerParameterType type)
        {
            if (controller.parameters.Any(parameter => parameter.name == parameterName))
            {
                return;
            }

            controller.AddParameter(parameterName, type);
        }

        private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
        {
            foreach (var childState in stateMachine.states)
            {
                if (childState.state.name == stateName)
                {
                    return childState.state;
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
                    SetPixel(texture, x, y, color);
                }
            }
        }

        private static void DrawEllipse(Texture2D texture, int centerX, int centerY, int radiusX, int radiusY, Color color)
        {
            var radiusXSquared = radiusX * radiusX;
            var radiusYSquared = radiusY * radiusY;
            var limit = radiusXSquared * radiusYSquared;
            for (var y = centerY - radiusY; y <= centerY + radiusY; y++)
            {
                for (var x = centerX - radiusX; x <= centerX + radiusX; x++)
                {
                    var dx = x - centerX;
                    var dy = y - centerY;
                    if (dx * dx * radiusYSquared + dy * dy * radiusXSquared <= limit)
                    {
                        SetPixel(texture, x, y, color);
                    }
                }
            }
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
#endif
