using Minecraft.Math;
using OpenTK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public static class Structure
    {
        private static readonly object addToQueueLock = new object();

        public static void GenerateMajorFlora(int index, ConcurrentQueue<BlockMod> queue, Vector3i position,
            int minHeight, int maxHeight)
        {
            switch (index) {
                case 0:
                    MakeTree(queue, position, minHeight, maxHeight);
                    return;
                case 1:
                    MakeCactus(queue, position, minHeight, maxHeight);
                    return;
                default:
                    return;
            }
        }

        public static void MakeCactus(ConcurrentQueue<BlockMod> queue, Vector3i position, int minHeight, int maxHeight)
        {
            int height = (int)((maxHeight - minHeight) * Noise.Get2DPerlinNoise(new Vector2(position.X, position.Z), -120f, 1f)) + minHeight;

            if (height < minHeight)
                height = minHeight;

            lock (addToQueueLock) {
                for (int i = 1; i < height; i++)
                    queue.Enqueue(new BlockMod(new Vector3i(position.X, position.Y + i, position.Z), 18));

                queue.Enqueue(new BlockMod(new Vector3i(position.X, position.Y + height, position.Z), 19));
            }
        }

        public static void MakeTree(ConcurrentQueue<BlockMod> queue, Vector3i position, int minHeight, int maxHeight)
        {
            int height = (int)((maxHeight - minHeight) * Noise.Get2DPerlinNoise(new Vector2(position.X, position.Z), 120f, 1f)) + minHeight;

            if (height < minHeight)
                height = minHeight;

            lock (addToQueueLock) {
                for (int i = 1; i < height; i++)
                    queue.Enqueue(new BlockMod(new Vector3i(position.X, position.Y + i, position.Z), 15));

                for (int x = -2; x <= 2; x++)
                    for (int y = height - 2; y <= height + 1; y++)
                        for (int z = -2; z <= 2; z++)
                            if ((x != 0 || z != 0) || y >= height) {
                                Vector3i pos = new Vector3i(position.X + x, position.Y + y, position.Z + z);

                                if (queue.Where((BlockMod bm) => bm.Pos == pos && bm.Id == 15).Count() == 0)
                                    queue.Enqueue(new BlockMod(pos, 16));
                            }
            }
        }
    }
}
