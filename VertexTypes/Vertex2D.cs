using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.VertexTypes
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex2D
    {
        public const int Size = 4 * sizeof(float);

        public Vector2 position;
        public Vector2 uv;

        public Vertex2D(Vector2 _pos, Vector2 _uv)
        {
            position = _pos;
            uv = _uv;
        }

        public Vertex2D(float x, float y, float u, float v)
        {
            position = new Vector2(x, y);
            uv = new Vector2(u, v);
        }

        public Vertex2D(float x, float y) : this(x, y, 0f, 0f)
        { }
    }
}
