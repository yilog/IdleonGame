using IdleonGame.Items;
using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.UI
{
    public sealed class UIEquipmentWindowController : UIWindowController
    {
        public const string WindowIdConst = "equipment";

        [SerializeField] private Button closeButton;
        [SerializeField] private Transform slotRoot;

        private PlayerInventory inventory;
        private PlayerEquipment equipment;
        private UIEquipmentSlotView[] slotViews;

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
            equipment = FindObjectOfType<PlayerEquipment>();
            CacheSlots();
            Refresh();
        }

        public bool TryEquipFromInventory(int inventorySlotIndex, EquipmentSlotType slotType)
        {
            if (equipment == null)
            {
                equipment = FindObjectOfType<PlayerEquipment>();
            }

            if (inventory == null)
            {
                inventory = FindObjectOfType<PlayerInventory>();
            }

            var success = equipment != null && equipment.TryEquipFromInventory(inventory, inventorySlotIndex, slotType);
            Refresh();
            Manager?.GetWindow<UIInventoryWindowController>(UIInventoryWindowController.WindowIdConst)?.Refresh();
            return success;
        }

        public bool TryUnequip(EquipmentSlotType slotType)
        {
            if (equipment == null)
            {
                equipment = FindObjectOfType<PlayerEquipment>();
            }

            if (inventory == null)
            {
                inventory = FindObjectOfType<PlayerInventory>();
            }

            var success = equipment != null && equipment.TryUnequipToInventory(slotType, inventory);
            Refresh();
            Manager?.GetWindow<UIInventoryWindowController>(UIInventoryWindowController.WindowIdConst)?.Refresh();
            return success;
        }

        public void Refresh()
        {
            if (equipment == null)
            {
                equipment = FindObjectOfType<PlayerEquipment>();
            }

            if (equipment == null || slotViews == null)
            {
                return;
            }

            foreach (var slotView in slotViews)
            {
                slotView.SetItem(equipment.GetEquippedItemId(slotView.SlotType));
            }
        }

        private void CacheSlots()
        {
            if (slotRoot == null)
            {
                slotRoot = transform.Find("Slots");
            }

            slotViews = slotRoot != null ? slotRoot.GetComponentsInChildren<UIEquipmentSlotView>(true) : new UIEquipmentSlotView[0];
            foreach (var slotView in slotViews)
            {
                slotView.Initialize(this, slotView.SlotType);
            }
        }
    }
}
