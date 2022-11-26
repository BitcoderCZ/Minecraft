using Minecraft.Graphics.UI;

namespace Minecraft
{
    public class ItemSlot
    {
        public ItemStack stack;
        public UIItemSlot uIItemSlot;
        public bool isCreativeSlot;

        public ItemSlot(UIItemSlot _slot)
        {
            isCreativeSlot = false;
            uIItemSlot = _slot;
            uIItemSlot.Link(this);
        }

        public void LinkUI(UIItemSlot _slot)
            => uIItemSlot = _slot;

        public void UnlinkUI()
            => uIItemSlot = null;

        public int Take(int amount)
        {
            if (amount >= stack.amount) {
                int amt = stack.amount;
                EmptySlot();
                if (Linked)
                    uIItemSlot.UpdateSlot();
                return amt;
            }
            else {
                if (!isCreativeSlot)
                    stack.amount -= (byte)amount;
                if (Linked)
                    uIItemSlot.UpdateSlot();
                return amount;
            }
        }

        public ItemStack TakeAll()
        {
            ItemStack _stack = new ItemStack(stack.id, stack.amount);
            EmptySlot();
            return _stack;
        }

        public void SetStack(ItemStack _stack)
        {
            stack = new ItemStack(_stack.id, _stack.amount);
            if (Linked)
                uIItemSlot.UpdateSlot();
        }

        public void Add(uint id, int amount)
            => Add(new ItemStack(id, (byte)amount));
        public void Add(ItemStack _stack)
        {
            if (HasItem) {
                stack.amount += _stack.amount;
                if (stack.amount > 64)
                    stack.amount = 64;
            }
            else {
                byte am = _stack.amount;
                if (am > 64)
                    am = 64;
                stack = new ItemStack(_stack.id, am);
            }
            if (Linked)
                uIItemSlot.UpdateSlot();
        }

        public void EmptySlot()
        {
            if (!isCreativeSlot)
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

        public byte Amount
        {
            get => HasItem ? stack.amount : (byte)0;
        }
    }
}