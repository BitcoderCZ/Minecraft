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

using Minecraft.Math;
using System.ComponentModel;
using Minecraft.Graphics;
using Minecraft.Graphics.UI;

namespace Minecraft
{
    public class Window : GameWindow
    {
        public float AspectRatio;

        public bool Running;

        Shader shader;
        Shader texShader;
        Shader uiShader;
        public KeyboardState keyboardState;

        // Mouse
        bool mouseLocked;
        Point lastMousePos;
        Point origCursorPosition; // position before lock

        public Window()
        {
            Width = 1280;
            Height = 720;
            AspectRatio = (float)Width / (float)Height;
            Title = "Minecraft";
        }

        public DebugProc debMessageCallback;

        protected override void OnLoad(EventArgs e)
        {
            Running = true;

            GL.Enable(EnableCap.DebugOutput);
            debMessageCallback = new DebugProc(MessageCallback); // Fixed error: A callback was made on a garbage collected delegate
            GL.DebugMessageCallback(debMessageCallback, IntPtr.Zero); 

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            shader = new Shader();
            shader.Compile("shader");
            uiShader = new Shader();
            uiShader.Compile("ui");
            texShader = new Shader();
            texShader.Compile("tex");

            Texture.CreateBlockTA();
            GUI.Init();
            GUI.SetScene(0); // Game

            World.Generate();

            base.WindowBorder = WindowBorder.Fixed;
            base.WindowState = WindowState.Normal;
            GL.Viewport(0, 0, Width, Height);
            shader.Bind();
            Player.SetRotation(new Vector3(0f, 180f, 0f));
            Camera.UpdateView(Width, Height);
            shader.UploadMat4("uProjection", ref Camera.projMatrix);
            shader.UploadMat4("uView", ref Camera.viewMatrix);

            LockMouse();
        }

        private void MessageCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            if (id == 131185)
                return;
            byte[] managedArray = new byte[length];
            Marshal.Copy(message, managedArray, 0, length);
            Console.WriteLine($"MessageCallback: Source:{source}, Type:{type}, id:{id}, " +
                $"Severity:{severity}, Message: {Encoding.ASCII.GetString(managedArray)}");
        }

        float halfSecondUpdate = 0f;
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            float delta = (float)e.Time;

            // Rotation
            if (keyboardState.IsKeyDown(Key.Left))
                Player.Rotation.Y += delta * 160f;
            else if (keyboardState.IsKeyDown(Key.Right))
                Player.Rotation.Y -= delta * 160f;
            if (keyboardState.IsKeyDown(Key.Up))
                Player.Rotation.X -= delta * 80f;
            else if (keyboardState.IsKeyDown(Key.Down))
                Player.Rotation.X += delta * 80f;

            if (mouseLocked) {
                Point mouseDelta = System.Windows.Forms.Cursor.Position - new Size(lastMousePos);
                if (mouseDelta != Point.Empty) {
                    Player.Rotation.X += mouseDelta.Y * 0.25f;
                    Player.Rotation.Y += -mouseDelta.X * 0.25f;
                    CenterCursor();
                }
            }

            if (Player.Rotation.X < -89)
                Player.Rotation.X = -89;
            else if (Player.Rotation.X > 89)
                Player.Rotation.X = 89;

            Player.Update(keyboardState, delta);

            // Other keyboard
            if (keyboardState.IsKeyDown(Key.Escape)) {
                UnlockMouse();
                GUI.SetScene(1);
            }

            float FPS = 1f / delta;

            halfSecondUpdate += delta;
            if (halfSecondUpdate > 0.5f) {
                halfSecondUpdate = 0f;

                Title = $"Minecraft FPS: {SystemPlus.MathPlus.Round(FPS, 2)}";
            }

            World.Update();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(Color.Blue);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            shader.Bind();
            Camera.UpdateView(Width, Height);
            shader.UploadMat4("uProjection", ref Camera.projMatrix);
            shader.UploadMat4("uView", ref Camera.viewMatrix);
            World.Render(shader);

            texShader.Bind();
            shader.UploadMat4("uProjection", ref Camera.projMatrix);
            shader.UploadMat4("uView", ref Camera.viewMatrix);
            Player.Render(texShader);
            GUI.Render(uiShader);
            
            SwapBuffers();
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            keyboardState = e.Keyboard;
            GUI.OnKeyDown(e.Key, e.Modifiers);
        }
        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            keyboardState = e.Keyboard;
            GUI.OnKeyUp(e.Key, e.Modifiers);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (!mouseLocked /*&& GUI.Scene == 0*/) {
                LockMouse();
                GUI.SetScene(0);
            }
            else
                Player.MouseDown(e.Button);

            GUI.OnMouseDown(e.Button, e.Position);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            GUI.OnMouseUp(e.Button, e.Position);
        }

        // Mouse
        private void CenterCursor()
        {
            System.Windows.Forms.Cursor.Position = new Point(Width / 2 + Location.X, Height / 2 + Location.Y);
            lastMousePos = System.Windows.Forms.Cursor.Position;
        }
        protected void LockMouse()
        {
            mouseLocked = true;
            origCursorPosition = System.Windows.Forms.Cursor.Position;
            CursorVisible = false;
            CenterCursor();
        }

        protected void UnlockMouse()
        {
            mouseLocked = false;
            CursorVisible = true;
            System.Windows.Forms.Cursor.Position = origCursorPosition;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            Player.MouseScrool(e.Delta);
        }

        protected override void OnClosed(EventArgs e)
        {
            Running = false;
        }
    }
}
