using Minecraft.VertexTypes;
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

        public static void CreateCube(float scale, out TexVertex[] verts, out uint[] tris)
        {
            verts = new TexVertex[6 * 4];
            tris = new uint[6 * 6];

            uint vertIndex = 0;
            int triIndex = 0;
            for (int p = 0; p < 6; p++) {
                verts[vertIndex] = new TexVertex(VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]] * scale, VoxelData.voxelUvs[0]);
                verts[vertIndex + 1] = new TexVertex(VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]] * scale, VoxelData.voxelUvs[1]);
                verts[vertIndex + 2] = new TexVertex(VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]] * scale, VoxelData.voxelUvs[2]);
                verts[vertIndex + 3] = new TexVertex(VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]] * scale, VoxelData.voxelUvs[3]);

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
    }
}
