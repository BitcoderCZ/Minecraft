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
using SystemPlus;
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
            new TextureInfo("Button", TexFlip.Horizontal),
            new TextureInfo("Button_Selected", TexFlip.Horizontal),
            new TextureInfo("InputField", TexFlip.Horizontal),
            new TextureInfo("Logo"), // 271 x 44
            new TextureInfo("TextBox"), // 271 x 44
        };
        public static readonly Dictionary<string, int> Textures = new Dictionary<string, int>();

        public static List<IGUIElement>[] Scenes;

        public static void Init(Font font)
        {
            elements = new List<IGUIElement>();

            Bitmap bm;
            for (int i = 0; i < texturesToLoad.Length; i++) {
                if (!File.Exists(texPath + texturesToLoad[i].name + ".png")) {
                    Console.WriteLine($"Block texture {texturesToLoad[i].name}, wasn't found. skipped.");
                    continue;
                }

                bm = (Bitmap)Image.FromFile(texPath + texturesToLoad[i].name + ".png");//DirectBitmap.Load(texPath + texturesToLoad[i].name + ".png", false);

                if (texturesToLoad[i].flip != TexFlip.None) {
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
                }

                BitmapData data = bm.LockBits(new Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, 
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                Textures.Add(texturesToLoad[i].name, new Texture(data, TextureWrapMode.Repeat).id);

                bm.UnlockBits(data);
                bm.Dispose();
            }

            LoadColorTextures();

            Scenes = new List<IGUIElement>[]
            {
                new List<IGUIElement> {
                    UIImage.CreateCenter(0.06f, 0.06f, Textures["Crosshair"], true),
                }, // In Game 0
                new List<IGUIElement> {
                    new UIImage(-1f, -1f, 2f, 2f, Textures["BlackTransparent"], false),
                    UItext.CreateCenter("Game Menu", 0, 350, 3f, font),
                    new UISliceButton("Give Feedback", 0f, 0f, 1.8f, font, Textures["Button"], 2),
                }, // Pause menu 1
                new List<IGUIElement> {
                    UIImage.CreateRepeate(-1f, -1f, 2f, 2f, Textures["options_background"], 12, 12),
                    UIImage.CreatePixel(new Vector2i(230, 250), new Vector2i(820, 60), Textures["Black"]), // loading bar
                    UIImage.CreatePixel(new Vector2i(240, 260), new Vector2i(0, 40), Textures["Green"]), // loading bar
                    UItext.CreateCenter("Loading...", 0, 50, 5f, font),
                    UIImage.CreatePixel(new Vector2i(335, 200), new Vector2i(610, 40), Textures["Black"]), // small loading bar
                    UIImage.CreatePixel(new Vector2i(340, 205), new Vector2i(0, 30), Textures["Green"]), // small loading bar
                }, // Loading 2
                new List<IGUIElement>
                {
                    new UIImage(-1f, -1f, 2f, 2f, Textures["BlackTransparent"], false),

                }, // Inventory 3
                new List<IGUIElement>
                {
                    UIImage.CreateRepeate(-1f, -1f, 2f, 2f, Textures["options_background"], 12, 12),
                    UIImage.CreateCenter(1.8f * 1.5f, 0.29f * 1.5f, Textures["Logo"], true),
                }, // Main Menu 4
                new List<IGUIElement>
                {
                    UIImage.CreateRepeate(-1f, -1f, 2f, 2f, Textures["options_background"], 12, 12),
                    UIImage.CreateCenter(1.2f, 1.8f, Textures["BlackTransparent"], false),
                    UItext.CreateCenter("Settings", 0, 440, 6f, font),
                    new UItext("Render Distance:", -0.45f, 0.3f, 2f, font),
                    new UISlider(0.05f, 0.25f, 0.4f, 0.15f, 2f, font, 2f, 8f, 3f, 0), // 4
                    new UItext("Mouse Sensitivity:", -0.5f, 0.1f, 2f, font),
                    new UISlider(0.05f, 0.05f, 0.4f, 0.15f, 2f, font, 0.1f, 10f, 1f, 1), // 6
                    new UItext("Animated Chunks:", -0.45f, -0.1f, 2f, font),
                    new UICheckBox(0.05f, -0.15f, 0.15f, true), // 8
                    new UISliceButton("Cancel", -0.45f, -0.8f, 3f, font, Textures["Button"], 2, (MouseButton btn) => {
                        if (btn == MouseButton.Left)
                            SetScene(4);
                    }),
                    new UISliceButton("OK", 0.25f, -0.8f, 3f, font, Textures["Button"], 2, (MouseButton btn) => {
                        if (btn != MouseButton.Left)
                            return;
                        World.Settings.RenderDistance = MathPlus.RoundToInt((Scenes[5][4] as UISlider).Value);
                        World.Settings.MouseSensitivity = MathPlus.Round((Scenes[5][6] as UISlider).Value, 1);
                        World.Settings.AnimatedChunks = (Scenes[5][8] as UICheckBox).Checked;
                        World.SaveSettings();
                            SetScene(4);
                    }),
                }, // Settings 5
            };
            AddToScenes(font);
        }
        private static void AddToScenes(Font font)
        {
            int numbSlots = 9;
            slotSize = (Vector2i)(new Vector2(24, 24) * World.Settings.GUIScale);
            iteminslotSize = (Vector2i)(new Vector2(16, 16) * World.Settings.GUIScale);
            iteminslotOffset = new Vector2i(slotSize.X / 2 - iteminslotSize.X / 2, slotSize.Y / 2 - iteminslotSize.Y / 2);
            backSize = new Vector2i(4 + numbSlots * slotSize.X, slotSize.Y + 4);
            backPos = new Vector2i((int)Util.Width / 2 - backSize.X / 2, 20);

            UIItemSlot[] slots = new UIItemSlot[numbSlots];
            for (int i = 0; i < numbSlots; i++) {
                slots[i] = new UIItemSlot(new Vector2i(backPos.X + 2 + i * slotSize.X, backPos.Y + 2));
                Scenes[0].Add(slots[i]);
                Scenes[3].Add(slots[i]);
            }

            UIImage highlight = UIImage.CreatePixel(new Vector2i(backPos.X, backPos.Y), (Vector2i)(new Vector2(26, 26)
                * World.Settings.GUIScale), Textures["ToolbarHighlight"]);
            Scenes[0].Add(highlight);

            Toolbar.Init(highlight, slots);

            CreativeInventory.Init(ref Scenes[3], backPos, slotSize, numbSlots);

            // Main Menu
            (Scenes[4][1] as GUIElement).Position = new Vector3(-Scenes[4][1].Width / 2f, 0.35f, 1f);
            UISliceButton playBtn = new UISliceButton("Play", 0f, 0f, 3f, font, Textures["Button"], 2, padB: 0.075f, padT: 0.075f);
            bool clickedPlay = false;
            playBtn.OnClick = (MouseButton btn) =>
            {
                if (btn == MouseButton.Left) {
                    if (!clickedPlay) {
                        Program.Window.LockMouse();
                        SetScene(2);
                        Program.Window.JoinWorld();
                        clickedPlay = true;
                    }
                }
            };
            playBtn.PixX -= playBtn.PixWidth / 2;
            playBtn.PixY -= playBtn.PixHeight / 2;
            playBtn.UpdateApparence();
            Scenes[4].Add(playBtn);

            UISliceButton settingsBtn = new UISliceButton("Settings", 0f, 0f, 3f, font, Textures["Button"], 2, padB: 0.075f, padT: 0.075f);
            settingsBtn.OnClick = (MouseButton btn) =>
            {
                if (btn == MouseButton.Left) {
                    World.LoadSettings();
                    (Scenes[5][4] as UISlider).Value = World.Settings.RenderDistance;
                    (Scenes[5][6] as UISlider).Value = World.Settings.MouseSensitivity;
                    (Scenes[5][8] as UICheckBox).Checked = World.Settings.AnimatedChunks;
                    SetScene(5); // Settings
                }
            };
            settingsBtn.PixX -= settingsBtn.PixWidth / 2;
            settingsBtn.PixY = 200;
            settingsBtn.UpdateApparence();
            Scenes[4].Add(settingsBtn);

            UISliceButton exitBtn = new UISliceButton("Exit", 0f, 0f, 3f, font, Textures["Button"], 2, padB: 0.075f, padT: 0.075f);
            bool clickedExit = false;
            exitBtn.OnClick = (MouseButton btn) =>
            {
                if (btn == MouseButton.Left) {
                    if (!clickedExit) {
                        Program.Window.Running = false;
                        Environment.Exit(0);
                        clickedExit = true;
                    }
                }
            };
            exitBtn.PixX -= exitBtn.PixWidth / 2;
            exitBtn.PixY = 80;
            exitBtn.UpdateApparence();
            Scenes[4].Add(exitBtn);
        }

        public static List<IGUIElement> GetUnderPoint(Vector2i point)
        {
            List<IGUIElement> list = new List<IGUIElement>();
            float x = ((float)point.X / Program.Window.Width * 2f) - 1f;
            float y = (((float)point.Y / Program.Window.Height * 2f) - 1f) * -1f;
            
            for (int i = 0; i < elements.Count; i++) {
                if (x >= elements[i].Position.X && y >= elements[i].Position.Y
                    && x < elements[i].Position.X + elements[i].Width && y < elements[i].Position.Y + elements[i].Height)
                    list.Add(elements[i]);
            }

            return list;
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

            db = new DirectBitmap(1, 1);
            db.Clear(Color.FromArgb(255, 255, 255, 255));
            Textures.Add("White", new Texture(db.Data, db.Width, db.Height, TextureWrapMode.Repeat).id);
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

            if (Scene == 3)
                DragAndDropHandler.cursorSlot.Render(uiShader);
        }

        public static void OnKeyPress(char keyChar)
        {
            for (int i = 0; i < elements.Count; i++) {
                elements[i].OnKeyPress(keyChar);
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
            float x = (float)pos.X / Program.Window.Width * 2f - 1f;
            float y = ((float)pos.Y / Program.Window.Height * 2f - 1) * -1f;
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
