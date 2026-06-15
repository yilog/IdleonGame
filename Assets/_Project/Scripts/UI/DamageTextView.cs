using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.UI
{
    public sealed class DamageTextView : MonoBehaviour
    {
        [SerializeField] private Text damageText;
        [SerializeField] private float lifetimeSeconds = 0.8f;
        [SerializeField] private float floatDistance = 42f;

        private RectTransform rectTransform;
        private Vector2 startPosition;
        private float startTime;

        public bool IsExpired => Time.time >= startTime + lifetimeSeconds;

        private void Awake()
        {
            rectTransform = transform as RectTransform;
        }

        public void Show(int damage, Vector2 anchoredPosition)
        {
            rectTransform = transform as RectTransform;
            startPosition = anchoredPosition;
            startTime = Time.time;
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = anchoredPosition;
            }

            if (damageText != null)
            {
                damageText.text = $"-{damage}";
                damageText.color = Color.white;
            }

            gameObject.SetActive(true);
        }

        public void Tick()
        {
            var t = Mathf.Clamp01((Time.time - startTime) / lifetimeSeconds);
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = startPosition + Vector2.up * (floatDistance * t);
            }

            if (damageText != null)
            {
                var color = damageText.color;
                color.a = 1f - t;
                damageText.color = color;
            }
        }

        public void Clear()
        {
            gameObject.SetActive(false);
        }
    }
}
