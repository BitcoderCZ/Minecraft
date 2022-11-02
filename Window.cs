using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using OpenTK.Input;

namespace Minecraft
{
    public class Window : GameWindow
    {
        Shader shader;
        int vao;
        int vbo;
        int ebo;
        Matrix4 transform;
        KeyboardState keyboardState;

        public Window()
        {
            Width = 1280;
            Height = 720;
            Title = "Minecraft";
        }

        void Upload()
        {
            Vertex[] vertices = new Vertex[]
            {
                new Vertex(){ position = new Vector3(-0.5f, -0.5f, 0f)},
                new Vertex(){ position = new Vector3(-0.5f, 0.5f, 0f)},
                new Vertex(){ position = new Vector3(0.5f, 0.5f, 0f)},
                new Vertex(){ position = new Vector3(0.5f, -0.5f, 0f)},
            };

            uint[] indices = new uint[]
            {
                0, 1, 3,
                1, 2, 3
            };

            GL.CreateVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            GL.CreateBuffers(1, out ebo);
            GL.NamedBufferData(ebo, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);
            GL.VertexArrayElementBuffer(vao, ebo);

            int vertexBindingPoint = 0;
            GL.CreateBuffers(1, out vbo);
            GL.NamedBufferData(vbo, vertices.Length * Vertex.Size, vertices, BufferUsageHint.StaticDraw);
            GL.VertexArrayVertexBuffer(vao, vertexBindingPoint, vbo, IntPtr.Zero, Vertex.Size);

            // pos
            GL.VertexArrayAttribFormat(vao, 0, 3, VertexAttribType.Float, false, 0);
            GL.VertexArrayAttribBinding(vao, 0, vertexBindingPoint);
            GL.EnableVertexArrayAttrib(vao, 0);
            // color
            /*GL.VertexArrayAttribFormat(vao, 1, 4, VertexAttribType.Float, false, 3 * sizeof(float));
            GL.VertexArrayAttribBinding(vao, 1, vertexBindingPoint);
            GL.EnableVertexArrayAttrib(vao, 1);*/
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.Enable(EnableCap.DebugOutput);
            GL.DebugMessageCallback(MessageCallback, IntPtr.Zero);

            shader = new Shader();
            shader.Compile("shader");

            base.WindowBorder = WindowBorder.Fixed;
            base.WindowState = WindowState.Normal;
            GL.Viewport(0, 0, Width, Height);
            shader.Bind();
            Camera.SetRotation(new Vector3(0f, 180f, 0f));
            Camera.UpdateView(Width, Height);
            transform = Matrix4.CreateTranslation(new Vector3(0f, 0f, 0f));
            GL.UniformMatrix4(0, false, ref transform);
            GL.UniformMatrix4(1, false, ref Camera.projMatrix);
            GL.UniformMatrix4(2, false, ref Camera.viewMatrix);
            Upload();
        }

        private void MessageCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            Console.WriteLine($"MessageCallback: Source:{source}, Type:{type}, id:{id}, Severity:{severity}, Message: {message}");
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            float delta = (float)e.Time;
            if (keyboardState.IsKeyDown(Key.Left))
                Camera.Rotation.Y += delta * 160f;
            else if (keyboardState.IsKeyDown(Key.Right))
                Camera.Rotation.Y -= delta * 160f;
            if (keyboardState.IsKeyDown(Key.Up))
                Camera.Rotation.X -= delta * 80f;
            else if (keyboardState.IsKeyDown(Key.Down))
                Camera.Rotation.X += delta * 80f;

            if (Camera.Rotation.X < -90)
                Camera.Rotation.X = -90;
            else if (Camera.Rotation.X > 90)
                Camera.Rotation.X = 90;

            if (keyboardState.IsKeyDown(Key.W))
                Camera.Move(0f, delta * 8f);
            else if (keyboardState.IsKeyDown(Key.S))
                Camera.Move(180f, delta * 8f);
            if(keyboardState.IsKeyDown(Key.A))
                Camera.Move(90f, delta * 8f);
            else if (keyboardState.IsKeyDown(Key.D))
                Camera.Move(270f, delta * 8f);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(Color.Blue);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            shader.Bind();
            Camera.UpdateView(Width, Height);
            GL.UniformMatrix4(0, false, ref transform);
            GL.UniformMatrix4(1, false, ref Camera.projMatrix);
            GL.UniformMatrix4(2, false, ref Camera.viewMatrix);
            GL.BindVertexArray(vao);
            GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }
        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            keyboardState = e.Keyboard;
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            keyboardState = e.Keyboard;
        }
    }
}
