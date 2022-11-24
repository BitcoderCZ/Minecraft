using Minecraft.Graphics.UI;

namespace Minecraft
{
    public class ItemSlot
    {
        public ItemStack stack;
        public UIItemSlot uIItemSlot;

        public ItemSlot(UIItemSlot _slot)
        {
            uIItemSlot = _slot;
            uIItemSlot.Link(this);
        }

        public void LinkUI(UIItemSlot _slot)
            => uIItemSlot = _slot;

        public void UnlinkUI()
            => uIItemSlot = null;

        public int Take(byte amount)
        {
            if (amount >= stack.amount) {
                int amt = stack.amount;
                EmptySlot();
                if (Linked)
                    uIItemSlot.UpdateSlot();
                return amt;
            }
            else {
                stack.amount -= amount;
                if (Linked)
                    uIItemSlot.UpdateSlot();
                return amount;
            }
        }

        public void EmptySlot()
        {
            stack = null;
            if (Linked)
                uIItemSlot.UpdateSlot();
        }

        public bool HasItem
        {
            get => stack != null;
        }

        public bool Linked
        {
            get => uIItemSlot != null;
        }

        public uint ItemId
        {
            get => HasItem ? stack.id : 0;
        }
    }
}