using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemPlus.Utils;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Minecraft
{
    public class Texture
    {
        public int id;
        public int Width;
        public int Height;

        public Texture(string path)
        {
            //Bitmap db = (Bitmap)Image.FromFile(path);
            DirectBitmap db = DirectBitmap.Load(path, false);

            Width = db.Width;
            Height = db.Height;

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
    }
}
