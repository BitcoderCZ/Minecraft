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

        public static float Get2DPerlinNoise(Vector2 pos, float offset, float scale)
        {
            return noise.cnoise(new float2(((pos.X + 0.1f) / VoxelData.ChunkWidth + xOff) * scale + offset,
                                            ((pos.Y + 0.1f) / VoxelData.ChunkWidth + yOff) * scale + offset));
        }

        public static void SetSeed(uint _seed)
        {
            seed = _seed;
            Random r = new Random(seed);
            float min = -10000f;
            float max = 10000f;
            xOff = r.NextFloat(min, max);
            yOff = r.NextFloat(min, max);
            /*xOff = 0;
            yOff = 0;*/
        }
    }
}
