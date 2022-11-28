using OpenTK;
using System;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using OpenTK.Input;
using System.Runtime.InteropServices;

using Minecraft.Graphics;
using Minecraft.Graphics.UI;
using System.Threading;

using Font = Minecraft.Graphics.Font;
using Minecraft.Math;

namespace Minecraft
{
    public class Window : GameWindow
    {
        public float AspectRatio;

        public bool Running;

        Shader shader;
        Shader texShader;
        Shader uiShader;
        public Font font;
        public KeyboardState keyboardState;
        public Vector2 mousePos;

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

            Icon = new Icon(Environment.CurrentDirectory + "/Data/icon.ico");
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

            shader = new Shader(); // Used by blocks
            shader.Compile("shader");
            uiShader = new Shader(); // Used by UI
            uiShader.Compile("ui");
            texShader = new Shader(); // Used for non world 3d
            texShader.Compile("tex");

            Texture.CreateBlockTA();
            Texture.LoadItems();
            World.Init();
            font = new Font("Minecraft", 32);
            GUI.Init(font);

            GUI.SetScene(4); // Loading

            base.WindowBorder = WindowBorder.Fixed;
            base.WindowState = WindowState.Normal;
            GL.Viewport(0, 0, Width, Height);
            shader.Bind();
            Player.SetRotation(new Vector3(0f, 180f, 0f));
            Camera.UpdateView(Width, Height);
            shader.UploadMat4("uProjection", ref Camera.projMatrix);
            shader.UploadMat4("uView", ref Camera.viewMatrix);

            LockMouse();
            UnlockMouse();
        }

        private void MessageCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr _message, IntPtr userParam)
        {
            if (id == 131185)
                return;
            string message = Marshal.PtrToStringAnsi(_message, length);
            Console.WriteLine($"MessageCallback: Source:{source}, Type:{type}, id:{id}, " +
                $"Severity:{severity}, Message: {message}");
        }

        float halfSecondUpdate = 0f;
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            float delta = (float)e.Time;

            if (mouseLocked) {
                Point mouseDelta = System.Windows.Forms.Cursor.Position - new Size(lastMousePos);
                if (mouseDelta != Point.Empty) {
                    if (GUI.Scene == 0) {
                        Player.Rotation.X += mouseDelta.Y * World.Settings.MouseSensitivity * 0.25f;
                        Player.Rotation.Y += -mouseDelta.X * World.Settings.MouseSensitivity * 0.25f;
                    }
                    CenterCursor();
                }
            }

            if (GUI.Scene == 0) {
                // Rotation
                if (keyboardState.IsKeyDown(Key.Left))
                    Player.Rotation.Y += delta * 160f;
                else if (keyboardState.IsKeyDown(Key.Right))
                    Player.Rotation.Y -= delta * 160f;
                if (keyboardState.IsKeyDown(Key.Up))
                    Player.Rotation.X -= delta * 80f;
                else if (keyboardState.IsKeyDown(Key.Down))
                    Player.Rotation.X += delta * 80f;

                if (Player.Rotation.X < -89)
                    Player.Rotation.X = -89;
                else if (Player.Rotation.X > 89)
                    Player.Rotation.X = 89;
            }

            GUI.Update(delta);
            Player.Update(keyboardState, delta);
            DragAndDropHandler.Update();
            World.Update(delta);

            float FPS = 1f / delta;

            halfSecondUpdate += delta;
            if (halfSecondUpdate > 0.5f) {
                halfSecondUpdate = 0f;

                Title = $"Minecraft FPS: {SystemPlus.MathPlus.Round(FPS, 2)}";
            }

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(World.SkyColor);
            GL.DepthMask(true);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (GUI.Scene != 2) {
                shader.Bind();
                Camera.UpdateView(Width, Height);
                shader.UploadMat4("uProjection", ref Camera.projMatrix);
                shader.UploadMat4("uView", ref Camera.viewMatrix);
                World.Render(shader);

                GL.Enable(EnableCap.CullFace);

                texShader.Bind();
                texShader.UploadMat4("uProjection", ref Camera.projMatrix);
                texShader.UploadMat4("uView", ref Camera.viewMatrix);
                Player.Render(texShader);
            }

            GL.DepthMask(false);
            GUI.Render(uiShader);
            
            SwapBuffers();
        }

        public void JoinWorld()
        {
            Thread worldGenThread = new Thread(new ThreadStart(() =>
            {
                World.Generate();
            }));
            worldGenThread.Start();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            GUI.OnKeyPress(e.KeyChar);
        }
        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            keyboardState = e.Keyboard;
            if (keyboardState.IsKeyDown(Key.Escape) && GUI.Scene != 3) {
                UnlockMouse();
                if (GUI.Scene == 0)
                    GUI.SetScene(1);
            }
            GUI.OnKeyDown(e.Key, e.Modifiers);
            Player.OnKeyDown(e.Key, e.Modifiers);
            DragAndDropHandler.OnKeyDown(e.Key);
        }
        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            keyboardState = e.Keyboard;
            GUI.OnKeyUp(e.Key, e.Modifiers);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (!mouseLocked && (GUI.Scene == 1 || GUI.Scene == 2)) {
                LockMouse();
                if (GUI.Scene == 1)
                    GUI.SetScene(0);
            }
            else if (GUI.Scene == 0)
                Player.OnMouseDown(e.Button);

            GUI.OnMouseDown(e.Button, e.Position);
            DragAndDropHandler.OnMouseDown(e.Button, (Vector2i)e.Position);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            GUI.OnMouseUp(e.Button, e.Position);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            mousePos = new Vector2(e.Position.X, e.Position.Y);
        }

        // Mouse
        public void CenterCursor()
        {
            System.Windows.Forms.Cursor.Position = new Point(Width / 2 + Location.X, Height / 2 + Location.Y);
            lastMousePos = System.Windows.Forms.Cursor.Position;
        }
        public void LockMouse()
        {
            mouseLocked = true;
            origCursorPosition = System.Windows.Forms.Cursor.Position;
            CursorVisible = false;
            CenterCursor();
        }

        public void UnlockMouse()
        {
            mouseLocked = false;
            CursorVisible = true;
            System.Windows.Forms.Cursor.Position = origCursorPosition;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            Toolbar.MouseScrool(e.Delta);
        }

        protected override void OnClosed(EventArgs e)
        {
            Running = false;
        }
    }
}
