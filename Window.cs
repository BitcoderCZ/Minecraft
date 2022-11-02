using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using OpenTK.Input;
using System.Runtime.InteropServices;

namespace Minecraft
{
    public class Window : GameWindow
    {
        Shader shader;
        Texture dirtTex;
        Vector3[] cubes;
        KeyboardState keyboardState;

        public Window()
        {
            Width = 1280;
            Height = 720;
            Title = "Minecraft";
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.Enable(EnableCap.DebugOutput);
            GL.DebugMessageCallback(MessageCallback, IntPtr.Zero);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            shader = new Shader();
            shader.Compile("shader");

            dirtTex = new Texture(Environment.CurrentDirectory + "/Data/textures/dirt.png");

            cubes = new Vector3[100];
            int i = 0;
            for (int x = -5; x < 5; x++) {
                for (int z = -5; z < 5; z++) {
                    cubes[i] = new Vector3(x, 0, z);
                    i++;
                }
            }

            base.WindowBorder = WindowBorder.Fixed;
            base.WindowState = WindowState.Normal;
            GL.Viewport(0, 0, Width, Height);
            shader.Bind();
            Camera.SetRotation(new Vector3(0f, 180f, 0f));
            Camera.UpdateView(Width, Height);
            shader.UploadMat4("uProjection", ref Camera.projMatrix);
            shader.UploadMat4("uView", ref Camera.viewMatrix);
        }

        private void MessageCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            byte[] managedArray = new byte[length];
            Marshal.Copy(message, managedArray, 0, length);
            Console.WriteLine($"MessageCallback: Source:{source}, Type:{type}, id:{id}, " +
                $"Severity:{severity}, Message: {Encoding.ASCII.GetString(managedArray)}");
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

            if (Camera.Rotation.X < -85)
                Camera.Rotation.X = -85;
            else if (Camera.Rotation.X > 85)
                Camera.Rotation.X = 85;

            if (keyboardState.IsKeyDown(Key.W))
                Camera.Move(0f, delta * 8f);
            else if (keyboardState.IsKeyDown(Key.S))
                Camera.Move(180f, delta * 8f);
            if(keyboardState.IsKeyDown(Key.A))
                Camera.Move(90f, delta * 8f);
            else if (keyboardState.IsKeyDown(Key.D))
                Camera.Move(270f, delta * 8f);
            if (keyboardState.IsKeyDown(Key.Space))
                Camera.position.Y += delta * 6f;
            else if (keyboardState.IsKeyDown(Key.ShiftLeft))
                Camera.position.Y -= delta * 6f;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(Color.Blue);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            shader.Bind();
            Camera.UpdateView(Width, Height);
            shader.UploadMat4("uProjection", ref Camera.projMatrix);
            shader.UploadMat4("uView", ref Camera.viewMatrix);
            for (int i = 0; i < cubes.Length; i++) {
                Cube.drawCube(cubes[i], dirtTex, shader);
            }

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
