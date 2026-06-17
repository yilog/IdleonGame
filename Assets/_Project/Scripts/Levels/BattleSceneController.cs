using IdleonGame.Character;
using IdleonGame.Combat;
using IdleonGame.Core;
using IdleonGame.Data;
using IdleonGame.Items;
using IdleonGame.Player;
using IdleonGame.UI;
using UnityEngine;

namespace IdleonGame.Levels
{
    [DisallowMultipleComponent]
    public sealed class BattleSceneController : MonoBehaviour
    {
        private const string PlayerObjectName = "Player_TestBlock";
        private const string PlayerAnimatorControllerPath = "Animations/Archer/Archer";
        private const string PlayerPrefabPath = "Prefabs/Characters/Archer";

        [SerializeField] private AttackDefinition basicAttack;
        [SerializeField] private AttackDefinition rangedAttack;
        [SerializeField] private Vector2 initialPlayerPosition = new(-8f, -2f);

        private GameObject player;
        private Sprite runtimePlayerSprite;
        private PlayerClassPresentationDefinition cachedPresentation;

        public GameObject Player => player;

        private void Awake()
        {
            PlayerRuntimeDataService.EnsureExists();
            UIManager.EnsureExists();
            EnsurePlayer();
        }

        private void Start()
        {
            SwitchPlayerClass(PlayerClassType.Warrior);
        }

        public GameObject EnsurePlayer()
        {
            if (player != null)
            {
                return player;
            }

            player = GameObject.Find(PlayerObjectName);
            if (player == null)
            {
                player = CreatePlayer(initialPlayerPosition);
            }

            ConfigurePlayer(player);
            return player;
        }

        public void MovePlayerTo(Vector2 position)
        {
            var currentPlayer = EnsurePlayer();
            currentPlayer.transform.position = new Vector3(position.x, position.y, currentPlayer.transform.position.z);

            var body = currentPlayer.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.velocity = Vector2.zero;
                body.angularVelocity = 0f;
            }

            currentPlayer.GetComponent<PlayerAutoNavigator>()?.StopNavigation();
            currentPlayer.GetComponent<PlayerClimb>()?.StopClimbingForNavigation();
        }

        public bool SwitchPlayerClass(PlayerClassType playerClass)
        {
            var runtimeData = PlayerRuntimeDataService.EnsureExists();
            runtimeData.SetPlayerClass(playerClass);
            cachedPresentation = null;

            var currentPlayer = EnsurePlayer();
            currentPlayer.GetComponent<PlayerAutoNavigator>()?.StopNavigation();
            currentPlayer.GetComponent<PlayerClimb>()?.StopClimbingForNavigation();
            currentPlayer.GetComponent<PlayerSkillController>()?.CancelCurrentSkill();

            var presentation = GetPlayerPresentation();
            if (presentation == null)
            {
                return false;
            }

            ApplyPresentation(currentPlayer, presentation, true);
            ConfigurePlayerSkills(currentPlayer);
            return true;
        }

        private GameObject CreatePlayer(Vector2 position)
        {
            var created = CreatePlayerRoot();
            created.name = PlayerObjectName;
            created.transform.position = new Vector3(position.x, position.y, 0f);

            var body = created.GetComponent<Rigidbody2D>();
            if (body == null)
            {
                body = created.AddComponent<Rigidbody2D>();
            }

            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 3f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            var collider = created.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = created.AddComponent<BoxCollider2D>();
            }

            collider.size = CharacterAnchor2D.PlayerColliderSize;
            collider.offset = CharacterAnchor2D.PlayerColliderOffset;

            EnsurePresentationComponents(created);

            EnsureComponent<CharacterStats>(created);
            EnsureComponent<PlayerRuntimeDataBinder>(created);
            EnsureComponent<PlayerInventory>(created);
            EnsureComponent<PlayerEquipment>(created);
            EnsureComponent<PlayerClimb>(created);
            EnsureComponent<PlayerMovement>(created);
            EnsureComponent<PlayerAutoNavigator>(created);
            EnsureComponent<PlayerSkillController>(created);
            EnsureComponent<PlayerAttack>(created);
            EnsureComponent<PlayerClickInteractor>(created);
            EnsureComponent<PlayerController>(created);

            return created;
        }

        private GameObject CreatePlayerRoot()
        {
            var presentation = GetPlayerPresentation();
            var prefab = presentation != null && presentation.Prefab != null
                ? presentation.Prefab
                : Resources.Load<GameObject>(PlayerPrefabPath);
            if (prefab != null)
            {
                return Instantiate(prefab);
            }

            Debug.LogWarning($"Player prefab was not found in Resources: {PlayerPrefabPath}");
            return new GameObject(PlayerObjectName);
        }

        private void EnsurePresentationComponents(GameObject target)
        {
            var presentation = GetPlayerPresentation();
            ApplyPresentation(target, presentation, false);
            EnsureComponent<PlayerAnimator>(target);
        }

        private void ApplyPresentation(GameObject target, PlayerClassPresentationDefinition presentation, bool force)
        {
            var spriteRenderer = target.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                var view = new GameObject("view");
                view.transform.SetParent(target.transform, false);
                spriteRenderer = view.AddComponent<SpriteRenderer>();
            }

            var sourceRenderer = presentation != null && presentation.Prefab != null
                ? presentation.Prefab.GetComponentInChildren<SpriteRenderer>()
                : null;
            if (sourceRenderer != null)
            {
                spriteRenderer.sprite = sourceRenderer.sprite;
                spriteRenderer.transform.localPosition = sourceRenderer.transform.localPosition;
                spriteRenderer.transform.localScale = sourceRenderer.transform.localScale;
            }
            else if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = GetOrCreateRuntimePlayerSprite();
            }

            spriteRenderer.sortingOrder = GameRenderLayers.SortingOrders.Player;

            var animator = target.GetComponent<Animator>();
            if (animator == null)
            {
                animator = target.AddComponent<Animator>();
            }

            var controller = presentation != null && presentation.AnimatorController != null
                ? presentation.AnimatorController
                : GetPlayerAnimatorController();
            if (force || animator.runtimeAnimatorController == null)
            {
                animator.runtimeAnimatorController = controller;
            }
        }

        private static T EnsureComponent<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private void ConfigurePlayer(GameObject target)
        {
            EnsurePresentationComponents(target);

            var playerLayer = LayerMask.NameToLayer(GameLayerNames.Player);
            if (playerLayer >= 0)
            {
                target.layer = playerLayer;
            }

            var stats = target.GetComponent<CharacterStats>();
            if (stats != null && stats.CurrentHealth <= 0)
            {
                stats.Configure(100, 50, 6, 1);
            }
            PlayerRuntimeDataService.Instance?.SyncFromStats(stats);
            EnsureComponent<PlayerRuntimeDataBinder>(target);

            EnsureComponent<PlayerSkillController>(target);
            ConfigurePlayerSkills(target);

            var animator = target.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController == null)
            {
                animator.runtimeAnimatorController = GetPlayerAnimatorController();
            }
        }

        private void ConfigurePlayerSkills(GameObject target)
        {
            var attack = target.GetComponent<PlayerAttack>();
            if (attack == null)
            {
                return;
            }

            var playerClass = PlayerRuntimeDataService.Instance != null
                ? PlayerRuntimeDataService.Instance.Data.playerClass
                : PlayerClassType.Archer;
            var skillSet = PlayerClassSkillDatabase.Find(playerClass)
                ?? PlayerClassSkillDatabase.Find(PlayerClassType.Archer);
            var classBasicAttack = skillSet != null && skillSet.BasicAttack != null ? skillSet.BasicAttack : basicAttack;
            var classSecondaryAttack = skillSet != null && skillSet.SecondaryAttack != null ? skillSet.SecondaryAttack : rangedAttack;
            attack.Configure(classBasicAttack, classSecondaryAttack, LayerMask.GetMask(GameLayerNames.Monster));
        }

        private PlayerClassPresentationDefinition GetPlayerPresentation()
        {
            var playerClass = PlayerRuntimeDataService.Instance != null
                ? PlayerRuntimeDataService.Instance.Data.playerClass
                : PlayerClassType.Archer;
            cachedPresentation = PlayerClassPresentationDatabase.Find(playerClass)
                ?? PlayerClassPresentationDatabase.Find(PlayerClassType.Archer);
            return cachedPresentation;
        }

        private RuntimeAnimatorController GetPlayerAnimatorController()
        {
            var presentation = cachedPresentation != null ? cachedPresentation : GetPlayerPresentation();
            return presentation != null && presentation.AnimatorController != null
                ? presentation.AnimatorController
                : Resources.Load<RuntimeAnimatorController>(PlayerAnimatorControllerPath);
        }

        private Sprite GetOrCreateRuntimePlayerSprite()
        {
            if (runtimePlayerSprite != null)
            {
                return runtimePlayerSprite;
            }

            var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };

            for (var y = 0; y < texture.height; y++)
            {
                for (var x = 0; x < texture.width; x++)
                {
                    var border = x == 0 || y == 0 || x == texture.width - 1 || y == texture.height - 1;
                    texture.SetPixel(x, y, border ? new Color32(23, 45, 80, 255) : new Color32(66, 135, 245, 255));
                }
            }

            texture.Apply();
            runtimePlayerSprite = Sprite.Create(texture, new Rect(0f, 0f, 16f, 16f), CharacterAnchor2D.BottomCenterPivot, 16f);
            return runtimePlayerSprite;
        }
    }
}
