using IdleonGame.Character;
using IdleonGame.Items;
using IdleonGame.Levels;
using IdleonGame.Map;
using IdleonGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IdleonGame.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerClickInteractor : MonoBehaviour, ILevelSceneReferenceClient
    {
        [SerializeField] private Camera inputCamera;
        [SerializeField] private PlayerAttack attack;
        [SerializeField] private PlayerAutoNavigator autoNavigator;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private KeyCode autoHuntKey = KeyCode.L;

        private readonly Collider2D[] clickResults = new Collider2D[16];
        private Damageable pendingAttackTarget;
        private MapPortal pendingPortal;
        private bool isAutoHunting;

        public bool IsAutoHunting => isAutoHunting;

        private void Awake()
        {
            FindReferences();
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(autoHuntKey))
            {
                ToggleAutoHunt();
            }

            if (isAutoHunting && pendingAttackTarget == null)
            {
                pendingAttackTarget = FindNearestMonster();
            }

            if (pendingAttackTarget != null)
            {
                UpdatePendingAttack();
            }

            if (pendingPortal != null)
            {
                UpdatePendingPortal();
            }

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                if (UIInputBlocker.ShouldBlockScenePointerInput())
                {
                    return;
                }

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

            var portal = FindClickedPortal(hitCount, clickPoint);
            if (portal != null)
            {
                HandlePortalClick(portal);
                return;
            }

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
            pendingPortal = null;
            autoNavigator?.TryNavigateToWorldPosition(mouseWorld);
        }

        public void OnLevelSceneWillUnload(Scene scene)
        {
            if (!scene.IsValid())
            {
                return;
            }

            var pendingComponent = pendingAttackTarget as Component;
            if (pendingComponent != null && pendingComponent.gameObject.scene == scene)
            {
                pendingAttackTarget = null;
            }

            if (pendingPortal != null && pendingPortal.gameObject.scene == scene)
            {
                pendingPortal = null;
            }
        }

        public void OnLevelSceneLoaded(Scene scene)
        {
            if (!scene.IsValid())
            {
                return;
            }

            FindReferences();
        }

        private void HandleMonsterClick(Damageable monster)
        {
            pendingPortal = null;
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

        private void HandlePortalClick(MapPortal portal)
        {
            pendingAttackTarget = null;
            if (!portal.IsActive)
            {
                pendingPortal = null;
                autoNavigator?.StopNavigation();
                return;
            }

            pendingPortal = portal;

            if (pendingPortal.IsInPortalCell(transform.position))
            {
                ActivatePendingPortal();
                return;
            }

            if (autoNavigator == null || !autoNavigator.TryNavigateToWorldPosition(pendingPortal.WorldPosition))
            {
                pendingPortal = null;
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

        public void ToggleAutoHunt()
        {
            SetAutoHuntEnabled(!isAutoHunting);
        }

        public void SetAutoHuntEnabled(bool enabled)
        {
            isAutoHunting = enabled;
            if (!isAutoHunting)
            {
                pendingAttackTarget = null;
                pendingPortal = null;
                autoNavigator?.StopNavigation();
            }
        }

        private void UpdatePendingPortal()
        {
            if (pendingPortal == null)
            {
                return;
            }

            if (!pendingPortal.IsInPortalCell(transform.position))
            {
                return;
            }

            ActivatePendingPortal();
        }

        private void ActivatePendingPortal()
        {
            var portal = pendingPortal;
            pendingPortal = null;
            autoNavigator?.StopNavigation();
            portal.TryActivate();
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

        private MapPortal FindClickedPortal(int hitCount, Vector2 clickPoint)
        {
            MapPortal bestPortal = null;
            var bestDistance = float.PositiveInfinity;
            for (var i = 0; i < hitCount; i++)
            {
                var hit = clickResults[i];
                if (hit == null)
                {
                    continue;
                }

                var portal = hit.GetComponentInParent<MapPortal>();
                if (portal == null)
                {
                    continue;
                }

                var distance = Vector2.Distance(clickPoint, hit.ClosestPoint(clickPoint));
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestPortal = portal;
                }
            }

            return bestPortal;
        }

        private Damageable FindNearestMonster()
        {
            Damageable nearest = null;
            var nearestDistance = float.PositiveInfinity;
            var behaviours = FindObjectsOfType<MonoBehaviour>();
            foreach (var behaviour in behaviours)
            {
                var damageable = behaviour as Damageable;
                if (damageable == null || damageable.IsDead)
                {
                    continue;
                }

                var distance = Vector2.Distance(transform.position, behaviour.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = damageable;
                }
            }

            return nearest;
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
