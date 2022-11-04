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

namespace Minecraft
{
    public class Texture
    {
        public static int taid;

        public static readonly int[] textures = new int[1];

        public int id;
        public int Width;
        public int Height;

        private static readonly string texPath = Environment.CurrentDirectory + "/Data/Textures/Blocks/";

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
        };

        public static void CreateTA()
        {
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.GenTextures(1, out taid);
            GL.BindTexture(TextureTarget.Texture2DArray, taid);

            GL.PixelStore(PixelStoreParameter.UnpackRowLength, 16/*width*/);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMaxLevel, 4);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba32f, 16, 16, blockTextures.Length/*count*/);

            Bitmap bm;
            BitmapData data;

            Console.WriteLine("TEXTURE:BLOCKARRAY:GENERATE:START");
            for (int i = 0; i < blockTextures.Length; i++) {
                if (!File.Exists(texPath + blockTextures[i].name + ".png")) {
                    Console.WriteLine($"Block texture {blockTextures[i]}, wasn't found. skipped.");
                    continue;
                }

                bm = new Bitmap(texPath + blockTextures[i].name + ".png");

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
            Console.WriteLine($"TEXTURE:BLOCKARRAY:GENERATE:DONE count: {blockTextures.Length}");
        }

       /* public Texture(string path, int _id)
        {
            DirectBitmap db = DirectBitmap.Load(path, false);

            Width = db.Width;
            Height = db.Height;

            GL.GenTextures(1, out id);
            GL.BindTexture(TextureTarget.Texture2D, id);

            textures[_id - 1] = id;

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
        }*/
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

    [Flags]
    public enum TexFlip : byte
    {
        None = 0b0000_0000,
        Vertical = 0b0000_0001,
        Horizontal = 0b_0000_0010,
    }
}
