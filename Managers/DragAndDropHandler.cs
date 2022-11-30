using Minecraft.Graphics.UI;
using Minecraft.Math;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.Managers
{
    public static class DragAndDropHandler
    {
        public static UIItemSlot cursorSlot;
        private static ItemSlot cursorItemSlot;

        static DragAndDropHandler()
        {
            cursorSlot = new UIItemSlot(new Vector2i(200, 200));
            cursorSlot.slotImage = null;
            cursorSlot.UpdateSlot();
            cursorItemSlot = new ItemSlot(cursorSlot);
        }

        public static void Update()
        {
            if (GUI.Scene != 3)
                return;

            float x = (Program.Window.mousePos.X / Program.Window.Width * 2f) - 1f;
            float y = ((Program.Window.mousePos.Y / Program.Window.Height * 2f) - 1f) * -1f;

            cursorSlot.Position = new Vector3(x, y, cursorSlot.Position.Z);
            cursorSlot.Move();
        }

        public static void OnKeyDown(Key key)
        {
            if (GUI.Scene != 3)
                return;

            if (key == Key.Q && cursorSlot.HasItem)
                cursorSlot.slot.EmptySlot();
        }

        public static void OnMouseDown(MouseButton btns, Vector2i pos)
        {
            if (GUI.Scene != 3)
                return;

            if (btns == MouseButton.Left)
                HandleSlotClick(CheckForSlot(pos));
            else if (btns == MouseButton.Right)
                HandleSlotRigthClick(CheckForSlot(pos));
            else if (btns == MouseButton.Middle)
                HandleSlotMiddleClick(CheckForSlot(pos));
        }

        private static void HandleSlotMiddleClick(UIItemSlot clickedSlot)
        {
            if (clickedSlot == null)
                return;

            if (!cursorSlot.slot.HasItem && !clickedSlot.HasItem)
                return;
            else if (!cursorSlot.slot.HasItem && clickedSlot.slot.HasItem)
                cursorSlot.slot.SetStack(new ItemStack(clickedSlot.slot.ItemId, 64));
        }

        private static void HandleSlotRigthClick(UIItemSlot clickedSlot)
        {
            if (clickedSlot == null)
                return;

            if (!cursorSlot.slot.HasItem && !clickedSlot.HasItem)
                return;
            else if (!cursorSlot.HasItem && clickedSlot.HasItem) {
                if (clickedSlot.slot.Amount % 2 == 0) {
                    cursorSlot.slot.Add(clickedSlot.slot.ItemId, clickedSlot.slot.Amount / 2);
                    clickedSlot.slot.Take((byte)(clickedSlot.slot.Amount / 2));
                } else {
                    cursorSlot.slot.Add(clickedSlot.slot.ItemId, clickedSlot.slot.Amount / 2 + 1);
                    clickedSlot.slot.Take((byte)(clickedSlot.slot.Amount / 2 + 1));
                }
            }
            else if (cursorSlot.HasItem) {
                clickedSlot.slot.Add(cursorSlot.slot.ItemId, 1);
                cursorSlot.slot.Take(1);
            }
        }

        private static void HandleSlotClick(UIItemSlot clickedSlot)
        {
            if (clickedSlot == null)
                return;

            if (!cursorSlot.slot.HasItem && !clickedSlot.HasItem)
                return;
            else if (!cursorSlot.HasItem && clickedSlot.HasItem) {
                cursorSlot.slot.stack = clickedSlot.slot.TakeAll();
                cursorSlot.UpdateSlot();
            }
            else if (cursorSlot.HasItem && !clickedSlot.HasItem) {
                clickedSlot.slot.stack = cursorSlot.slot.TakeAll();
                clickedSlot.UpdateSlot();
            } else if (cursorSlot.HasItem && clickedSlot.HasItem) {
                if (clickedSlot.slot.isCreativeSlot) {
                    cursorSlot.slot.EmptySlot();
                }
                else {
                    if (cursorSlot.slot.ItemId == clickedSlot.slot.ItemId) {
                        if (clickedSlot.slot.Amount < 64) {
                            if (clickedSlot.slot.Amount + cursorSlot.slot.Amount <= 64) {
                                clickedSlot.slot.Add(cursorSlot.slot.TakeAll());
                            }
                            else {
                                clickedSlot.slot.Add(cursorSlot.slot.ItemId, cursorSlot.slot.Take(64 - clickedSlot.slot.Amount));
                            }
                        }
                    }
                    else {
                        ItemStack stack = clickedSlot.slot.TakeAll();
                        clickedSlot.slot.SetStack(cursorSlot.slot.TakeAll());
                        cursorSlot.slot.SetStack(stack);
                    }
                }
            }
        }

        private static UIItemSlot CheckForSlot(Vector2i pos)
        {
            if (GUI.Scene != 3)
                return null;

            List<UIItemSlot> slots = GUI.GetUnderPoint(pos).Where((IGUIElement element) => element is UIItemSlot)
                .Select((IGUIElement element) => element as UIItemSlot).ToList();

            if (slots.Count == 0)
                return null;
            else
                return slots[0];
        }
    }
}
