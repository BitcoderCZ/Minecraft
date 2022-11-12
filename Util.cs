using Minecraft.Math;
using Minecraft.VertexTypes;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public static class Util
    {
        public const float PI = (float)System.Math.PI;

        public static float Width = 1280f;
        public static float Height = 720f;

        public static void CreateCube(float scale, out TexVertex[] verts, out uint[] tris)
        {
            verts = new TexVertex[6 * 4];
            tris = new uint[6 * 6];

            uint vertIndex = 0;
            int triIndex = 0;
            for (int p = 0; p < 6; p++) {
                verts[vertIndex] = new TexVertex(BlockData.blockVerts[BlockData.blockTris[p, 0]] * scale, BlockData.voxelUvs[0]);
                verts[vertIndex + 1] = new TexVertex(BlockData.blockVerts[BlockData.blockTris[p, 1]] * scale, BlockData.voxelUvs[1]);
                verts[vertIndex + 2] = new TexVertex(BlockData.blockVerts[BlockData.blockTris[p, 2]] * scale, BlockData.voxelUvs[2]);
                verts[vertIndex + 3] = new TexVertex(BlockData.blockVerts[BlockData.blockTris[p, 3]] * scale, BlockData.voxelUvs[3]);

                tris[triIndex] = vertIndex;
                tris[triIndex + 1] = vertIndex + 1;
                tris[triIndex + 2] = vertIndex + 2;
                tris[triIndex + 3] = vertIndex + 2;
                tris[triIndex + 4] = vertIndex + 1;
                tris[triIndex + 5] = vertIndex + 3;
                vertIndex += 4;
                triIndex += 6;
            }
        }

        public static Vector2 NormalToGL(float x, float y)
            => new Vector2((x * 2f) - 1f, (y * 2f) - 1f);
        public static Vector2 NormalToGL(Vector2 pos)
            => NormalToGL(pos.X, pos.Y);

        public static Vector2 GLToNormal(float x, float y)
            => new Vector2((x + 1f) / 2f, (y + 1f) / 2f);
        public static Vector2 GLToNormal(Vector2 pos)
            => GLToNormal(pos.X, pos.Y);

        public static Vector2 PixelToNormal(int x, int y)
            => new Vector2((float)x / Width, (float)y / Height);
        public static Vector2 PixelToNormal(Vector2i pos)
            => PixelToNormal(pos.X, pos.Y);
        public static Vector2 PixelToGL(int x, int y)
            => new Vector2(((float)x / Width) * 2f - 1f, ((float)y / Height) * 2f - 1f);
        public static Vector2 PixelToGL(Vector2i pos)
            => PixelToGL(pos.X, pos.Y);
    }
}
