using IdleonGame.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IdleonGame.UI
{
    [DisallowMultipleComponent]
    public sealed class UIEquipmentSlotView : MonoBehaviour, IDropHandler, IPointerClickHandler
    {
        [SerializeField] private EquipmentSlotType slotType;
        [SerializeField] private Image iconImage;
        [SerializeField] private Text labelText;

        private UIEquipmentWindowController owner;

        public EquipmentSlotType SlotType => slotType;

        public void Initialize(UIEquipmentWindowController window, EquipmentSlotType type)
        {
            owner = window;
            slotType = type;
            if (labelText != null)
            {
                labelText.text = type.ToString().ToUpperInvariant();
            }
        }

        public void SetItem(string itemId)
        {
            var definition = ItemDatabase.Find(itemId);
            if (iconImage != null)
            {
                iconImage.enabled = definition != null && definition.Icon != null;
                iconImage.sprite = definition != null ? definition.Icon : null;
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            var source = UIInventoryDragPayload.SourceSlot;
            if (source == null || source.IsEmpty)
            {
                return;
            }

            owner?.TryEquipFromInventory(source.SlotIndex, slotType);
            UIInventoryDragPayload.Clear();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                owner?.TryUnequip(slotType);
            }
        }
    }
}
