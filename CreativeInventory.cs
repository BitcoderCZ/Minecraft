using Minecraft.Graphics;
using Minecraft.Graphics.UI;
using Minecraft.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public static class CreativeInventory
    {
        private static UIItemSlot[] slots;

        public static void Init(ref List<IGUIElement> elements, Vector2i backPos, Vector2i slotSize, int numbSlots)
        {
            int margin = (int)(6f * World.Settings.GUIScale);

            Vector2i startSize = new Vector2i(slotSize.X * numbSlots, slotSize.Y * 5);
            Vector2i invBackSize = startSize + margin * 2;
            Vector2i startPos = new Vector2i(backPos.X, /*backPos.Y + slotSize.Y * 2*/Program.Window.Height / 2 - startSize.Y / 2);
            Vector2i invBackPos = startPos - margin;
            elements.Add(UIImage.CreatePixel(invBackPos, invBackSize, GUI.Textures["BlackTransparent"]));

            int x = 0;
            int y = 0;

            slots = new UIItemSlot[Texture.items.Length - 1];

            for (int i = 1; i < Texture.items.Length; i++) {
                UIItemSlot slot = new UIItemSlot(new Vector2i(startPos.X + x * slotSize.X, startPos.Y + startSize.Y - y * slotSize.Y - slotSize.Y));
                ItemSlot iSlot = new ItemSlot(slot);
                iSlot.isCreativeSlot = true;
                slot.Link(iSlot);
                slot.slot.SetStack(new ItemStack((uint)i, (byte)64));
                slot.UpdateSlot();
                elements.Add(slot);
                slots[i - 1] = slot;

                x++;
                if (x >= numbSlots) {
                    x = 0;
                    y++;
                }
            }
        }
    }
}
