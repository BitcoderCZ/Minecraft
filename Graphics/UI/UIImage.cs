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
        public int textureID;

        public static UIImage CreateCenter(float width, float height, int _textureID, bool mentainAspectRatio)
        {
            /*if (mentainAspectRatio)
                return new UIImage((-width / 2f) / Program.Window.AspectRatio, -height / 2f, width / Program.Window.AspectRatio, height, _textureID, false);
            else
                return new UIImage(-width / 2f, -height / 2f, width, height, _textureID, mentainAspectRatio);*/
            return new UIImage(-width / 2f, -height / 2f, width, height, _textureID, mentainAspectRatio);
        }

        public static UIImage CreatePixel(int x, int y, int width, int height, int _textureID, float z = 1f)
            => new UIImage(Util.PixelToGL(x, y), Util.PixelToNormal(width, height), _textureID, false, z);
        public static UIImage CreatePixel(Vector2i pos, Vector2i size, int _textureID, float z = 1f)
            => new UIImage(Util.PixelToGL(pos), Util.PixelToNormal(size) * 2, _textureID, false, z);

        public UIImage(float x, float y, float width, float height, int _textureID, bool mentainAspectRatio, float z = 1f)
        {
            Position = new Vector3((mentainAspectRatio ? x / Program.Window.AspectRatio : x), y, 0f);
            Width = mentainAspectRatio ? width / Program.Window.AspectRatio : width;
            Height = height;
            textureID = _textureID;

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
