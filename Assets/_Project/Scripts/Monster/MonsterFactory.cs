using IdleonGame.Character;
using IdleonGame.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace IdleonGame.Monster
{
    public static class MonsterFactory
    {
        public static MonsterController CreateMonster(MonsterDefinition definition, Vector3 position, Tilemap groundTilemap)
        {
            if (definition == null)
            {
                return null;
            }

            var monsterObject = definition.Prefab != null
                ? Object.Instantiate(definition.Prefab)
                : new GameObject($"Monster_{definition.MonsterId}");
            monsterObject.name = $"Monster_{definition.MonsterId}";
            monsterObject.transform.position = position;
            if (groundTilemap != null && groundTilemap.gameObject.scene.IsValid())
            {
                SceneManager.MoveGameObjectToScene(monsterObject, groundTilemap.gameObject.scene);
            }

            var monsterLayer = LayerMask.NameToLayer(GameLayerNames.Monster);
            if (monsterLayer >= 0)
            {
                monsterObject.layer = monsterLayer;
            }

            var spriteRenderer = monsterObject.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = monsterObject.AddComponent<SpriteRenderer>();
            }

            if (definition.Sprite != null)
            {
                spriteRenderer.sprite = definition.Sprite;
            }

            spriteRenderer.sortingOrder = GameRenderLayers.SortingOrders.Monster;

            var body = monsterObject.GetComponent<Rigidbody2D>();
            if (body == null)
            {
                body = monsterObject.AddComponent<Rigidbody2D>();
            }

            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 3f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            var collider = monsterObject.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = monsterObject.AddComponent<BoxCollider2D>();
            }

            collider.size = CharacterAnchor2D.MonsterColliderSize;
            collider.offset = CharacterAnchor2D.MonsterColliderOffset;

            var stats = monsterObject.GetComponent<CharacterStats>();
            if (stats == null)
            {
                stats = monsterObject.AddComponent<CharacterStats>();
            }

            stats.Configure(
                definition.MaxHealth,
                definition.MaxMana,
                definition.AttackPower,
                definition.Defense);

            if (monsterObject.GetComponent<MonsterAnimator>() == null)
            {
                monsterObject.AddComponent<MonsterAnimator>();
            }

            var controller = monsterObject.GetComponent<MonsterController>();
            if (controller == null)
            {
                controller = monsterObject.AddComponent<MonsterController>();
            }

            controller.Configure(definition, groundTilemap);
            return controller;
        }
    }
}
