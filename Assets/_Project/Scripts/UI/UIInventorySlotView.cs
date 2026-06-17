using IdleonGame.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IdleonGame.UI
{
    [DisallowMultipleComponent]
    public sealed class UIInventorySlotView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Text countText;
        [SerializeField] private Image dragGhostImage;

        private UIInventoryWindowController owner;
        private int slotIndex;
        private string itemId;
        private int count;

        private const int DragGhostSortingOrder = 10000;

        public int SlotIndex => slotIndex;
        public string ItemId => itemId;
        public bool IsEmpty => string.IsNullOrEmpty(itemId) || count <= 0;

        public void Initialize(UIInventoryWindowController window, int index, Image sharedDragGhostImage)
        {
            owner = window;
            slotIndex = index;
            dragGhostImage = sharedDragGhostImage;
            EnsureDragGhostSorting();
        }

        public void SetItem(string newItemId, int newCount)
        {
            itemId = newItemId;
            count = Mathf.Max(0, newCount);
            var definition = ItemDatabase.Find(itemId);
            if (iconImage != null)
            {
                iconImage.enabled = definition != null && definition.Icon != null;
                iconImage.sprite = definition != null ? definition.Icon : null;
            }

            if (countText != null)
            {
                countText.text = count > 1 ? count.ToString() : string.Empty;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (IsEmpty)
            {
                return;
            }

            UIInventoryDragPayload.Begin(this);
            if (dragGhostImage != null)
            {
                EnsureDragGhostSorting();
                dragGhostImage.sprite = iconImage != null ? iconImage.sprite : null;
                dragGhostImage.enabled = dragGhostImage.sprite != null;
                dragGhostImage.transform.position = eventData.position;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragGhostImage != null && dragGhostImage.enabled)
            {
                dragGhostImage.transform.position = eventData.position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragGhostImage != null)
            {
                dragGhostImage.enabled = false;
            }

            UIInventoryDragPayload.Clear(this);
        }
        private void EnsureDragGhostSorting()
        {
            if (dragGhostImage == null)
            {
                return;
            }

            dragGhostImage.raycastTarget = false;
            var dragCanvas = dragGhostImage.GetComponent<Canvas>();
            if (dragCanvas == null)
            {
                dragCanvas = dragGhostImage.gameObject.AddComponent<Canvas>();
            }

            dragCanvas.overrideSorting = true;
            dragCanvas.sortingOrder = DragGhostSortingOrder;
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                owner?.UseSlot(slotIndex);
            }
        }
    }
}
