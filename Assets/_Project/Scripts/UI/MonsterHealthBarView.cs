using IdleonGame.Monster;
using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.UI
{
    public sealed class MonsterHealthBarView : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private Vector3 worldOffset = new(0f, 1.1f, 0f);

        private RectTransform rectTransform;
        private RectTransform canvasRect;
        private MonsterController target;
        private Camera worldCamera;
        private float expireAt;

        public MonsterController Target => target;
        public bool IsExpired => target == null || target.IsDead || Time.time >= expireAt;

        private void Awake()
        {
            rectTransform = transform as RectTransform;
        }

        public void Initialize(RectTransform ownerCanvasRect)
        {
            canvasRect = ownerCanvasRect;
            rectTransform = transform as RectTransform;
        }

        public void Show(MonsterController monster, Camera camera, float lifetimeSeconds)
        {
            target = monster;
            worldCamera = camera;
            expireAt = Time.time + lifetimeSeconds;
            gameObject.SetActive(true);
            RefreshFill();
            FollowTarget();
        }

        public void RefreshLifetime(float lifetimeSeconds)
        {
            expireAt = Time.time + lifetimeSeconds;
        }

        public void Tick()
        {
            RefreshFill();
            FollowTarget();
        }

        public void Clear()
        {
            target = null;
            gameObject.SetActive(false);
        }

        private void RefreshFill()
        {
            if (fillImage == null || target == null || target.Stats == null)
            {
                return;
            }

            fillImage.fillAmount = target.Stats.MaxHealth > 0
                ? Mathf.Clamp01((float)target.Stats.CurrentHealth / target.Stats.MaxHealth)
                : 0f;
        }

        private void FollowTarget()
        {
            if (target == null || rectTransform == null || canvasRect == null)
            {
                return;
            }

            var cameraToUse = worldCamera != null ? worldCamera : Camera.main;
            if (cameraToUse == null)
            {
                return;
            }

            var screenPosition = cameraToUse.WorldToScreenPoint(target.transform.position + worldOffset);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, null, out var localPoint))
            {
                rectTransform.anchoredPosition = localPoint;
            }
        }
    }
}
