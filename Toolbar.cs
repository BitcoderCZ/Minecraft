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

        static ItemSlot[] slots;

        static int slotIndex;

        public static void Init(UIImage _highlight, UIImage[] _icons)
        {
            highlight = _highlight;
            slots = new ItemSlot[_icons.Length];
            for (int i = 0; i < slots.Length; i++)
                slots[i] = new ItemSlot(0, _icons[i]);

            for (uint i = 0; i < slots.Length; i++)
                SetSlot(i, i + 1);

            SetSlot((uint)slots.Length - 1, 17);

            Player.selectedItem = slots[slotIndex].itemID;
        }

        private static void SetSlot(uint slot, uint item)
        {
            slots[slot].itemID = item;
            if (item == 0)
                slots[slot].icon.textureID = GUI.Textures["Transparent"];
            else
                slots[slot].icon.textureID = Texture.items[item];
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

            Player.selectedItem = slots[slotIndex].itemID;
        }
    }

    public struct ItemSlot
    {
        public uint itemID;
        public UIImage icon;

        public ItemSlot(uint _itemID, UIImage _icon)
        {
            itemID = _itemID;
            icon = _icon;
        }
    }
}
