using IdleonGame.Character;
using IdleonGame.Items;
using UnityEngine;

namespace IdleonGame.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerClickInteractor : MonoBehaviour
    {
        [SerializeField] private Camera inputCamera;
        [SerializeField] private PlayerAttack attack;
        [SerializeField] private PlayerAutoNavigator autoNavigator;
        [SerializeField] private PlayerInventory inventory;

        private readonly Collider2D[] clickResults = new Collider2D[16];
        private Damageable pendingAttackTarget;

        private void Awake()
        {
            FindReferences();
        }

        private void Update()
        {
            if (pendingAttackTarget != null)
            {
                UpdatePendingAttack();
            }

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                HandlePrimaryClick();
            }
        }

        private void HandlePrimaryClick()
        {
            FindReferences();
            if (inputCamera == null)
            {
                return;
            }

            var mouseWorld = inputCamera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            var clickPoint = new Vector2(mouseWorld.x, mouseWorld.y);
            var hitCount = Physics2D.OverlapPointNonAlloc(clickPoint, clickResults);

            var monster = FindClickedMonster(hitCount, clickPoint);
            if (monster != null)
            {
                HandleMonsterClick(monster);
                return;
            }

            var item = FindClickedItem(hitCount, clickPoint);
            if (item != null)
            {
                pendingAttackTarget = null;
                item.TryPickup(inventory);
                return;
            }

            pendingAttackTarget = null;
            autoNavigator?.TryNavigateToWorldPosition(mouseWorld);
        }

        private void HandleMonsterClick(Damageable monster)
        {
            pendingAttackTarget = monster;
            if (attack != null && attack.TryUseRangedAttack(monster))
            {
                return;
            }

            var targetTransform = (monster as Component)?.transform;
            if (targetTransform != null && autoNavigator != null)
            {
                autoNavigator.TryNavigateToWorldPosition(targetTransform.position);
            }
        }

        private void UpdatePendingAttack()
        {
            if (pendingAttackTarget.IsDead)
            {
                pendingAttackTarget = null;
                return;
            }

            if (attack == null || !attack.IsRangedAttackReady())
            {
                return;
            }

            if (!attack.IsTargetInRangedAttackRange(pendingAttackTarget))
            {
                var targetTransform = (pendingAttackTarget as Component)?.transform;
                if (targetTransform != null)
                {
                    autoNavigator?.TryNavigateToWorldPosition(targetTransform.position);
                }

                return;
            }

            autoNavigator?.StopNavigation();
            attack.TryUseRangedAttack(pendingAttackTarget);
        }

        private Damageable FindClickedMonster(int hitCount, Vector2 clickPoint)
        {
            Damageable bestTarget = null;
            var bestDistance = float.PositiveInfinity;
            for (var i = 0; i < hitCount; i++)
            {
                var hit = clickResults[i];
                if (hit == null)
                {
                    continue;
                }

                var damageable = hit.GetComponentInParent<Damageable>();
                if (damageable == null || damageable.IsDead)
                {
                    continue;
                }

                var distance = Vector2.Distance(clickPoint, hit.ClosestPoint(clickPoint));
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTarget = damageable;
                }
            }

            return bestTarget;
        }

        private WorldItemPickup FindClickedItem(int hitCount, Vector2 clickPoint)
        {
            WorldItemPickup bestItem = null;
            var bestDistance = float.PositiveInfinity;
            for (var i = 0; i < hitCount; i++)
            {
                var hit = clickResults[i];
                if (hit == null)
                {
                    continue;
                }

                var item = hit.GetComponentInParent<WorldItemPickup>();
                if (item == null)
                {
                    continue;
                }

                var distance = Vector2.Distance(clickPoint, hit.ClosestPoint(clickPoint));
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestItem = item;
                }
            }

            return bestItem;
        }

        private void FindReferences()
        {
            if (inputCamera == null)
            {
                inputCamera = Camera.main;
            }

            if (attack == null)
            {
                attack = GetComponent<PlayerAttack>();
            }

            if (autoNavigator == null)
            {
                autoNavigator = GetComponent<PlayerAutoNavigator>();
            }

            if (inventory == null)
            {
                inventory = GetComponent<PlayerInventory>();
            }
        }
    }
}
