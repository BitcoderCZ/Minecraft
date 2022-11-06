using Minecraft.VertexTypes;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.Graphics.UI
{
    public abstract class GUIElement : IGUIElement
    {
        public Vector3 Position { get; set; }

        public float Width { get; protected set; }
        public float Height { get; protected set; }

        public bool Active { get; set; }

        public Vertex2D[] vertices;
        public uint[] triangles;

        protected int vao;
        protected int vbo;
        protected int ebo;

        public abstract void Render(Shader s);
        public virtual void Update(float delta) { }
        public virtual void OnMouseDown(MouseButton button, bool onElement) { }
        public virtual void OnMouseUp(MouseButton button, bool onElement) { }
        public virtual void OnKeyDown(Key key, KeyModifiers modifiers) { }
        public virtual void OnKeyUp(Key key, KeyModifiers modifiers) { }

        protected virtual void InitMesh()
        {
            GL.CreateVertexArrays(1, out vao);
            GL.BindVertexArray(vao);
            GL.CreateBuffers(1, out ebo);
            GL.CreateBuffers(1, out vbo);
            UploadMesh();
        }

        protected virtual void UploadMesh()
        {
            GL.NamedBufferData(ebo, triangles.Length * sizeof(uint), triangles, BufferUsageHint.DynamicDraw);
            GL.VertexArrayElementBuffer(vao, ebo);

            int vertexBindingPoint = 0;
            GL.NamedBufferData(vbo, vertices.Length * Vertex2D.Size, vertices, BufferUsageHint.DynamicDraw);
            GL.VertexArrayVertexBuffer(vao, vertexBindingPoint, vbo, IntPtr.Zero, Vertex2D.Size);

            // pos
            GL.VertexArrayAttribFormat(vao, 0, 2, VertexAttribType.Float, false, 0);
            GL.VertexArrayAttribBinding(vao, 0, vertexBindingPoint);
            GL.EnableVertexArrayAttrib(vao, 0);
            // uv
            GL.VertexArrayAttribFormat(vao, 1, 2, VertexAttribType.Float, false, 2 * sizeof(float));
            GL.VertexArrayAttribBinding(vao, 1, vertexBindingPoint);
            GL.EnableVertexArrayAttrib(vao, 1);
        }
    }
}
