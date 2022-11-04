using Minecraft.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public static class World
    {
        public static BlockType[] blocktypes = new BlockType[]
        {
            new BlockType("air", false, 0), // 0
            new BlockType("stone", true, 1), // 1
            new BlockType("grass_block", true, 3, 3, 2, 4, 3, 3), // 2
            new BlockType("dirt", true, 4), // 3
            new BlockType("cobblestone", true, 5), // 4
            new BlockType("oak_planks", true, 6), // 5
            new BlockType("oak_sapling", false, 7), // 6
            new BlockType("bedrock", true, 8), // add water
            //new BlockType("sand", true, 13), // 
        };

        static Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

        public static void Generate()
        {
            Console.WriteLine("WORLD:GENERATE:START");
            for (int x = 0; x < VoxelData.WorldSizeInChunks; x++)
                for (int z = 0; z < VoxelData.WorldSizeInChunks; z++) {
                    CreateNewChunk(x, z);
                }
            Console.WriteLine("WORLD:GENERATE:DONE");
        }

        public static uint GetGenBlock(int x, int y, int z)
        {
            if (!IsBlockInWorld(x, y, z))
                return 0;
            if (y == 0)
                return 7;
            else if (y == VoxelData.ChunkHeight - 1)
                return 2;
            else
                return 3;
        }
        public static uint GetGenBlock(Vector3i pos)
            => GetGenBlock(pos.X, pos.Y, pos.Z);

        private static void CreateNewChunk(int x, int z)
        {
            Chunk chunk = new Chunk(new Flat2i(x, z));
            chunks[x, z] = chunk;
        }

        private static bool IsChunkInWorld(Vector2i pos)
        {
            if (pos.X > 0 && pos.X < VoxelData.WorldSizeInChunks && pos.Y > 0 && pos.Y < VoxelData.WorldSizeInChunks)
                return true;
            else
                return false;
        }

        private static bool IsBlockInWorld(int x, int y, int z)
        {
            if (x > 0 && x < VoxelData.WorldSizeInBlocks
                && y > 0 && y < VoxelData.ChunkHeight
                && z > 0 && z < VoxelData.WorldSizeInBlocks)
                return true;
            else
                return false;
        }
        private static bool IsBlockInWorld(Vector3i pos)
         => IsBlockInWorld(pos.X, pos.Y, pos.Z);

        public static void Render(Shader s)
        {
            for (int x = 0; x < VoxelData.WorldSizeInChunks; x++)
                for (int z = 0; z < VoxelData.WorldSizeInChunks; z++)
                    chunks[x, z].Render(s);
        }
    }

    public class BlockType
    {
        public string blockName;
        public bool isSolid;

        public uint backFaceTexture;
        public uint frontFaceTexture;
        public uint topFaceTexture;
        public uint bottomFaceTexture;
        public uint leftFaceTexture;
        public uint rightFaceTexture;

        public BlockType(string name, bool solid, uint back, uint front, uint top, uint bottom, uint left, uint right)
        {
            blockName = name;
            isSolid = solid;
            backFaceTexture = back;
            frontFaceTexture = front;
            topFaceTexture = top;
            bottomFaceTexture = bottom;
            leftFaceTexture = left;
            rightFaceTexture = right;
        }

        public BlockType(string name, bool solid, uint tex) : this(name, solid, tex, tex, tex, tex, tex, tex)
        { }

        public uint GetTextureID(int faceIndex)
        {
            switch (faceIndex) {
                case 0:
                    return backFaceTexture;
                case 1:
                    return frontFaceTexture;
                case 2:
                    return topFaceTexture;
                case 3:
                    return bottomFaceTexture;
                case 4:
                    return leftFaceTexture;
                case 5:
                    return rightFaceTexture;
                default:
                    Console.WriteLine($"Error in GetTextureID; invalid face index {faceIndex}");
                    return 0;
            }
        }
    }
}
