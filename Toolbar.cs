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

        public static void Init(UIImage _highlight, UIItemSlot[] _icons)
        {
            highlight = _highlight;
            slots = new ItemSlot[_icons.Length];
            for (int i = 0; i < slots.Length; i++)
                slots[i] = new ItemSlot(_icons[i]);

            for (uint i = 0; i < slots.Length; i++)
                SetSlot(i, i + 1, 64);

            SetSlot((uint)slots.Length - 1, 17, 64);
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

        private static void SetHighlight(int slot)
        {
            highlight.Position = new Vector3(Util.PixelToGL(GUI.backPos.X + GUI.slotSize.X * slot, GUI.backPos.Y));
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

            SetHighlight(slotIndex);
        }
    }

    public class _ItemSlot
    {
        public uint itemID;
        public UIImage icon;

        public _ItemSlot(uint _itemID, UIImage _icon)
        {
            itemID = _itemID;
            icon = _icon;
        }
    }
}
