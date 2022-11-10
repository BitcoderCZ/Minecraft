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
    public class UItext : GUIElement
    {
        public string Text;
        private UIImage[] chars;
        public float Scale;
        private Font font;

        public static UItext CreateCenter(string text, int xOff, int yOff, float scale, Font font)
        {
            float realWidth = 0f;
            for (int i = 0; i < text.Length; i++) {
                realWidth += (float)font.Sizes[text[i]].Width / Util.Width * scale;
            }
            float off = realWidth / 2f;
            return new UItext(text, (float)xOff / Util.Width - off, (float)yOff / Util.Height, scale, font);
        }

        public UItext(string _text, float x, float y, float scale, Font _font)
        {
            Position = new Vector3(x, y, 0f);
            Text = _text;
            chars = new UIImage[Text.Length];
            Scale = scale;
            font = _font;

            CreateMesh();

            Active = true;
        }

        public override void Render(Shader s)
        {
            if (!Active)
                return;

            for (int i = 0; i < chars.Length; i++) {
                chars[i].Render(s);
            }
            /*int slot = 0;
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.Uniform1(6, slot);

            GL.BindVertexArray(vao);
            GL.DrawElements(BeginMode.Triangles, triangles.Length, DrawElementsType.UnsignedInt, 0);*/
        }

        private void CreateMesh()
        {
            /*List<uint> tris = new List<uint>();
            List<Vertex2D> verts = new List<Vertex2D>();*/

            float xOff = 0f;

            for (int i = 0; i < Text.Length; i++) {
                CreateCharMesh(/*ref tris, ref verts,*/ Text[i], i, ref xOff);
            }

            /*triangles = tris.ToArray();
            vertices = verts.ToArray();

            InitMesh();*/
        }

        private void CreateCharMesh(char ch, int i, ref float xOff)
        {
            float width = (float)font.Sizes[ch].Width / Util.Width * Scale;
            float height = (float)font.Sizes[ch].Height / Util.Height * Scale;

            float yOff = 0f;

            if (ch == 'p' || ch == 'q' || ch == 'g' || ch == 'y')
                yOff = 0.01f;

            yOff *= Scale;

            chars[i] = new UIImage(Position.X + xOff, Position.Y - yOff, width, height, font.Chars[ch], false);

            xOff += width;
        }
    }
}
