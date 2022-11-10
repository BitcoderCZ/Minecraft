using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemPlus.Utils;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Minecraft.Graphics
{
    public class Texture
    {
        public static int taid;
        public static int uiid;

        public static readonly int[] textures = new int[2];
        public static int[] items;

        public int id;
        public int Width;
        public int Height;

        private static readonly string blockPath = Environment.CurrentDirectory + "/Data/Textures/Blocks/";
        private static readonly string itemPath = Environment.CurrentDirectory + "/Data/Textures/Items/";

        private static readonly TextureInfo[] blockTextures = new TextureInfo[]
        {
            new TextureInfo("stone"), // 1
            new TextureInfo("grass_block_top", new Vector3(0.65f, 1f, 0.2f)), // 2
            new TextureInfo("grass_block_side", TexFlip.Horizontal), // 3
            new TextureInfo("dirt"), // 4
            new TextureInfo("cobblestone"), // 5
            new TextureInfo("oak_planks"), // 6
            new TextureInfo("oak_sapling"), // 7
            new TextureInfo("bedrock"), // 8
            new TextureInfo("sand"), // 9
            new TextureInfo("granite"), // 10
            new TextureInfo("polished_granite"), // 11
            new TextureInfo("diorite"), // 12
            new TextureInfo("polished_diorite"), // 13
            new TextureInfo("andesite"), // 14
            new TextureInfo("polished_andesite"), // 15
            new TextureInfo("oak_log"), // 16
            new TextureInfo("oak_leaves", new Vector3(0.65f, 1f, 0.2f)), // 17
        };
        private static readonly ItemTextureInfo[] itemTextures = new ItemTextureInfo[]
        {
            new ItemTextureInfo("stone", LF.Items),
            new ItemTextureInfo("grass_block", LF.Items),
            new ItemTextureInfo("dirt", LF.Items),
            new ItemTextureInfo("cobblestone", LF.Items),
            new ItemTextureInfo("oak_planks", LF.Items),
            new ItemTextureInfo("oak_sapling", LF.Blocks),
            new ItemTextureInfo("bedrock", LF.Items),
            new ItemTextureInfo("sand", LF.Items),
            new ItemTextureInfo("granite", LF.Items),
        };

        public static void LoadItems()
        {
            items = new int[itemTextures.Length + 1];
            items[0] = -1;

            DirectBitmap bm;
            for (int i = 0; i < itemTextures.Length; i++) {
                string path = (itemTextures[i].loadFrom == LF.Blocks ? blockPath : itemPath) + itemTextures[i].name + ".png";
                if (!File.Exists(path)) {
                    Console.WriteLine($"Block texture {itemTextures[i].name}, wasn't found. skipped.");
                    continue;
                }

                bm = DirectBitmap.Load(path, false);

                if (itemTextures[i].flip != TexFlip.None) {
                    if ((itemTextures[i].flip & TexFlip.Vertical) == TexFlip.Vertical)
                        bm.Bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    if ((itemTextures[i].flip & TexFlip.Horizontal) == TexFlip.Horizontal)
                        bm.Bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                }

                if (itemTextures[i].colorFilter != Vector3.One) {
                    Vector3 filter = itemTextures[i].colorFilter;
                    for (int x = 0; x < bm.Width; x++)
                        for (int y = 0; y < bm.Height; y++) {
                            Color c = bm.GetPixel(x, y);
                            float r = (float)(c.R / 255f) * filter.X;
                            float g = (float)(c.G / 255f) * filter.Y;
                            float b = (float)(c.B / 255f) * filter.Z;
                            bm.SetPixel(x, y, Color.FromArgb(255, (byte)(r * 255f), (byte)(g * 255f), (byte)(b * 255f)));
                        }
                }

                items[i + 1] = new Texture(bm.Data, bm.Width, bm.Height, TextureWrapMode.Repeat).id;

                bm.Dispose();
            }
        }

        public static void CreateBlockTA()
        {
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.GenTextures(1, out taid);
            GL.BindTexture(TextureTarget.Texture2DArray, taid);

            //GL.PixelStore(PixelStoreParameter.UnpackRowLength, 16/*width*/); NO!!!
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMaxLevel, 4);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba32f, 16, 16, blockTextures.Length);

            Bitmap bm;
            BitmapData data;

            Console.WriteLine("TEXTURE:BLOCKARRAY:GENERATE:BLOCK:START");
            for (int i = 0; i < blockTextures.Length; i++) {
                if (!File.Exists(blockPath + blockTextures[i].name + ".png")) {
                    Console.WriteLine($"Block texture {blockTextures[i].name}, wasn't found. skipped.");
                    continue;
                }

                bm = new Bitmap(blockPath + blockTextures[i].name + ".png");

                if (blockTextures[i].flip != TexFlip.None) {
                    if ((blockTextures[i].flip & TexFlip.Vertical) == TexFlip.Vertical)
                        bm.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    if ((blockTextures[i].flip & TexFlip.Horizontal) == TexFlip.Horizontal)
                        bm.RotateFlip(RotateFlipType.RotateNoneFlipY);
                }

                if (blockTextures[i].colorFilter != Vector3.One) {
                    Vector3 filter = blockTextures[i].colorFilter;
                    for (int x = 0; x < bm.Width; x++)
                        for (int y = 0; y < bm.Height; y++) {
                            Color c = bm.GetPixel(x, y);
                            float r = (float)(c.R / 255f) * filter.X;
                            float g = (float)(c.G / 255f) * filter.Y;
                            float b = (float)(c.B / 255f) * filter.Z;
                            bm.SetPixel(x, y, Color.FromArgb(255, (byte)(r * 255f), (byte)(g * 255f), (byte)(b * 255f)));
                        }
                }

                data = bm.LockBits(new Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, i, bm.Width, bm.Height, 1, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                bm.UnlockBits(data);
                bm.Dispose();
            }
            Console.WriteLine($"TEXTURE:BLOCKARRAY:GENERATE:BLOCK:DONE count: {blockTextures.Length}");
        }

        public Texture(string path)
        {
            DirectBitmap db = DirectBitmap.Load(path, false);

            Width = db.Width;
            Height = db.Height;

            int slot = 0;
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            GL.GenTextures(1, out id);
            GL.BindTexture(TextureTarget.Texture2D, id);

            BitmapData data = db.Bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            db.Bitmap.UnlockBits(data);

            db.Dispose();
        }

        public Texture(DirectBitmap db)
        {
            Width = db.Width;
            Height = db.Height;

            int slot = 0;
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            GL.GenTextures(1, out id);
            GL.BindTexture(TextureTarget.Texture2D, id);

            BitmapData data = db.Bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            db.Bitmap.UnlockBits(data);

            db.Dispose();
        }

        public Texture(BitmapData data, TextureWrapMode wrapMode)
        {
            Width = data.Width;
            Height = data.Height;

            int slot = 0;
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            GL.GenTextures(1, out id);
            GL.BindTexture(TextureTarget.Texture2D, id);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        public Texture(int[] pixels, int width, int height, TextureWrapMode wrapMode)
        {
            Width = width;
            Height = height;

            int slot = 0;
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            GL.GenTextures(1, out id);
            GL.BindTexture(TextureTarget.Texture2D, id);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }
    }

    public struct TextureInfo
    {
        public string name;
        public TexFlip flip;
        public Vector3 colorFilter;

        public TextureInfo(string _name, TexFlip _flip, Vector3 _colorFilter)
        {
            name = _name;
            flip = _flip;
            colorFilter = _colorFilter;
        }

        public TextureInfo(string _name, TexFlip _flip) : this(_name, _flip, Vector3.One)
        { }

        public TextureInfo(string _name, Vector3 _colorFilter) : this(_name, TexFlip.None, _colorFilter)
        { }

        public TextureInfo(string _name) : this(_name, TexFlip.None, Vector3.One)
        { }
    }

    public struct ItemTextureInfo
    {
        public string name;
        public TexFlip flip;
        public LF loadFrom;
        public Vector3 colorFilter;

        public ItemTextureInfo(string _name, TexFlip _flip, LF _loadFrom, Vector3 _colorFilter)
        {
            name = _name;
            flip = _flip;
            loadFrom = _loadFrom;
            colorFilter = _colorFilter;
        }

        public ItemTextureInfo(string _name, TexFlip _flip) : this(_name, _flip, LF.Items, Vector3.One)
        { }

        public ItemTextureInfo(string _name, Vector3 _colorFilter) : this(_name, TexFlip.None, LF.Items, _colorFilter)
        { }

        public ItemTextureInfo(string _name) : this(_name, TexFlip.None, LF.Items, Vector3.One)
        { }

        public ItemTextureInfo(string _name, LF _loadFrom) : this(_name, TexFlip.None, _loadFrom, Vector3.One)
        { }
    }

    [Flags]
    public enum TexFlip : byte
    {
        None = 0b0000_0000,
        Vertical = 0b0000_0001,
        Horizontal = 0b_0000_0010,
    }

    public enum LF : byte
    {
        Blocks = 0,
        Items = 1,
    }
}
