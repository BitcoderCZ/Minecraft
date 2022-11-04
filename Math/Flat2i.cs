using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.Math
{
    public struct Flat2i
    {
        public static readonly Flat2i Zero = new Flat2i(0, 0);
        public static readonly Flat2i One = new Flat2i(1, 1);

        public int X;
        public int Z;

        public Flat2i(int _X, int _Z)
        {
            X = _X;
            Z = _Z;
        }

        public static Flat2i FromBlock(Vector3 pos)
            => new Flat2i((int)pos.X / VoxelData.ChunkWidth, (int)pos.Z / VoxelData.ChunkWidth);
        public static Flat2i FromBlock(Vector3i pos)
            => new Flat2i(pos.X / VoxelData.ChunkWidth, pos.Z / VoxelData.ChunkWidth);

        public static Flat2i operator +(Flat2i a, Flat2i b)
            => new Flat2i(a.X + b.X, a.Z + b.Z);
        public static Flat2i operator -(Flat2i a, Flat2i b)
            => new Flat2i(a.X - b.X, a.Z - b.Z);
        public static Flat2i operator *(Flat2i a, Flat2i b)
            => new Flat2i(a.X * b.X, a.Z * b.Z);
        public static Flat2i operator /(Flat2i a, Flat2i b)
            => new Flat2i(a.X / b.X, a.Z / b.Z);

        public static Flat2i operator *(Flat2i a, int b)
            => new Flat2i(a.X * b, a.Z * b);
        public static Flat2i operator /(Flat2i a, int b)
            => new Flat2i(a.X / b, a.Z / b);
        public static Vector2 operator *(Flat2i a, float b)
            => new Vector2(a.X * b, a.Z * b);
        public static Vector2 operator /(Flat2i a, float b)
            => new Vector2(a.X / b, a.Z / b);

        public static Flat2i operator +(Flat2i a)
            => a;
        public static Flat2i operator -(Flat2i a)
            => new Flat2i(-a.X, -a.Z);

        public static implicit operator Vector2(Flat2i a)
            => new Vector2((float)a.X, (float)a.Z);
        public static explicit operator Flat2i(Vector2 a)
            => new Flat2i((int)a.X, (int)a.Y);

        public static bool operator ==(Flat2i a, Flat2i b)
            => a.X == b.X && a.Z == b.Z;
        public static bool operator !=(Flat2i a, Flat2i b)
            => a.X != b.X || a.Z != b.Z;

        public override string ToString() => $"({X}, {Z})";

        public override bool Equals(object obj)
        {
            if (obj is Flat2i v)
                return this == v;
            else
                return false;
        }

        public override int GetHashCode() => X ^ Z;
    }
}
