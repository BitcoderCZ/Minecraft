using Minecraft.Graphics;
using Minecraft.Graphics.UI;
using Minecraft.Math;
using Minecraft.VertexTypes;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemPlus;
using SystemPlus.Utils;

namespace Minecraft
{
    public static class Player
    {
        private const float walfSpeed = 5f;
        private const float sprintSpeed = 8f;
        private const float gravity = -9.8f * 1.5f;
        private const float jumpForce = 6f;

        private const float maxFallSpeed = -35f;

        private const float playerWidth = 0.25f;
        private const float playerHeight = 1.85f;
        private const float secondCheckHeight = 1.5f;

        private static bool isGrounded;

        public static Vector3 Position = Vector3.Zero;
        public static Vector3 Rotation;
        private static Vector3 velocity;
        private static float verticalMomentum;
        private static bool jumpRequest;

        private static float checkIncrement = 0.05f;
        private static float reach = 8f;
        private static uint selectedBlock = 1;

        private static RenderObject highlightblock;
        private static Vector3 placeBlock;

        static Player()
        {
            velocity = Vector3.Zero;

            Util.CreateCube(1.1f, out TexVertex[] verts, out uint[] tris);
            DirectBitmap db = new DirectBitmap(16, 16);
            db.Clear(Color.FromArgb(255 / 4, 255, 255, 255));
            Texture solidWhite = new Texture(db);
            highlightblock = new RenderObject(verts, tris, solidWhite.id);
            highlightblock.Active = false;

            placeBlock = -Vector3.One;
        }

        public static void SetRotation(Vector3 rot)
        {
            Rotation = rot;
        }

        public static void Move(float offset, float speed)
        {
            Vector3 move = new Vector3(0f, 0f, 1f) * speed;
            Matrix3 mat = Matrix3.CreateRotationY(((Rotation.Y + offset) * Util.PI) / 180f);
            Position += move * mat;
        }

        public static void Render(Shader s)
        {
            highlightblock.Render(s);
        }

        public static void Update(KeyboardState keyboardState, float delta) // todo implement sneak
        {
            PlaceCursorBlock();

            // Movement
            GetInputs(keyboardState, out float ver, out float hor);

            ver *= delta;
            hor *= delta;

            CalcVelocity(ver, hor, delta);
            if (jumpRequest)
                Jump();

            jumpRequest = false;

            Position += velocity;

            Console.Title = $"Player pos: {Position}, Player chunk: {World.prevPlayerChunk}, high: {highlightblock.Position}, scene: {GUI.Scene}";
            /*if (keyboardState.IsKeyDown(Key.W)) // flying
                Move(0f, delta * mult);
            else if (keyboardState.IsKeyDown(Key.S))
                Move(180f, delta * mult);
            if (keyboardState.IsKeyDown(Key.A))
                Move(90f, delta * mult);
            else if (keyboardState.IsKeyDown(Key.D))
                Move(270f, delta * mult);
            if (keyboardState.IsKeyDown(Key.Space)) 
                Position.Y += delta * 6f;
            else if (keyboardState.IsKeyDown(Key.ShiftLeft))
                Position.Y -= delta * 6f;*/
        }

        private static void Jump()
        {
            verticalMomentum = jumpForce;
            isGrounded = false;
            jumpRequest = false;
        }

        private static void CalcVelocity(float ver, float hor, float delta)
        {
            if (verticalMomentum > maxFallSpeed && !isGrounded)
                verticalMomentum += gravity * delta;

            // x, z
            Vector3 vForward = Vector3.UnitZ;
            Vector3 vRight = Vector3.UnitX;
            Matrix3 mat = Matrix3.CreateRotationY((Rotation.Y * Util.PI) / 180f);
            velocity = (vForward * ver + vRight * hor) * mat;

            // y
            velocity.Y += verticalMomentum * delta;

            if ((velocity.Z > 0f && front) || (velocity.Z < 0f && back))
                velocity.Z = 0;
            if ((velocity.X > 0f && right) || (velocity.X < 0f && left))
                velocity.X = 0;

            if (velocity.Y < 0f)
                velocity.Y = CheckDownSpeed(velocity.Y);
            else if (velocity.Y > 0f)
                velocity.Y = CheckUpSpeed(velocity.Y);
        }

        private static void GetInputs(KeyboardState keyboardState, out float ver, out float hor)
        {
            ver = 0f;
            hor = 0f;

            float mult = keyboardState.IsKeyDown(Key.LControl) ? sprintSpeed : walfSpeed;
            if (keyboardState.IsKeyDown(Key.W)) // flying
                ver = mult;
            else if (keyboardState.IsKeyDown(Key.S))
                ver = -mult;
            if (keyboardState.IsKeyDown(Key.A))
                hor = mult;
            else if (keyboardState.IsKeyDown(Key.D))
                hor = -mult;

            if (isGrounded && keyboardState.IsKeyDown(Key.Space))
                jumpRequest = true;
        }

        public static void MouseDown(MouseButton button)
        {
            if (button == MouseButton.Left && highlightblock.Active) {
                Vector3i block = (Vector3i)highlightblock.Position;
                block += Vector3i.One;
                World.GetChunkFromBlock(block).SetBlockGlobalPos(block, 0, true);
            } else if (button == MouseButton.Right && placeBlock != -Vector3.One) {
                Vector3 block = placeBlock;
                Vector3i v = new Vector3i(MathPlus.RoundToInt(block.X), MathPlus.RoundToInt(block.Y), MathPlus.RoundToInt(block.Z));
                if (v.Y >= VoxelData.ChunkHeight || v == (Vector3i)Position || v == (Vector3i)Position + new Vector3i(0, 1, 0))
                    return;
                World.GetChunkFromBlock(block).SetBlockGlobalPos(v, selectedBlock, true);
            }
        }

        public static void MouseScrool(int scrool)
        {
            if (scrool > 0)
                selectedBlock++;
            else
                selectedBlock--;

            if (selectedBlock >= World.blocktypes.Length)
                selectedBlock = 1;
            else if (selectedBlock < 1)
                selectedBlock = (uint)World.blocktypes.Length - 1;
        }

        private static void PlaceCursorBlock()
        {
            float step = checkIncrement;
            Vector3 lastPos = new Vector3();
            placeBlock = -Vector3i.One;

            while (step < reach) {
                Vector3 pos = Position + Camera.Offset + (Camera.Forward * step);

                if (World.CheckForBlock(pos)) {
                    highlightblock.Position = (Vector3i)pos - new Vector3(0.0625f, 0.0625f, 0.0625f);
                    placeBlock = (Vector3i)lastPos;
                    highlightblock.Active = true;
                    return;
                }

                lastPos = pos;

                step += checkIncrement;
            }

            highlightblock.Active = false;
            placeBlock = -Vector3i.One;
        }

        private static float CheckDownSpeed(float downSpeed)
        {
            if (World.CheckForBlock(Position.X - playerWidth, Position.Y + downSpeed, Position.Z - playerWidth) ||
                World.CheckForBlock(Position.X + playerWidth, Position.Y + downSpeed, Position.Z - playerWidth) ||
                World.CheckForBlock(Position.X - playerWidth, Position.Y + downSpeed, Position.Z + playerWidth) ||
                World.CheckForBlock(Position.X + playerWidth, Position.Y + downSpeed, Position.Z + playerWidth)) {
                isGrounded = true;
                verticalMomentum = verticalMomentum / 2f;
                return 0f;
            } else {
                isGrounded = false;
                return downSpeed;
            }
        }
        private static float CheckUpSpeed(float upSpeed)
        {
            if (World.CheckForBlock(Position.X - playerWidth, Position.Y + playerHeight + upSpeed, Position.Z - playerWidth) ||
                World.CheckForBlock(Position.X + playerWidth, Position.Y + playerHeight + upSpeed, Position.Z - playerWidth) ||
                World.CheckForBlock(Position.X - playerWidth, Position.Y + playerHeight + upSpeed, Position.Z + playerWidth) ||
                World.CheckForBlock(Position.X + playerWidth, Position.Y + playerHeight + upSpeed, Position.Z + playerWidth)) {
                verticalMomentum = -0.0000001f;
                return 0f;
            }
            else {
                return upSpeed;
            }
        }

        public static bool front
        {
            get {
                if (World.CheckForBlock(Position.X, Position.Y, Position.Z + playerWidth) ||
                    World.CheckForBlock(Position.X, Position.Y + secondCheckHeight, Position.Z + playerWidth))
                    return true;
                else
                    return false;
            }
        }
        public static bool back
        {
            get {
                if (World.CheckForBlock(Position.X, Position.Y, Position.Z - playerWidth) ||
                    World.CheckForBlock(Position.X, Position.Y + secondCheckHeight, Position.Z - playerWidth))
                    return true;
                else
                    return false;
            }
        }
        public static bool left
        {
            get {
                if (World.CheckForBlock(Position.X - playerWidth, Position.Y, Position.Z) ||
                    World.CheckForBlock(Position.X - playerWidth, Position.Y + secondCheckHeight, Position.Z))
                    return true;
                else
                    return false;
            }
        }
        public static bool right
        {
            get {
                if (World.CheckForBlock(Position.X + playerWidth, Position.Y, Position.Z) ||
                    World.CheckForBlock(Position.X + playerWidth, Position.Y + secondCheckHeight, Position.Z))
                    return true;
                else
                    return false;
            }
        }
    }
}
