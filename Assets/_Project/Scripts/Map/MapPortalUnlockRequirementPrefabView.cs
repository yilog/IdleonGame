using UnityEngine;

namespace IdleonGame.Map
{
    public sealed class MapPortalUnlockRequirementPrefabView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer skullIcon;
        [SerializeField] private TextMesh countText;

        private void Awake()
        {
            if (skullIcon == null)
            {
                skullIcon = GetComponentInChildren<SpriteRenderer>(true);
            }

            if (countText == null)
            {
                countText = GetComponentInChildren<TextMesh>(true);
            }
        }

        public void SetRemainingCount(int remainingCount)
        {
            if (countText != null)
            {
                countText.text = Mathf.Max(0, remainingCount).ToString();
            }
        }

        public void SetSortingOrder(int sortingOrder)
        {
            if (skullIcon != null)
            {
                skullIcon.sortingOrder = sortingOrder;
            }

            if (countText != null)
            {
                var meshRenderer = countText.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.sortingOrder = sortingOrder + 1;
                }
            }
        }
    }
}
