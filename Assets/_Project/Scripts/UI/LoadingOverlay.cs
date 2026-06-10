using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.UI
{
    [DisallowMultipleComponent]
    public sealed class LoadingOverlay : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image blocker;
        [SerializeField] private Color blockerColor = Color.black;

        private void Awake()
        {
            EnsureOverlay();
            Hide();
        }

        public void Show()
        {
            EnsureOverlay();
            canvas.enabled = true;
        }

        public void Hide()
        {
            EnsureOverlay();
            canvas.enabled = false;
        }

        private void EnsureOverlay()
        {
            if (canvas == null)
            {
                canvas = GetComponentInChildren<Canvas>();
            }

            if (canvas == null)
            {
                var canvasObject = new GameObject("LoadingOverlayCanvas");
                canvasObject.transform.SetParent(transform, false);
                canvas = canvasObject.AddComponent<Canvas>();
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;

            if (blocker == null)
            {
                blocker = canvas.GetComponentInChildren<Image>();
            }

            if (blocker == null)
            {
                var blockerObject = new GameObject("Blocker");
                blockerObject.transform.SetParent(canvas.transform, false);
                blocker = blockerObject.AddComponent<Image>();
            }

            blocker.color = blockerColor;
            blocker.raycastTarget = true;

            var rectTransform = blocker.rectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
