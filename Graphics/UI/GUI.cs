using Minecraft.Math;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemPlus.Utils;

namespace Minecraft.Graphics.UI
{
    public static class GUI
    {
        public static Vector2i slotSize;
        public static Vector2i iteminslotSize;
        public static Vector2i iteminslotOffset;
        public static Vector2i backSize;
        public static Vector2i backPos;


        public static int Scene { get; private set; }

        public static List<IGUIElement> elements;

        private static readonly string texPath = Environment.CurrentDirectory + "/Data/Textures/UI/";
        private static readonly TextureInfo[] texturesToLoad = new TextureInfo[]
        {
            new TextureInfo("Crosshair"),
            new TextureInfo("ItemSlotBG"),
            new TextureInfo("ToolbarHighlight"),
            new TextureInfo("options_background"),
            new TextureInfo("Button"),
            new TextureInfo("Button_Selected"),
        };
        public static readonly Dictionary<string, int> Textures = new Dictionary<string, int>();

        public static List<IGUIElement>[] Scenes;

        public static void Init(Font font)
        {
            elements = new List<IGUIElement>();

            DirectBitmap bm;
            for (int i = 0; i < texturesToLoad.Length; i++) {
                if (!File.Exists(texPath + texturesToLoad[i].name + ".png")) {
                    Console.WriteLine($"Block texture {texturesToLoad[i].name}, wasn't found. skipped.");
                    continue;
                }

                bm = DirectBitmap.Load(texPath + texturesToLoad[i].name + ".png", false);

                if (texturesToLoad[i].flip != TexFlip.None) {
                    if ((texturesToLoad[i].flip & TexFlip.Vertical) == TexFlip.Vertical)
                        bm.Bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    if ((texturesToLoad[i].flip & TexFlip.Horizontal) == TexFlip.Horizontal)
                        bm.Bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                }

                if (texturesToLoad[i].colorFilter != Vector3.One) {
                    Vector3 filter = texturesToLoad[i].colorFilter;
                    for (int x = 0; x < bm.Width; x++)
                        for (int y = 0; y < bm.Height; y++) {
                            Color c = bm.GetPixel(x, y);
                            float r = (float)(c.R / 255f) * filter.X;
                            float g = (float)(c.G / 255f) * filter.Y;
                            float b = (float)(c.B / 255f) * filter.Z;
                            bm.SetPixel(x, y, Color.FromArgb(255, (byte)(r * 255f), (byte)(g * 255f), (byte)(b * 255f)));
                        }
                }

                Textures.Add(texturesToLoad[i].name, new Texture(bm.Data, bm.Width, bm.Height, TextureWrapMode.Repeat).id);

                bm.Dispose();
            }

            LoadColorTextures();

            Scenes = new List<IGUIElement>[]
            {
                new List<IGUIElement> {
                    UIImage.CreateCenter(0.06f, 0.06f, Textures["Crosshair"], true),
                }, // In Game
                new List<IGUIElement> {
                    new UIImage(-1f, -1f, 2f, 2f, Textures["BlackTransparent"], false),
                    UItext.CreateCenter("Game Menu", 0, 350, 3f, font),
                    new UIButton("Give Feedback", 0f, 0f, 1.8f, font, Textures["Button"]),
                }, // Pause menu
                new List<IGUIElement> { 
                    UIImage.CreateRepeate(-1f, -1f, 2f, 2f, Textures["options_background"], 12, 12),
                    UIImage.CreatePixel(new Vector2i(230, 250), new Vector2i(820, 60), Textures["Black"]), // loading bar
                    UIImage.CreatePixel(new Vector2i(240, 260), new Vector2i(0, 40), Textures["Green"]), // loading bar
                    UItext.CreateCenter("Loading...", 0, 50, 5f, font),
                    UIImage.CreatePixel(new Vector2i(335, 200), new Vector2i(610, 40), Textures["Black"]), // small loading bar
                    UIImage.CreatePixel(new Vector2i(340, 205), new Vector2i(0, 30), Textures["Green"]), // small loading bar
                }, // Loading
                new List<IGUIElement>
                {

                }, // Inventory
            };

            int numbSlots = 9;
            slotSize = (Vector2i)(new Vector2(24, 24) * BlockData.GUIScale);
            iteminslotSize = (Vector2i)(new Vector2(16, 16) * BlockData.GUIScale);
            iteminslotOffset = new Vector2i(slotSize.X / 2 - iteminslotSize.X / 2, slotSize.Y / 2 - iteminslotSize.Y / 2);
            backSize = new Vector2i(4 + numbSlots * slotSize.X, slotSize.Y + 4);
            backPos = new Vector2i((int)Util.Width / 2 - backSize.X / 2, 20);

            UIItemSlot[] slots = new UIItemSlot[numbSlots];
            for (int i = 0; i < numbSlots; i++) {
                slots[i] = new UIItemSlot(new Vector2i(backPos.X + 2 + i * slotSize.X, backPos.Y + 2));
                Scenes[0].Add(slots[i]);
            }
                /*UIImage[] icons = new UIImage[numbSlots];
                for (int i = 0; i < numbSlots; i++) {
                    Scenes[0].Add(UIImage.CreatePixel(new Vector2i(backPos.X + 2 + i * slotSize.X, backPos.Y + 2), slotSize, Textures["ItemSlotBG"], 0.5f));
                    UIImage icon = UIImage.CreatePixel(new Vector2i(backPos.X + 2 + i * slotSize.X + iteminslotOffset.X, backPos.Y + 2 + iteminslotOffset.Y),
                        iteminslotSize, Textures["Transparent"], 0.6f);
                    icons[i] = icon;
                    Scenes[0].Add(icon);
                }*/

            UIImage highlight = UIImage.CreatePixel(new Vector2i(backPos.X, backPos.Y), (Vector2i)(new Vector2(26, 26) 
                * BlockData.GUIScale), Textures["ToolbarHighlight"]);
            Scenes[0].Add(highlight);

            Toolbar.Init(highlight, slots);
        }

        private static void LoadColorTextures()
        {
            DirectBitmap db = new DirectBitmap(1, 1);
            db.Clear(Color.FromArgb(255 / 3, 0, 0, 0));
            Textures.Add("BlackTransparent", new Texture(db.Data, db.Width, db.Height, TextureWrapMode.Repeat).id);
            db.Dispose();

            db = new DirectBitmap(1, 1);
            db.Clear(Color.FromArgb(255, 0, 0, 0));
            Textures.Add("Black", new Texture(db.Data, db.Width, db.Height, TextureWrapMode.Repeat).id);
            db.Dispose();

            db = new DirectBitmap(1, 1);
            db.Clear(Color.FromArgb(0, 0, 0, 0));
            Textures.Add("Transparent", new Texture(db.Data, db.Width, db.Height, TextureWrapMode.Repeat).id);
            db.Dispose();

            db = new DirectBitmap(1, 1);
            db.Clear(Color.FromArgb(255, 0, 255, 0));
            Textures.Add("Green", new Texture(db.Data, db.Width, db.Height, TextureWrapMode.Repeat).id);
            db.Dispose();
        }

        public static void SetScene(int id)
        {
            elements.Clear();
            for (int i = 0; i < Scenes[id].Count; i++)
                elements.Add(Scenes[id][i]);
            Scene = id;
        }

        public static void Update(float delta)
        {
            for (int i = 0; i < elements.Count; i++) {
                elements[i].Update(delta);
            }
        }

        public static void Render(Shader uiShader)
        {
            if (Scene == 2) {
                (elements[2] as UIImage).UpdateVerts();
                (elements[5] as UIImage)?.UpdateVerts();
            }
            uiShader.Bind();
            for (int i = 0; i < elements.Count; i++) {
                elements[i].Render(uiShader);
            }
        }

        public static void OnKeyDown(Key key, KeyModifiers modifiers)
        {
            for (int i = 0; i < elements.Count; i++) {
                elements[i].OnKeyDown(key, modifiers);
            }
        }
        public static void OnKeyUp(Key key, KeyModifiers modifiers)
        {
            for (int i = 0; i < elements.Count; i++) {
                elements[i].OnKeyUp(key, modifiers);
            }
        }
        public static void OnMouseDown(MouseButton button, Point pos)
        {
            float x = (float)pos.X / Program.Window.Width;
            float y = (float)pos.Y / Program.Window.Height;
            for (int i = 0; i < elements.Count; i++) {
                elements[i].OnMouseDown(button,
                    x >= elements[i].Position.X && x < elements[i].Width + elements[i].Position.X &&
                    y >= elements[i].Position.Y && y < elements[i].Height + elements[i].Position.Y);
            }
        }
        public static void OnMouseUp(MouseButton button, Point pos)
        {
            float x = (float)pos.X / Program.Window.Width;
            float y = (float)pos.Y / Program.Window.Height;
            for (int i = 0; i < elements.Count; i++) {
                elements[i].OnMouseUp(button,
                    x >= elements[i].Position.X && x < elements[i].Width + elements[i].Position.X &&
                    y >= elements[i].Position.Y && y < elements[i].Height + elements[i].Position.Y);
            }
        }
    }
}
