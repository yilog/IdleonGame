namespace IdleonGame.UI
{
    public static class UIInventoryDragPayload
    {
        public static UIInventorySlotView SourceSlot { get; private set; }

        public static void Begin(UIInventorySlotView source)
        {
            SourceSlot = source;
        }

        public static void Clear(UIInventorySlotView source)
        {
            if (SourceSlot == source)
            {
                SourceSlot = null;
            }
        }

        public static void Clear()
        {
            SourceSlot = null;
        }
    }
}
