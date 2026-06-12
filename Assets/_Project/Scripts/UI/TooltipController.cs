using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.UI
{
    [DisallowMultipleComponent]
    public sealed class TooltipController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Vector2 pointerOffset = new(16f, -16f);

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = transform as RectTransform;
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            Hide();
        }

        private void Update()
        {
            if (canvasGroup == null || canvasGroup.alpha <= 0f || rectTransform == null)
            {
                return;
            }

            rectTransform.position = (Vector2)UnityEngine.Input.mousePosition + pointerOffset;
        }

        public void Show(string title, string body)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }

            if (bodyText != null)
            {
                bodyText.text = body;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        public void Hide()
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}
