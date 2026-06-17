using IdleonGame.Character;
using IdleonGame.Monster;
using UnityEngine;

namespace IdleonGame.Markers
{
    [DisallowMultipleComponent]
    public sealed class SceneMarkerManager : MonoBehaviour
    {
        private const string AttackTargetMarkerPath = "Prefabs/SceneMarkers/AttackTargetMarker";
        private const string MoveTargetMarkerPath = "Prefabs/SceneMarkers/MoveTargetMarker";
        private const float AttackMarkerMinimumVisibleSeconds = 1f;

        private static SceneMarkerManager instance;

        [SerializeField] private SceneMarkerView attackTargetMarkerPrefab;
        [SerializeField] private SceneMarkerView moveTargetMarkerPrefab;

        private SceneMarkerView attackTargetMarker;
        private SceneMarkerView moveTargetMarker;
        private Damageable markedAttackTarget;
        private float attackMarkerShownAt;
        private bool attackMarkerHideRequested;

        public static SceneMarkerManager Instance => instance;

        public static SceneMarkerManager EnsureExists()
        {
            if (instance != null)
            {
                return instance;
            }

            var existing = FindObjectOfType<SceneMarkerManager>();
            if (existing != null)
            {
                instance = existing;
                return instance;
            }

            var managerObject = new GameObject("SceneMarkerManager");
            return managerObject.AddComponent<SceneMarkerManager>();
        }

        public void ShowAttackTarget(Damageable target)
        {
            if (target == null || target.IsDead)
            {
                HideAttackTargetImmediately();
                return;
            }

            var targetTransform = (target as Component)?.transform;
            if (targetTransform == null)
            {
                HideAttackTargetImmediately();
                return;
            }

            LoadPrefabs();
            if (attackTargetMarker == null && attackTargetMarkerPrefab != null)
            {
                attackTargetMarker = Instantiate(attackTargetMarkerPrefab, transform);
            }

            if (attackTargetMarker == null)
            {
                return;
            }

            markedAttackTarget = target;
            attackMarkerShownAt = Time.time;
            attackMarkerHideRequested = false;
            attackTargetMarker.Follow(targetTransform);
        }

        public void RequestHideAttackTarget(Damageable target)
        {
            if (target != null && markedAttackTarget != null && !ReferenceEquals(target, markedAttackTarget))
            {
                return;
            }

            attackMarkerHideRequested = true;
            TryHideRequestedAttackMarker();
        }

        public void HideAttackTargetImmediately()
        {
            markedAttackTarget = null;
            attackMarkerHideRequested = false;
            attackTargetMarker?.Hide();
        }

        public void ShowMoveTarget(Vector3 worldPosition)
        {
            LoadPrefabs();
            if (moveTargetMarker == null && moveTargetMarkerPrefab != null)
            {
                moveTargetMarker = Instantiate(moveTargetMarkerPrefab, transform);
            }

            moveTargetMarker?.ShowAt(worldPosition);
        }

        public void HideMoveTarget()
        {
            moveTargetMarker?.Hide();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            LoadPrefabs();
        }

        private void Update()
        {
            if (markedAttackTarget != null && markedAttackTarget.IsDead)
            {
                HideAttackTargetImmediately();
                return;
            }

            if (markedAttackTarget != null)
            {
                var targetComponent = markedAttackTarget as Component;
                if (targetComponent == null)
                {
                    HideAttackTargetImmediately();
                    return;
                }
            }

            TryHideRequestedAttackMarker();
        }

        private void TryHideRequestedAttackMarker()
        {
            if (!attackMarkerHideRequested)
            {
                return;
            }

            if (Time.time - attackMarkerShownAt < AttackMarkerMinimumVisibleSeconds)
            {
                return;
            }

            HideAttackTargetImmediately();
        }

        private void LoadPrefabs()
        {
            if (attackTargetMarkerPrefab == null)
            {
                attackTargetMarkerPrefab = Resources.Load<SceneMarkerView>(AttackTargetMarkerPath);
            }

            if (moveTargetMarkerPrefab == null)
            {
                moveTargetMarkerPrefab = Resources.Load<SceneMarkerView>(MoveTargetMarkerPath);
            }
        }
    }
}
