using Minecraft.Math;
using Minecraft.VertexTypes;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.Graphics.UI
{
    public class UIImage : GUIElement
    {
        public int PixX
        {
            get => (int)(((Position.X + 1f) / 2f) * (float)Util.Width); // Width is 0 - 2
            set => Position = new Vector3((((float)value / (float)Util.Width) * 2f) - 1f, Position.Y, Position.Z);
        }
        public int PixY
        {
            get => (int)(((Position.Y + 1f) / 2f) * (float)Util.Height); // Width is 0 - 2
            set => Position = new Vector3(Position.X, (((float)value / (float)Util.Height) * 2f) - 1f, Position.Z);
        }

        public int PixWidth
        {
            get => (int)((Width / 2f) * (float)Util.Width); // Width is 0 - 2
            set => Width = ((float)value / (float)Util.Width) * 2f;
        }
        public int PixHeight
        {
            get => (int)((Height / 2f) * (float)Util.Height); // Width is 0 - 2
            set => Height = ((float)value / (float)Util.Height) * 2f;
        }

        public int textureID;

        private float z;

        public static UIImage CreateCenter(float width, float height, int _textureID, bool mentainAspectRatio)
            => new UIImage(-width / 2f, -height / 2f, width, height, _textureID, mentainAspectRatio);

        public static UIImage CreatePixel(int x, int y, int width, int height, int _textureID, float z = 1f)
            => new UIImage(Util.PixelToGL(x, y), Util.PixelToNormal(width, height) * 2, _textureID, false, z);
        public static UIImage CreatePixel(Vector2i pos, Vector2i size, int _textureID, float z = 1f)
            => new UIImage(Util.PixelToGL(pos), Util.PixelToNormal(size) * 2, _textureID, false, z);

        public static UIImage CreateRepeate(float x, float y, float width, float height, int _textureID, int _repX, int repY, float z = 1f)
        {
            UIImage img = new UIImage(x, y, width, height, _textureID, false, z);

            float repX = (float)_repX * Program.Window.AspectRatio;

            img.vertices[0] = new Vertex2D(0f, 0f, -z, 0f, 1f * repY);
            img.vertices[1] = new Vertex2D(0f, img.Height, -z, 0f, 0f);
            img.vertices[2] = new Vertex2D(img.Width, 0f, -z, 1f * repX, 1f * repY);
            img.vertices[3] = new Vertex2D(img.Width, img.Height, -z, 1f * repX, 0f);

            img.UploadMesh();
            return img;
        }

        public UIImage()
        {

        }

        public UIImage(float x, float y, float width, float height, int _textureID, bool mentainAspectRatio, float _z = 1f)
        {
            Position = new Vector3((mentainAspectRatio ? x / Program.Window.AspectRatio : x), y, 0f);
            Width = mentainAspectRatio ? width / Program.Window.AspectRatio : width;
            Height = height;
            textureID = _textureID;

            z = _z;
            vertices = new Vertex2D[4]
            {
                new Vertex2D(0f, 0f, -z, 0f, 1f),
                new Vertex2D(0f, Height, -z, 0f, 0f),
                new Vertex2D(Width, 0f, -z, 1f, 1f),
                new Vertex2D(Width, Height, -z, 1f, 0f),
            };
            triangles = new uint[6]
            {
                2, 1, 0,
                2, 3, 1,
            };

            InitMesh();
            Active = true;
        }
        public UIImage(Vector2 _pos, float width, float height, int _textureID, bool mentainAspectRatio, float z = 1f) : this(_pos.X, _pos.Y, width, height, _textureID, mentainAspectRatio, z)
        { }
        public UIImage(Vector2 _pos, Vector2 _size, int _textureID, bool mentainAspectRatio, float z = 1f) : this(_pos.X, _pos.Y, _size.X, _size.Y, _textureID, mentainAspectRatio, z)
        { }

        public void UpdateVerts()
        {
            vertices[0] = new Vertex2D(0f, 0f, -z, 0f, 1f);
            vertices[1] = new Vertex2D(0f, Height, -z, 0f, 0f);
            vertices[2] = new Vertex2D(Width, 0f, -z, 1f, 1f);
            vertices[3] = new Vertex2D(Width, Height, -z, 1f, 0f);

            UploadMesh();
        }

        public override void Render(Shader s)
        {
            if (!Active)
                return;

            Matrix4 mat = Matrix4.CreateTranslation(Position);
            s.UploadMat4("uTransform", ref mat);

            int slot = 0;
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.Uniform1(6, slot);

            GL.BindVertexArray(vao);
            GL.DrawElements(BeginMode.Triangles, triangles.Length, DrawElementsType.UnsignedInt, 0);
        }
    }
}
