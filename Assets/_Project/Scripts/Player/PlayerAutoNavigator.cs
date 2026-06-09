using System.Collections.Generic;
using IdleonGame.Navigation;
using UnityEngine;

namespace IdleonGame.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerAutoNavigator : MonoBehaviour
    {
        [SerializeField] private Camera inputCamera;
        [SerializeField] private TilemapNavigationPathfinder pathfinder;
        [SerializeField] private float waypointTolerance = 0.12f;
        [SerializeField] private float horizontalTolerance = 0.08f;
        [SerializeField] private float verticalTolerance = 0.08f;

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
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
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

            if (Mathf.Abs(delta.x) > horizontalTolerance)
            {
                horizontal = Mathf.Sign(delta.x);
            }

            return true;
        }

        private void TryNavigateToMousePosition()
        {
            FindSceneReferences();
            if (inputCamera == null || pathfinder == null)
            {
                return;
            }

            var mouseWorld = inputCamera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            if (!pathfinder.TryFindPath(transform.position, mouseWorld, path))
            {
                isNavigating = false;
                currentIndex = 0;
                return;
            }

            currentIndex = path.Count > 1 ? 1 : 0;
            isNavigating = path.Count > 0;
        }

        private void AdvanceReachedWaypoints()
        {
            while (currentIndex < path.Count)
            {
                var target = path[currentIndex];
                var targetWorld = pathfinder.GetNodeWorldPosition(target);
                if (Vector2.Distance(transform.position, targetWorld) > waypointTolerance)
                {
                    break;
                }

                currentIndex++;
            }

            if (currentIndex >= path.Count)
            {
                isNavigating = false;
                currentIndex = 0;
                path.Clear();
            }
        }

        private void FindSceneReferences()
        {
            if (inputCamera == null)
            {
                inputCamera = Camera.main;
            }

            if (pathfinder == null)
            {
                pathfinder = Object.FindObjectOfType<TilemapNavigationPathfinder>();
            }
        }
    }
}
