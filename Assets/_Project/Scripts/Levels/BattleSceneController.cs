using IdleonGame.Character;
using IdleonGame.Combat;
using IdleonGame.Core;
using IdleonGame.Items;
using IdleonGame.Player;
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

        public GameObject Player => player;

        private void Awake()
        {
            EnsurePlayer();
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
            EnsureComponent<PlayerInventory>(created);
            EnsureComponent<PlayerClimb>(created);
            EnsureComponent<PlayerMovement>(created);
            EnsureComponent<PlayerAutoNavigator>(created);
            EnsureComponent<PlayerAttack>(created);
            EnsureComponent<PlayerClickInteractor>(created);
            EnsureComponent<PlayerController>(created);

            return created;
        }

        private GameObject CreatePlayerRoot()
        {
            var prefab = Resources.Load<GameObject>(PlayerPrefabPath);
            if (prefab != null)
            {
                return Instantiate(prefab);
            }

            Debug.LogWarning($"Player prefab was not found in Resources: {PlayerPrefabPath}");
            return new GameObject(PlayerObjectName);
        }

        private void EnsurePresentationComponents(GameObject target)
        {
            var spriteRenderer = target.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = target.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = GetOrCreateRuntimePlayerSprite();
            }

            //spriteRenderer.transform.localPosition = Vector3.zero;
            //spriteRenderer.transform.localScale = Vector3.one;
            spriteRenderer.sortingOrder = GameRenderLayers.SortingOrders.Player;

            var animator = target.GetComponent<Animator>();
            if (animator == null)
            {
                animator = target.AddComponent<Animator>();
            }

            if (animator.runtimeAnimatorController == null)
            {
                animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(PlayerAnimatorControllerPath);
            }

            EnsureComponent<PlayerAnimator>(target);
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

            var attack = target.GetComponent<PlayerAttack>();
            if (attack != null)
            {
                attack.Configure(basicAttack, rangedAttack, LayerMask.GetMask(GameLayerNames.Monster));
            }

            var animator = target.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController == null)
            {
                animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(PlayerAnimatorControllerPath);
            }
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
