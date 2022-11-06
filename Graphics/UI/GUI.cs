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
        public static int Scene { get; private set; }

        public static List<IGUIElement> elements;

        private static readonly string texPath = Environment.CurrentDirectory + "/Data/Textures/UI/";
        private static readonly TextureInfo[] texturesToLoad = new TextureInfo[]
        {
            new TextureInfo("Crosshair"),
        };
        public static readonly Dictionary<string, int> Textures = new Dictionary<string, int>();

        private static IGUIElement[][] Scenes;

        public static void Init()
        {
            elements = new List<IGUIElement>();

            DirectBitmap bm;
            BitmapData data;
            for (int i = 0; i < texturesToLoad.Length; i++) {
                if (!File.Exists(texPath + texturesToLoad[i].name + ".png")) {
                    Console.WriteLine($"Block texture {texturesToLoad[i].name}, wasn't found. skipped.");
                    continue;
                }

                bm = DirectBitmap.Load(texPath + texturesToLoad[i].name + ".png", false);//new Bitmap(texPath + texturesToLoad[i].name + ".png");

                /*if (texturesToLoad[i].flip != TexFlip.None) {
                    if ((texturesToLoad[i].flip & TexFlip.Vertical) == TexFlip.Vertical)
                        bm.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    if ((texturesToLoad[i].flip & TexFlip.Horizontal) == TexFlip.Horizontal)
                        bm.RotateFlip(RotateFlipType.RotateNoneFlipY);
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
                }*/

                //data = bm.Bitmap.LockBits(new Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                
                Textures.Add(texturesToLoad[i].name, new Texture(/*data*/bm.Data, bm.Width, bm.Height, TextureWrapMode.Repeat).id);

                //bm.Bitmap.UnlockBits(data);
                bm.Dispose();
            }

            DirectBitmap db = new DirectBitmap(1, 1);
            db.Clear(Color.FromArgb(255 / 3, 0, 0, 0));

            Textures.Add("BlackTransparent", new Texture(db.Data, db.Width, db.Height, TextureWrapMode.Repeat).id);

            db.Dispose();

            Scenes = new IGUIElement[][]
            {
                new IGUIElement[] { UIImage.CreateCenter(0.06f, 0.06f, Textures["Crosshair"], true)/*new UIImage(-0.1f / Program.Window.AspectRatio, -0.1f, 0.2f / Program.Window.AspectRatio, 0.2f, Textures["Crosshair"])*/ }, // In Game
                new IGUIElement[] { new UIImage(-1f, -1f, 2f, 2f, Textures["BlackTransparent"]), }, // Pause menu
            };
        }

        public static void SetScene(int id)
        {
            elements.Clear();
            for (int i = 0; i < Scenes[id].Length; i++)
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
