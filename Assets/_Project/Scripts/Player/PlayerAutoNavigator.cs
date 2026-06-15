using System.Collections.Generic;
using IdleonGame.Levels;
using IdleonGame.Navigation;
using IdleonGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IdleonGame.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerAutoNavigator : MonoBehaviour, ILevelSceneReferenceClient
    {
        [SerializeField] private Camera inputCamera;
        [SerializeField] private TilemapNavigationPathfinder pathfinder;
        [SerializeField] private PlayerClimb climb;
        [SerializeField] private bool handleMouseClickInput;
        [SerializeField] private float waypointTolerance = 0.05f;
        [SerializeField] private float horizontalTolerance = 0.03f;
        [SerializeField] private float verticalTolerance = 0.03f;

        private readonly List<TilemapNavigationNode> path = new List<TilemapNavigationNode>();
        private int currentIndex;
        private bool isNavigating;

        public bool IsNavigating => isNavigating;

        private void Awake()
        {
            FindSceneReferences();
        }

        private void Update()
        {
            if (handleMouseClickInput && UnityEngine.Input.GetMouseButtonDown(0))
            {
                if (UIInputBlocker.ShouldBlockScenePointerInput())
                {
                    return;
                }

                TryNavigateToMousePosition();
            }
        }

        public bool TryGetInput(out float horizontal, out float vertical, out bool jumpPressed)
        {
            horizontal = 0f;
            vertical = 0f;
            jumpPressed = false;

            if (!isNavigating || pathfinder == null || path.Count == 0)
            {
                return false;
            }

            AdvanceReachedWaypoints();
            if (!isNavigating)
            {
                return false;
            }

            var target = path[currentIndex];
            var targetWorld = pathfinder.GetNodeWorldPosition(target);
            var delta = targetWorld - transform.position;

            if (target.Kind == NavigationNodeKind.Rope)
            {
                if (Mathf.Abs(delta.x) > horizontalTolerance)
                {
                    horizontal = Mathf.Sign(delta.x);
                    return true;
                }

                if (Mathf.Abs(delta.y) > verticalTolerance)
                {
                    vertical = Mathf.Sign(delta.y);
                }

                return true;
            }

            if (climb != null && climb.IsClimbing)
            {
                if (Mathf.Abs(delta.y) > verticalTolerance)
                {
                    vertical = Mathf.Sign(delta.y);
                    return true;
                }

                climb.StopClimbingForNavigation();
            }

            if (Mathf.Abs(delta.x) > horizontalTolerance)
            {
                horizontal = Mathf.Sign(delta.x);
            }

            return true;
        }

        public bool TryNavigateToWorldPosition(Vector3 targetWorld)
        {
            FindSceneReferences();
            if (pathfinder == null)
            {
                return false;
            }

            if (!pathfinder.TryFindPath(transform.position, targetWorld, path))
            {
                isNavigating = false;
                currentIndex = 0;
                return false;
            }

            currentIndex = path.Count > 1 ? 1 : 0;
            isNavigating = path.Count > 0;
            return isNavigating;
        }

        public void StopNavigation()
        {
            isNavigating = false;
            currentIndex = 0;
            path.Clear();
        }

        public void OnLevelSceneWillUnload(Scene scene)
        {
            if (!scene.IsValid())
            {
                return;
            }

            if (pathfinder != null && pathfinder.gameObject.scene == scene)
            {
                StopNavigation();
                pathfinder = null;
            }
        }

        public void OnLevelSceneLoaded(Scene scene)
        {
            if (!scene.IsValid())
            {
                return;
            }

            pathfinder = null;
            FindSceneReferences();
        }

        private void TryNavigateToMousePosition()
        {
            FindSceneReferences();
            if (inputCamera == null)
            {
                return;
            }

            var mouseWorld = inputCamera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            TryNavigateToWorldPosition(mouseWorld);
        }

        private void AdvanceReachedWaypoints()
        {
            while (currentIndex < path.Count)
            {
                var target = path[currentIndex];
                var targetWorld = pathfinder.GetNodeWorldPosition(target);
                if (!HasReachedWaypoint(target, targetWorld))
                {
                    break;
                }

                currentIndex++;
            }

            if (currentIndex >= path.Count)
            {
                if (climb != null && climb.IsClimbing)
                {
                    climb.StopClimbingForNavigation();
                }

                isNavigating = false;
                currentIndex = 0;
                path.Clear();
            }
        }

        private bool HasReachedWaypoint(TilemapNavigationNode target, Vector3 targetWorld)
        {
            return Mathf.Abs(transform.position.x - targetWorld.x) <= horizontalTolerance
                && Mathf.Abs(transform.position.y - targetWorld.y) <= waypointTolerance;
        }

        private void FindSceneReferences()
        {
            if (inputCamera == null)
            {
                inputCamera = Camera.main;
            }

            if (pathfinder == null)
            {
                pathfinder = LevelSceneReferenceResolver.FindInActiveScene<TilemapNavigationPathfinder>();
            }

            if (climb == null)
            {
                climb = GetComponent<PlayerClimb>();
            }
        }
    }
}
