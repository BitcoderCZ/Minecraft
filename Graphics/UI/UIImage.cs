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
        private int textureID;

        public static UIImage CreateCenter(float width, float height, int _textureID, bool square)
        {
            if (square)
                return new UIImage((-width / 2f) / Program.Window.AspectRatio, -height / 2f, width / Program.Window.AspectRatio, height, _textureID);
            else
                return new UIImage(-width / 2f, -height / 2f, width, height, _textureID);
        }

        public UIImage(float x, float y, float width, float height, int _textureID)
        {
            Position = new Vector3(x, y, 0f);
            Width = width;
            Height = height;
            textureID = _textureID;

            vertices = new Vertex2D[4]
            {
                new Vertex2D(0f, 0f, 0f, 1f),
                new Vertex2D(0f, 0f + height, 0f, 0f),
                new Vertex2D(0f + width, 0f, 1f, 1f),
                new Vertex2D(0f + width, 0f + height, 1f, 0f),
            };
            triangles = new uint[6]
            {
                2, 1, 0,
                2, 3, 1,
            };

            InitMesh();
            Active = true;
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
