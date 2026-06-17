using IdleonGame.Items;
using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.UI
{
    public sealed class UIInventoryWindowController : UIWindowController
    {
        public const string WindowIdConst = "inventory";

        [SerializeField] private Button closeButton;
        [SerializeField] private Transform slotRoot;
        [SerializeField] private Image dragGhostImage;

        private PlayerInventory inventory;
        private UIInventorySlotView[] slotViews;

        private void Update()
        {
            Refresh();
        }

        protected override void OnOpen(object args)
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
                closeButton.onClick.AddListener(Close);
            }

            inventory = FindObjectOfType<PlayerInventory>();
            CacheSlots();
            Refresh();
        }

        public void UseSlot(int slotIndex)
        {
            inventory?.UseItem(slotIndex);
            Refresh();
        }

        public void Refresh()
        {
            if (inventory == null)
            {
                inventory = FindObjectOfType<PlayerInventory>();
            }

            if (inventory == null || slotViews == null)
            {
                return;
            }

            var slots = inventory.Slots;
            for (var i = 0; i < slotViews.Length; i++)
            {
                var slot = i < slots.Length ? slots[i] : null;
                if (slot == null || slot.IsEmpty)
                {
                    slotViews[i].SetItem(string.Empty, 0);
                    continue;
                }

                slotViews[i].SetItem(slot.itemId, slot.count);
            }
        }

        private void CacheSlots()
        {
            if (slotRoot == null)
            {
                slotRoot = transform.Find("Slots");
            }

            slotViews = slotRoot != null ? slotRoot.GetComponentsInChildren<UIInventorySlotView>(true) : new UIInventorySlotView[0];
            for (var i = 0; i < slotViews.Length; i++)
            {
                slotViews[i].Initialize(this, i, dragGhostImage);
            }
        }
    }
}
