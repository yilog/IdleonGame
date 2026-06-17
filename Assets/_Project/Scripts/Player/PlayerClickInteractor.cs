using IdleonGame.Character;
using IdleonGame.Items;
using IdleonGame.Levels;
using IdleonGame.Markers;
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
        [SerializeField] private float attackNavigationRepathDistance = 0.5f;
        [SerializeField] private float attackNavigationRepathInterval = 0.4f;

        private readonly Collider2D[] clickResults = new Collider2D[16];
        private Damageable pendingAttackTarget;
        private MapPortal pendingPortal;
        private PlayerAutoNavigator subscribedNavigator;
        private bool isAutoHunting;
        private Vector3 lastAttackNavigationTargetWorld;
        private float nextAttackNavigationRepathTime;

        public bool IsAutoHunting => isAutoHunting;

        private void Awake()
        {
            FindReferences();
        }

        private void OnDestroy()
        {
            UnsubscribeNavigator();
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
                ResetAttackNavigationState();
            }

            if (pendingAttackTarget != null)
            {
                UpdatePendingAttack();
            }

            if (pendingPortal != null)
            {
                UpdatePendingPortal();
            }

            if (UnityEngine.Input.GetMouseButton(0))
            {
                if (!UIInputBlocker.ShouldBlockScenePointerInput() && TryPickupWorldItemUnderMouse())
                {
                    return;
                }
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
                SceneMarkerManager.EnsureExists().HideMoveTarget();
                SceneMarkerManager.EnsureExists().HideAttackTargetImmediately();
                HandlePortalClick(portal);
                return;
            }

            var monster = FindClickedMonster(hitCount, clickPoint);
            if (monster != null)
            {
                SceneMarkerManager.EnsureExists().HideMoveTarget();
                HandleMonsterClick(monster);
                return;
            }

            var item = FindClickedItem(hitCount, clickPoint);
            if (item != null)
            {
                pendingAttackTarget = null;
                SceneMarkerManager.EnsureExists().HideAttackTargetImmediately();
                SceneMarkerManager.EnsureExists().HideMoveTarget();
                item.TryPickup(inventory);
                return;
            }

            pendingAttackTarget = null;
            pendingPortal = null;
            SceneMarkerManager.EnsureExists().HideAttackTargetImmediately();
            if (autoNavigator != null && autoNavigator.TryNavigateToWorldPosition(mouseWorld))
            {
                SceneMarkerManager.EnsureExists().ShowMoveTarget(autoNavigator.CurrentDestinationWorld);
            }
            else
            {
                SceneMarkerManager.EnsureExists().HideMoveTarget();
            }
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
                SceneMarkerManager.EnsureExists().HideAttackTargetImmediately();
            }

            if (pendingPortal != null && pendingPortal.gameObject.scene == scene)
            {
                pendingPortal = null;
            }

            SceneMarkerManager.EnsureExists().HideMoveTarget();
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
            ResetAttackNavigationState();
            SceneMarkerManager.EnsureExists().ShowAttackTarget(monster);
            if (attack != null && attack.TryUseRangedAttack(monster))
            {
                SceneMarkerManager.EnsureExists().RequestHideAttackTarget(monster);
                return;
            }

            var targetTransform = (monster as Component)?.transform;
            if (targetTransform != null && autoNavigator != null)
            {
                TryNavigateToAttackTarget(targetTransform.position, true);
            }
        }

        private void HandlePortalClick(MapPortal portal)
        {
            pendingAttackTarget = null;
            SceneMarkerManager.EnsureExists().HideAttackTargetImmediately();
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
                ResetAttackNavigationState();
                SceneMarkerManager.EnsureExists().HideAttackTargetImmediately();
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
                    TryNavigateToAttackTarget(targetTransform.position, false);
                }

                return;
            }

            autoNavigator?.StopNavigation();
            if (attack.TryUseRangedAttack(pendingAttackTarget))
            {
                SceneMarkerManager.EnsureExists().RequestHideAttackTarget(pendingAttackTarget);
            }
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
                ResetAttackNavigationState();
                pendingPortal = null;
                SceneMarkerManager.EnsureExists().HideAttackTargetImmediately();
                SceneMarkerManager.EnsureExists().HideMoveTarget();
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
            SceneMarkerManager.EnsureExists().HideMoveTarget();
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


        private bool TryPickupWorldItemUnderMouse()
        {
            FindReferences();
            if (inputCamera == null)
            {
                return false;
            }

            var mouseWorld = inputCamera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            var clickPoint = new Vector2(mouseWorld.x, mouseWorld.y);
            var hitCount = Physics2D.OverlapPointNonAlloc(clickPoint, clickResults);
            var item = FindClickedPickupItem(hitCount, clickPoint);
            if (item == null)
            {
                return false;
            }

            return item.TryPickup(inventory);
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
                if (item == null || item.IsPickedUp)
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


        private WorldItemPickup FindClickedPickupItem(int hitCount, Vector2 clickPoint)
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
                if (item == null || item.IsPickedUp)
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

        private void TryNavigateToAttackTarget(Vector3 targetWorld, bool force)
        {
            if (autoNavigator == null || (!force && !ShouldRepathToAttackTarget(targetWorld)))
            {
                return;
            }

            if (autoNavigator.TryNavigateToWorldPosition(targetWorld))
            {
                lastAttackNavigationTargetWorld = targetWorld;
                nextAttackNavigationRepathTime = Time.time + Mathf.Max(0.05f, attackNavigationRepathInterval);
            }
        }

        private bool ShouldRepathToAttackTarget(Vector3 targetWorld)
        {
            if (autoNavigator == null)
            {
                return false;
            }

            if (!autoNavigator.IsNavigating)
            {
                return true;
            }

            if (Time.time < nextAttackNavigationRepathTime)
            {
                return false;
            }

            return Vector2.Distance(lastAttackNavigationTargetWorld, targetWorld) >= Mathf.Max(0.05f, attackNavigationRepathDistance);
        }

        private void ResetAttackNavigationState()
        {
            lastAttackNavigationTargetWorld = transform.position;
            nextAttackNavigationRepathTime = 0f;
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

            SubscribeNavigator(autoNavigator);

            if (inventory == null)
            {
                inventory = GetComponent<PlayerInventory>();
            }
        }

        private void SubscribeNavigator(PlayerAutoNavigator navigator)
        {
            if (subscribedNavigator == navigator)
            {
                return;
            }

            UnsubscribeNavigator();
            subscribedNavigator = navigator;
            if (subscribedNavigator == null)
            {
                return;
            }

            subscribedNavigator.NavigationCompleted += OnNavigationFinished;
            subscribedNavigator.NavigationStopped += OnNavigationFinished;
        }

        private void UnsubscribeNavigator()
        {
            if (subscribedNavigator == null)
            {
                return;
            }

            subscribedNavigator.NavigationCompleted -= OnNavigationFinished;
            subscribedNavigator.NavigationStopped -= OnNavigationFinished;
            subscribedNavigator = null;
        }

        private void OnNavigationFinished()
        {
            SceneMarkerManager.EnsureExists().HideMoveTarget();
        }
    }
}
