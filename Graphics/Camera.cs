using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.Graphics
{
    public static class Camera // Eye level - 1.62, height - 1.85, width - 0.925
    {
        public static Matrix4 viewMatrix;
        public static Matrix4 projMatrix;
        private static Vector3 center = Vector3.Zero;
        private static readonly Vector3 up = Vector3.UnitY;
        public static bool ortho = false;

        public static Vector3 Offset = new Vector3(0f, 1.7f, 0f);

        public static Vector3 Forward
        {
            get {
                Player.Rotation.Y = mod(Player.Rotation.Y, 360);
                float x = (Player.Rotation.X * Util.PI) / 180f;
                float y = (Player.Rotation.Y * Util.PI) / 180f;
                Matrix3 mat = Matrix3.CreateRotationX(x) * Matrix3.CreateRotationY(y);
                return Vector3.UnitZ * mat;
            }
        }

        private static float mod(float x, float m)
        {
            float r = x % m;
            return r < 0 ? r + m : r;
        }

        public static void UpdateView(float width, float height)
        {
            Player.Rotation.Y = mod(Player.Rotation.Y, 360);
            float x = (Player.Rotation.X * Util.PI) / 180f;
            float y = (Player.Rotation.Y * Util.PI) / 180f;
            Vector3 offset = new Vector3(0f, 0f, 1f);
            Matrix3 mat = Matrix3.CreateRotationX(x) * Matrix3.CreateRotationY(y);
            Vector3 pos = Player.Position + Offset;
            center = pos + (offset * mat);
            
            viewMatrix = Matrix4.LookAt(pos, center, up);

            if (ortho) {
                float projWidth = width;
                float aspect = width / height;
                float projHeight = width / aspect;

                float left = -projWidth / 2f;
                float right = projWidth / 2f;
                float bottom = -projHeight / 2f;
                float top = projHeight / 2f;
                float near = 0.01f;
                float far = 100f;

                projMatrix = Matrix4.CreateOrthographicOffCenter(left, right, bottom, top, near, far);
            } else {
                float fov = 45f;
                float near = 0.0001f;
                float far = 10000f;
                float aspect = width / height;
                 projMatrix = Matrix4.CreatePerspectiveFieldOfView(DegToRad(fov), aspect, near, far);
            }
        }

        const float PI = (float)System.Math.PI;
        const float PIDiv = PI / 180f;

        static float DegToRad(float degrees)
        {
            float radians = PIDiv * degrees;
            return (radians);
        }
    }
}
