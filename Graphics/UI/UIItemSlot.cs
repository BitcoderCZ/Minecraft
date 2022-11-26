using Minecraft.Math;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.Graphics.UI
{
    public class UIItemSlot : GUIElement
    {
        public bool isLinked = false;
        public ItemSlot slot;
        public UIImage slotImage;
        public UIImage slotIcon;
        public UItext slotAmount;

        public bool HasItem
        {
            get => slot == null ? false : slot.HasItem;
        }

        public UIItemSlot(Vector2i pos)
        {
            PixX = pos.X;
            PixY = pos.Y;
            PixWidth = (int)(24f * BlockData.GUIScale);
            PixHeight = (int)(24f * BlockData.GUIScale);

            Vector2i slotSize = (Vector2i)(new Vector2(24, 24) * BlockData.GUIScale);
            Vector2i iteminslotSize = (Vector2i)(new Vector2(16, 16) * BlockData.GUIScale);
            Vector2i iteminslotOffset = new Vector2i(slotSize.X / 2 - iteminslotSize.X / 2, slotSize.Y / 2 - iteminslotSize.Y / 2);

            slotImage = new UIImage(Vector2.Zero, Vector2.Zero, GUI.Textures["ItemSlotBG"], false);
            slotImage.PixX = pos.X;
            slotImage.PixY = pos.Y;
            slotImage.PixWidth = slotSize.X;
            slotImage.PixHeight = slotSize.Y;
            slotImage.UpdateVerts();

            slotIcon = new UIImage(Vector2.Zero, Vector2.Zero, -1, false);
            slotIcon.PixX = pos.X + iteminslotOffset.X;
            slotIcon.PixY = pos.Y + iteminslotOffset.Y;
            slotIcon.PixWidth = iteminslotSize.X;
            slotIcon.PixHeight = iteminslotSize.Y;
            slotIcon.UpdateVerts();

            slotAmount = new UItext("", 0f, 0f, BlockData.GUIScale / 2f, Program.Window.font);
            slotAmount.PixX = pos.X + iteminslotOffset.X;
            slotAmount.PixY = pos.Y + iteminslotOffset.Y;
        }

        public void Link(ItemSlot _slot)
        {
            slot = _slot;
            isLinked = true;
            slot.LinkUI(this);
        }

        public void UnLink()
        {
            slot.UnlinkUI();
            slot = null;
        }

        public void Move()
        {
            Vector2i slotSize = (Vector2i)(new Vector2(24, 24) * BlockData.GUIScale);
            Vector2i iteminslotSize = (Vector2i)(new Vector2(16, 16) * BlockData.GUIScale);
            Vector2i iteminslotOffset = new Vector2i(slotSize.X / 2 - iteminslotSize.X / 2, slotSize.Y / 2 - iteminslotSize.Y / 2);

            if (slotImage != null)
                slotImage.Position = Position;
            slotIcon.PixX = PixX + iteminslotOffset.X;
            slotIcon.PixY = PixY + iteminslotOffset.Y;
            slotAmount.PixX = PixX + iteminslotOffset.X;
            slotAmount.PixY = PixY + iteminslotOffset.Y;
            slotAmount.SetPos();
        }

        public void UpdateSlot()
        {
            if (HasItem) {
                slotIcon.textureID = Texture.items[slot.stack.id];
                slotAmount.Text = slot.stack.amount.ToString();
                slotIcon.Active = true;
                slotAmount.Active = true;
                slotAmount.UpdateMesh();
            }
            else
                Clear();
        }

        private void Clear()
        {
            slotIcon.textureID = -1;
            slotAmount.Text = string.Empty;
            slotIcon.Active = false;
            slotAmount.Active = false;
        }

        public override void Render(Shader s)
        {
            if (slotImage != null)
                slotImage.Render(s);
            slotIcon.Render(s);
            slotAmount.Render(s);
        }
    }
}