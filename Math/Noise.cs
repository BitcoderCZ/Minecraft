using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;

using Random = Unity.Mathematics.Random;

namespace Minecraft.Math
{
    public static class Noise
    {
        public static uint seed;

        private static float xOff;
        private static float yOff;
        private static float zOff;

        public static float Get2DPerlinNoise(Vector2 pos, float offset, float scale)
        {
            return noise.cnoise(new float2(((pos.X + 0.1f) / VoxelData.ChunkWidth + xOff) * scale + offset,
                                            ((pos.Y + 0.1f) / VoxelData.ChunkWidth + yOff) * scale + offset));
        }

        public static bool Get3DPerlin(Vector3 pos, float offset, float scale, float threshold)
        {
            float x = (pos.X + offset + 0.1f + xOff) * scale;
            float y = (pos.Y + offset + 0.1f + yOff) * scale;
            float z = (pos.Z + offset + 0.1f + zOff) * scale;
            return noise.cnoise(new float3(x, y, z)) > threshold;
        }

        public static void SetSeed(uint _seed)
        {
            seed = _seed;
            Random r = new Random(seed);
            float min = -100000f;
            float max = 100000f;
            xOff = r.NextFloat(min, max);
            yOff = r.NextFloat(min, max);
            zOff = r.NextFloat(min, max);
        }
    }
}
