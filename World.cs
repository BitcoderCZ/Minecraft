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
            new BlockType("sand", true, 13), // 
        };

        static Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

        public static void Generate()
        {
            Console.WriteLine("WORLD:GENERATE:START");
            for (int x = 0; x < VoxelData.WorldSizeInChunks; x++)
                for (int z = 0; z < VoxelData.WorldSizeInChunks; z++) {
                    Chunk chunk = Chunk.Create(new Vector2i(x, z));
                    chunks[x, z] = chunk;
                }
            Console.WriteLine("WORLD:GENERATE:DONE");
        }

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
