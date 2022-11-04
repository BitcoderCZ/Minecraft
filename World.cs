using Minecraft.Math;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Minecraft
{
    public static class World
    {
        public static readonly BlockType[] blocktypes = new BlockType[]
        {
            new BlockType("air", false, 0), // 0
            new BlockType("stone", true, 1), // 1
            new BlockType("grass_block", true, 3, 3, 2, 4, 3, 3), // 2
            new BlockType("dirt", true, 4), // 3
            new BlockType("cobblestone", true, 5), // 4
            new BlockType("oak_planks", true, 6), // 5
            new BlockType("oak_sapling", false, 7), // 6
            new BlockType("bedrock", true, 8), // 7
            new BlockType("sand", true, 9), // 8
            new BlockType("granite", true, 10), // 9
            new BlockType("polished_granite", true, 11), // 10
            new BlockType("diorite", true, 12), // 11
            new BlockType("polished_diorite", true, 13), // 12
            new BlockType("andesite", true, 14), // 13
            new BlockType("polished_andesite", true, 15), // 14
        };
        public static readonly BiomeAttribs[] biomes = new BiomeAttribs[]
        {
            new BiomeAttribs("plains", 60, 14, 0.2f,new Lode("cave", 0, false, 5, 90, 0.08f, 0.42f, -100f),  new Lode("dirt", 3, false, 35, 65, 0.1f, 0.45f, 0f),
                                new Lode("granite", 9, false, 16, 55, 0.12f, 0.46f, 100f)),
        };
        public static uint seed;

        public static Vector3 spawnPos;

        private static Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
        private static List<Flat2i> activeChunks = new List<Flat2i>();
        private static Flat2i prevPlayerChunk;

        static World()
        {
            seed = 2147483647;
            Noise.SetSeed(seed);
            spawnPos = new Vector3(VoxelData.WorldSizeInBlocks / 2f, VoxelData.ChunkHeight + 2f, VoxelData.WorldSizeInBlocks / 2f);
        }

        public static void Update()
        {
            if (BlockToChunk(Player.Position) != prevPlayerChunk)
                CheckViewDistance();

            prevPlayerChunk = BlockToChunk(Player.Position);
        }

        public static void Generate()
        {
            Console.WriteLine("WORLD:GENERATE:START");

            for (int x = VoxelData.WorldSizeInChunks / 2 - VoxelData.RenderDistance; x < VoxelData.WorldSizeInChunks / 2 + VoxelData.RenderDistance; x++)
                for (int z = VoxelData.WorldSizeInChunks / 2 - VoxelData.RenderDistance; z < VoxelData.WorldSizeInChunks / 2 + VoxelData.RenderDistance; z++) {
                    if (IsChunkInWorld(x, z))
                        CreateNewChunk(x, z);
                }

            Player.Position = spawnPos + new Vector3(0.5f, 0.5f, 0.5f);
            Player.Position.Y = (int)(Noise.Get2DPerlinNoise(new Vector2(Player.Position.X, Player.Position.Z), 0f, biomes[0].terrainScale)
                * biomes[0].terrainHeight + biomes[0].minHeight) + 2.5f;
            prevPlayerChunk = BlockToChunk(Player.Position);

            Console.WriteLine("WORLD:GENERATE:DONE");
        }

        private static Flat2i BlockToChunk(Vector3i v)
            => new Flat2i(v.X / VoxelData.ChunkWidth, v.Z / VoxelData.ChunkWidth);
        private static Flat2i BlockToChunk(Vector3 v)
            => new Flat2i((int)v.X / VoxelData.ChunkWidth, (int)v.Z / VoxelData.ChunkWidth);

        private static void CheckViewDistance()
        {
            Flat2i playerChunk = BlockToChunk(Player.Position);

            List<Flat2i> prevActiveChunks = new List<Flat2i>(activeChunks);

            for (int x = playerChunk.X - VoxelData.RenderDistance; x < playerChunk.X + VoxelData.RenderDistance; x++)
                for (int z = playerChunk.Z - VoxelData.RenderDistance; z < playerChunk.Z + VoxelData.RenderDistance; z++) {
                    if (!IsChunkInWorld(x, z))
                        continue;

                    Flat2i chp = new Flat2i(x, z);

                    if (chunks[x, z] == null)
                        CreateNewChunk(x, z);
                    else if (!chunks[x, z].active) {
                        chunks[x, z].active = true;
                        activeChunks.Add(chp);
                    }

                    for (int i = 0; i < prevActiveChunks.Count; i++) {
                        if (prevActiveChunks[i] == chp) {
                            prevActiveChunks.RemoveAt(i);
                            break;
                        }
                    }
                }

            for (int i = 0; i < prevActiveChunks.Count; i++) {
                activeChunks.Remove(prevActiveChunks[i]);
                chunks[prevActiveChunks[i].X, prevActiveChunks[i].Z].active = false;
            }
        }

        public static bool CheckForBlock(float _x, float _y, float _z)
        {
            int x = (int)_x;
            int y = (int)_y;
            int z = (int)_z;

            int xChunk = x / VoxelData.ChunkWidth;
            int zChunk = z / VoxelData.ChunkWidth;

            x -= (xChunk * VoxelData.ChunkWidth);
            z -= (zChunk * VoxelData.ChunkWidth);

            return blocktypes[chunks[xChunk, zChunk].GetBlock(x, y, z)].isSolid;
        }

        public static uint GetGenBlock(int x, int y, int z)
        {
            // Immutable pass
            if (!IsBlockInWorld(x, y, z))
                return 0;

            // Bedrock
            if (y == 0)
                return 7;

            // Basic terrain pass
            int terrainHeight = (int)(Noise.Get2DPerlinNoise(new Vector2(x, z), 0f, biomes[0].terrainScale) * biomes[0].terrainHeight + biomes[0].minHeight);
            uint blockId;

            if (y == terrainHeight)
                blockId = 2;
            else if (y < terrainHeight && y >= terrainHeight - 3)
                blockId = 3;
            else if (y < terrainHeight)
                blockId = 1;
            else
                return 0;

            // second pass
            for (int i = 0; i < biomes[0].lodes.Length; i++) {
                Lode lode = biomes[0].lodes[i];
                if (!lode.reachTerrain && y >= terrainHeight - 3)
                    continue;
                if (y >= lode.minHeight && y <= lode.maxHeight) {
                    if (Noise.Get3DPerlin(new Vector3(x, y, z), lode.noiseOffset, lode.scale, lode.threshold))
                        return lode.blockID;
                }
            }
            

            return blockId;
        }
        public static uint GetGenBlock(Vector3i pos)
            => GetGenBlock(pos.X, pos.Y, pos.Z);

        private static void CreateNewChunk(int x, int z)
        {
            Chunk chunk = new Chunk(new Flat2i(x, z));
            chunks[x, z] = chunk;
            activeChunks.Add(new Flat2i(x, z));
            chunk.active = true;
        }

        private static bool IsChunkInWorld(int x, int z)
        {
            if (x >= 0 && x < VoxelData.WorldSizeInChunks && z >= 0 && z < VoxelData.WorldSizeInChunks)
                return true;
            else
                return false;
        }
        private static bool IsChunkInWorld(Flat2i pos)
            => IsChunkInWorld(pos.X, pos.Z);

        private static bool IsBlockInWorld(int x, int y, int z)
        {
            if (x >= 0 && x < VoxelData.WorldSizeInBlocks
                && y >= 0 && y < VoxelData.ChunkHeight
                && z >= 0 && z < VoxelData.WorldSizeInBlocks)
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
                    if (chunks[x, z] != null)
                        chunks[x, z].Render(s);
        }
    }
}
