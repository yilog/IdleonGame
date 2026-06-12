using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.UI
{
    public sealed class TestCloseWindowController : UIWindowController
    {
        [SerializeField] private Button closeButton;

        protected override void OnOpen(object args)
        {
            EnsureDefaultLayout();
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
                closeButton.onClick.AddListener(Close);
            }
        }

        protected override void OnClose()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
            }
        }

        private void EnsureDefaultLayout()
        {
            if (closeButton != null)
            {
                return;
            }

            var rectTransform = transform as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(420f, 240f);
            }

            var panelImage = gameObject.GetComponent<Image>();
            if (panelImage == null)
            {
                panelImage = gameObject.AddComponent<Image>();
            }

            panelImage.color = new Color32(24, 28, 36, 230);
            panelImage.raycastTarget = true;

            var title = CreateText("Title", transform, "Test Window", 22, TextAnchor.MiddleCenter);
            var titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -28f);
            titleRect.sizeDelta = new Vector2(-32f, 36f);

            var buttonObject = new GameObject("CloseButton", typeof(RectTransform));
            buttonObject.transform.SetParent(transform, false);
            closeButton = buttonObject.AddComponent<Button>();
            var buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color32(78, 132, 210, 255);

            var buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(140f, 44f);
            buttonRect.anchoredPosition = Vector2.zero;

            var label = CreateText("Label", buttonObject.transform, "Close", 18, TextAnchor.MiddleCenter);
            var labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
        }

        private static Text CreateText(string name, Transform parent, string text, int fontSize, TextAnchor alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            var label = textObject.AddComponent<Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;
            return label;
        }
    }
}
