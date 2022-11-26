using Minecraft.Graphics;
using Minecraft.Graphics.UI;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public static class Toolbar
    {
        static UIImage highlight;

        public static ItemSlot[] slots;

        public static int slotIndex;

        public static bool AllOcupied
        {
            get {
                for (int i = 0; i < slots.Length; i++)
                    if (!slots[i].HasItem)
                        return false;
                return true;
            }
        }

        public static void Init(UIImage _highlight, UIItemSlot[] _icons)
        {
            highlight = _highlight;
            slots = new ItemSlot[_icons.Length];
            for (int i = 0; i < slots.Length; i++)
                slots[i] = new ItemSlot(_icons[i]);

            for (uint i = 0; i < slots.Length; i++)
                SetSlot(i, 0, 0);
        }

        private static void SetSlot(uint slot, uint item, byte amount)
        {
            if (item == 0)
                slots[slot].EmptySlot();
            else if (!slots[slot].HasItem) {
                slots[slot].stack = new ItemStack(item, amount);
                slots[slot].uIItemSlot.UpdateSlot();
            }
            else {
                slots[slot].stack.id = item;
                slots[slot].stack.amount = amount;
                slots[slot].uIItemSlot.UpdateSlot();
            }
        }

        public static void SetHighlight()
        {
            highlight.Position = new Vector3(Util.PixelToGL(GUI.backPos.X + GUI.slotSize.X * slotIndex, GUI.backPos.Y));
        }

        public static void MouseScrool(int scrool)
        {
            if (scrool > 0)
                slotIndex--;
            else
                slotIndex++;

            if (slotIndex >= slots.Length)
                slotIndex = 0;
            else if (slotIndex < 0)
                slotIndex = slots.Length - 1;

            SetHighlight();
        }

        public static bool ContainsItem(uint itemId, out int index)
        {
            index = -1;
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].ItemId == itemId) {
                    index = i;
                    return true;
                }

            return false;
        }
    }
}
