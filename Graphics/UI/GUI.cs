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
        };
        public static readonly Dictionary<string, int> Textures = new Dictionary<string, int>();

        private static List<IGUIElement>[] Scenes;

        public static void Init()
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

            DirectBitmap db = new DirectBitmap(1, 1);
            db.Clear(Color.FromArgb(255 / 3, 0, 0, 0));
            Textures.Add("BlackTransparent", new Texture(db.Data, db.Width, db.Height, TextureWrapMode.Repeat).id);
            db.Dispose();

            db = new DirectBitmap(1, 1);
            db.Clear(Color.FromArgb(0, 0, 0, 0));
            Textures.Add("Transparent", new Texture(db.Data, db.Width, db.Height, TextureWrapMode.Repeat).id);
            db.Dispose();

            Scenes = new List<IGUIElement>[]
            {
                new List<IGUIElement> {
                    UIImage.CreateCenter(0.06f, 0.06f, Textures["Crosshair"], true),
                    
                }, // In Game
                new List<IGUIElement> { new UIImage(-1f, -1f, 2f, 2f, Textures["BlackTransparent"], false), }, // Pause menu
            };

            float scale = 2.5f;
            int numbSlots = 9;
            slotSize = (Vector2i)(new Vector2(24, 24) * scale);
            iteminslotSize = (Vector2i)(new Vector2(16, 16) * scale);
            iteminslotOffset = new Vector2i(slotSize.X / 2 - iteminslotSize.X / 2, slotSize.Y / 2 - iteminslotSize.Y / 2);
            backSize = new Vector2i(4 + numbSlots * slotSize.X, slotSize.Y + 4);
            backPos = new Vector2i((int)Util.Width / 2 - backSize.X / 2, 20);

            //Scenes[0].Add(UIImage.CreatePixel(backPos, backSize, Textures["BlackTransparent"], 3f));
            UIImage[] icons = new UIImage[numbSlots];
            for (int i = 0; i < numbSlots; i++) {
                Scenes[0].Add(UIImage.CreatePixel(new Vector2i(backPos.X + 2 + i * slotSize.X, backPos.Y + 2), slotSize, Textures["ItemSlotBG"], 0.5f));
                UIImage icon = UIImage.CreatePixel(new Vector2i(backPos.X + 2 + i * slotSize.X + iteminslotOffset.X, backPos.Y + 2 + iteminslotOffset.Y),
                    iteminslotSize, Textures["Transparent"]/*Texture.items[0]*/, 0.6f);
                icons[i] = icon;
                Scenes[0].Add(icon);
            }

            UIImage highlight = UIImage.CreatePixel(new Vector2i(backPos.X, backPos.Y), (Vector2i)(new Vector2(26, 26) * scale), Textures["ToolbarHighlight"]);
            Scenes[0].Add(highlight);

            Toolbar.Init(highlight, icons);
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
