using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.VertexTypes
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public const int Size = 10 * sizeof(float);

        public Vector3 position;
        public Vector3 uv;
        public Vector4 color;

        public Vertex(Vector3 _pos, Vector2 _uv, uint _id, Vector4 _color)
        {
            position = _pos;
            uv = new Vector3(_uv.X, _uv.Y, (float)_id - 1f);
            color = _color;
        }

        public Vertex(float x, float y, float z, float u, float v, uint _id, Vector4 _color)
        {
            position = new Vector3(x, y, z);
            uv = new Vector3(u, v, (float)_id - 1f);
            color = _color;
        }

        public Vertex(float x, float y, float z) : this(x, y, z, 0f, 0f, 1, Vector4.One)
        { }
    }
}
