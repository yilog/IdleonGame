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

        [SerializeField] private AttackDefinition basicAttack;
        [SerializeField] private AttackDefinition rangedAttack;
        [SerializeField] private Vector2 initialPlayerPosition = new(-8f, -1.5f);

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
            var created = new GameObject(PlayerObjectName);
            created.transform.position = new Vector3(position.x, position.y, 0f);

            var spriteRenderer = created.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetOrCreateRuntimePlayerSprite();
            spriteRenderer.sortingOrder = GameRenderLayers.SortingOrders.Player;

            var body = created.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 3f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            var collider = created.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.9f, 0.95f);

            created.AddComponent<CharacterStats>();
            created.AddComponent<PlayerInventory>();
            created.AddComponent<PlayerClimb>();
            created.AddComponent<PlayerMovement>();
            created.AddComponent<PlayerAutoNavigator>();
            created.AddComponent<PlayerAttack>();
            created.AddComponent<PlayerClickInteractor>();
            created.AddComponent<PlayerController>();

            return created;
        }

        private void ConfigurePlayer(GameObject target)
        {
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
            runtimePlayerSprite = Sprite.Create(texture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 16f);
            return runtimePlayerSprite;
        }
    }
}
