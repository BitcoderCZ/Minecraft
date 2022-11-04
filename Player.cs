using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public static class Player
    {
        public const float walfSpeed = 4f;
        public const float sprintSpeed = 6f;
        public const float gravity = -9.8f * 1.5f;
        public const float jumpForce = 6f;

        public const float maxFallSpeed = -15f;

        public const float playerWidth = 0.25f;
        public const float playerHeight = 1.85f;

        public static bool isGrounded;

        public static Vector3 Position = Vector3.Zero;
        public static Vector3 Rotation;
        private static Vector3 velocity;
        public static float verticalMomentum;
        public static bool jumpRequest;

        static Player()
        {
            velocity = Vector3.Zero;
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

        public static void Update(KeyboardState keyboardState, float delta)
        {
            // Movement
            GetInputs(keyboardState, out float ver, out float hor);

            ver *= delta;
            hor *= delta;

            CalcVelocity(ver, hor, delta);
            if (jumpRequest)
                Jump();

            jumpRequest = false;

            Position += velocity;
            Console.Title = isGrounded.ToString() + "  " + velocity.ToString();
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

        private static float CheckDownSpeed(float downSpeed)
        {
            if (World.CheckForBlock(Position.X - playerWidth, Position.Y + downSpeed, Position.Z - playerWidth) ||
                World.CheckForBlock(Position.X + playerWidth, Position.Y + downSpeed, Position.Z - playerWidth) ||
                World.CheckForBlock(Position.X - playerWidth, Position.Y + downSpeed, Position.Z + playerWidth) ||
                World.CheckForBlock(Position.X + playerWidth, Position.Y + downSpeed, Position.Z + playerWidth)) {
                isGrounded = true;
                verticalMomentum = -0.0001f;
                return 0;
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
                return 0;
            }
            else {
                return upSpeed;
            }
        }

        public static bool front
        {
            get {
                if (World.CheckForBlock(Position.X, Position.Y, Position.Z + playerWidth) ||
                    World.CheckForBlock(Position.X, Position.Y + 1f, Position.Z + playerWidth))
                    return true;
                else
                    return false;
            }
        }
        public static bool back
        {
            get {
                if (World.CheckForBlock(Position.X, Position.Y, Position.Z - playerWidth) ||
                    World.CheckForBlock(Position.X, Position.Y + 1f, Position.Z - playerWidth))
                    return true;
                else
                    return false;
            }
        }
        public static bool left
        {
            get {
                if (World.CheckForBlock(Position.X - playerWidth, Position.Y, Position.Z) ||
                    World.CheckForBlock(Position.X - playerWidth, Position.Y + 1f, Position.Z))
                    return true;
                else
                    return false;
            }
        }
        public static bool right
        {
            get {
                if (World.CheckForBlock(Position.X + playerWidth, Position.Y, Position.Z) ||
                    World.CheckForBlock(Position.X + playerWidth, Position.Y + 1f, Position.Z))
                    return true;
                else
                    return false;
            }
        }
    }
}
