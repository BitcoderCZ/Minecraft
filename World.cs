﻿using Minecraft.Graphics;
using Minecraft.Graphics.UI;
using Minecraft.Math;
using OpenTK;
using System;
using System.Collections;
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
            new BlockType("air", false, 0, -1), // 0
            new BlockType("stone", true, 1, 0), // 1
            new BlockType("grass_block", true, 3, 3, 2, 4, 3, 3, 1), // 2
            new BlockType("dirt", true, 4, 2), // 3
            new BlockType("cobblestone", true, 5, 3), // 4
            new BlockType("oak_planks", true, 6, 4), // 5
            new BlockType("oak_sapling", false, 7, 5), // 6
            new BlockType("bedrock", true, 8, 6), // 7
            new BlockType("sand", true, 9, 7), // 8
            new BlockType("granite", true, 10, 8), // 9
            new BlockType("polished_granite", true, 11, -1), // 10
            new BlockType("diorite", true, 12, -1), // 11
            new BlockType("polished_diorite", true, 13, -1), // 12
            new BlockType("andesite", true, 14, -1), // 13
            new BlockType("polished_andesite", true, 15, -1), // 14
            new BlockType("oak_log", true, 16, 16, 17, 17, 16, 16, 9), // 15
            new BlockType("oak_leaves", true, 18, 10), // 16
        };
        public static readonly BiomeAttribs[] biomes = new BiomeAttribs[]
        {
            new BiomeAttribs("plains", 60, 14, 0.2f, 1.0f, 0.5f, 18f, 0.6f, new Lode("cave", 0, false, 5, 90, 0.08f, 0.42f, -100f), 
                new Lode("dirt", 3, false, 35, 65, 0.1f, 0.45f, 0f),
                                new Lode("granite", 9, false, 16, 55, 0.12f, 0.46f, 100f)),
        };
        public static uint seed;

        public static Vector3 spawnPos;

        private static Chunk[,] chunks = new Chunk[BlockData.WorldSizeInChunks, BlockData.WorldSizeInChunks];
        private static List<Flat2i> activeChunks = new List<Flat2i>();
        public static Flat2i prevPlayerChunk;

        private static List<Flat2i> chunksToCreate = new List<Flat2i>();

        private static Queue<Flat2i> chunksToUpdate = new Queue<Flat2i>();

        private static Queue<BlockMod> modifications = new Queue<BlockMod>();

        private static Thread createChunksThread;

        public static bool Generated { get; private set; }

        static World()
        {
            Generated = false;
            Random r = new Random(DateTime.Now.Second * DateTime.Now.Millisecond / DateTime.Now.Hour);
            seed = (uint)r.Next();
            Noise.SetSeed(seed);
            spawnPos = new Vector3(BlockData.WorldSizeInBlocks / 2f, BlockData.ChunkHeight + 2f, BlockData.WorldSizeInBlocks / 2f);
            createChunksThread = new Thread(new ThreadStart(CreateChunks));
            createChunksThread.Start();
        }

        public static void Update()
        {
            if (BlockToChunk(Player.Position) != prevPlayerChunk)
                CheckViewDistance();
        }

        private static void CreateChunks()
        {
            while (Program.Window.Running) {
                if (chunksToCreate.Count > 0) {
                    chunks[chunksToCreate[0].X, chunksToCreate[0].Z].Init();
                    chunksToCreate.RemoveAt(0);

                    while (modifications.Count > 0) {
                        BlockMod mod = modifications.Dequeue();
                        Flat2i cp = BlockToChunk(mod.Pos);

                        if (chunks[cp.X, cp.Z] == null) {
                            chunks[cp.X, cp.Z] = new Chunk(cp, true);
                            activeChunks.Add(cp);
                        }

                        chunks[cp.X, cp.Z].modifications.Enqueue(mod);

                        if (!chunksToUpdate.Contains(cp))
                            chunksToUpdate.Enqueue(cp);
                    }

                    while (chunksToUpdate.Count > 0) {
                        Flat2i cp = chunksToUpdate.Dequeue();
                        chunks[cp.X, cp.Z].UpdateMesh();
                    }
                }
            }
        }

        public static void Generate()
        {
            Console.WriteLine("WORLD:GENERATE:START");

            UIImage loadBar = (UIImage)GUI.elements[2];

            float max = 800f;
            float length = 0f;
            float step = (1f / ((float)BlockData.RenderDistance * (float)BlockData.RenderDistance)) * max;
            step /= 4f;

            for (int x = BlockData.WorldSizeInChunks / 2 - BlockData.RenderDistance; x < BlockData.WorldSizeInChunks / 2 + BlockData.RenderDistance; x++)
                for (int z = BlockData.WorldSizeInChunks / 2 - BlockData.RenderDistance; z < BlockData.WorldSizeInChunks / 2 + BlockData.RenderDistance; z++) {
                    if (IsChunkInWorld(x, z)) {
                        Flat2i pos = new Flat2i(x, z);
                        chunks[x, z] = new Chunk(pos, true);
                        activeChunks.Add(pos);

                        length += step;
                        loadBar.PixWidth = (int)length;
                    }
                }

            while (modifications.Count > 0) {
                BlockMod mod = modifications.Dequeue();
                Flat2i cp = BlockToChunk(mod.Pos);

                if (chunks[cp.X, cp.Z] == null) {
                    chunks[cp.X, cp.Z] = new Chunk(cp, true);
                    activeChunks.Add(cp);
                }

                chunks[cp.X, cp.Z].modifications.Enqueue(mod);

                if (!chunksToUpdate.Contains(cp))
                    chunksToUpdate.Enqueue(cp);
            }

            while (chunksToUpdate.Count > 0) {
                Flat2i cp = chunksToUpdate.Dequeue();
                chunks[cp.X, cp.Z].UpdateMesh();
            }

            Player.Position = spawnPos + new Vector3(0.5f, 0.5f, 0.5f);
            Player.Position.Y = (int)(Noise.Get2DPerlinNoise(new Vector2(Player.Position.X, Player.Position.Z), 0f, biomes[0].terrainScale)
                * biomes[0].terrainHeight + biomes[0].minHeight) + 2.5f;
            prevPlayerChunk = BlockToChunk(Player.Position);

            Console.WriteLine("WORLD:GENERATE:DONE");

            Generated = true;

            GUI.SetScene(0);
        }

        public static Flat2i BlockToChunk(Vector3i v)
            => new Flat2i(v.X / BlockData.ChunkWidth, v.Z / BlockData.ChunkWidth);
        public static Flat2i BlockToChunk(Vector3 v)
            => new Flat2i((int)v.X / BlockData.ChunkWidth, (int)v.Z / BlockData.ChunkWidth);

        public static Chunk GetChunkFromBlock(Vector3i v)
            => chunks[v.X / BlockData.ChunkWidth, v.Z / BlockData.ChunkWidth];
        public static Chunk GetChunkFromBlock(Vector3 v)
            => chunks[(int)v.X / BlockData.ChunkWidth, (int)v.Z / BlockData.ChunkWidth];

        private static void CheckViewDistance()
        {
            Flat2i playerChunk = BlockToChunk(Player.Position);

            prevPlayerChunk = BlockToChunk(Player.Position);

            List<Flat2i> prevActiveChunks = new List<Flat2i>(activeChunks);

            for (int x = playerChunk.X - BlockData.RenderDistance; x < playerChunk.X + BlockData.RenderDistance; x++)
                for (int z = playerChunk.Z - BlockData.RenderDistance; z < playerChunk.Z + BlockData.RenderDistance; z++) {
                    if (!IsChunkInWorld(x, z))
                        continue;

                    Flat2i chp = new Flat2i(x, z);

                    if (chunks[x, z] == null) {
                        chunks[x, z] = new Chunk(chp, false);
                        chunksToCreate.Add(new Flat2i(x, z));
                    }
                    else if (!chunks[x, z].Active) {
                        chunks[x, z].Active = true;
                    }
                    if (!activeChunks.Contains(chp))
                        activeChunks.Add(chp);

                    for (int i = 0; i < prevActiveChunks.Count; i++) {
                        if (prevActiveChunks[i] == chp) {
                            prevActiveChunks.RemoveAt(i);
                        }
                    }
                }

            for (int i = 0; i < prevActiveChunks.Count; i++) {
                chunks[prevActiveChunks[i].X, prevActiveChunks[i].Z].Active = false;
            }
        }

        public static bool CheckForBlock(Vector3 pos)
        {
            Flat2i chunk = Flat2i.FromBlock(pos);
            Vector3i iPos = (Vector3i)pos;

            if (!IsBlockInWorld(iPos) || iPos.Y < 0 || iPos.Y > BlockData.ChunkHeight)
                return false;

            if (chunks[chunk.X, chunk.Z] != null && chunks[chunk.X, chunk.Z].BlocksGenerated)
                return blocktypes[chunks[chunk.X, chunk.Z].GetBlockGlobalPos(iPos)].isSolid;

            return blocktypes[GetGenBlock(iPos)].isSolid;
        }
        public static bool CheckForBlock(float x, float y, float z)
            => CheckForBlock(new Vector3(x, y, z));

        public static uint GetGenBlock(int x, int y, int z)
        {
            // Immutable pass
            if (!IsBlockInWorld(x, y, z))
                return 0;

            // Bedrock
            if (y == 0)
                return 7;

            Vector2 vec2 = new Vector2(x, z);

            // Basic terrain pass
            int terrainHeight = (int)(Noise.Get2DPerlinNoise(vec2, 0f, biomes[0].terrainScale) * biomes[0].terrainHeight + biomes[0].minHeight);
            uint blockId;

            if (y == terrainHeight)
                blockId = 2;
            else if (y < terrainHeight && y >= terrainHeight - 3)
                blockId = 3;
            else if (y < terrainHeight)
                blockId = 1;
            else
                return 0;

            // Second pass
            for (int i = 0; i < biomes[0].lodes.Length; i++) {
                Lode lode = biomes[0].lodes[i];
                if (!lode.reachTerrain && y >= terrainHeight - 3)
                    continue;
                if (y >= lode.minHeight && y <= lode.maxHeight) {
                    if (Noise.Get3DPerlin(new Vector3(x, y, z), lode.noiseOffset, lode.scale, lode.threshold))
                        return lode.blockID;
                }
            }

            // Tree pass
            if (y == terrainHeight) {
                if (Noise.Get2DPerlinNoise(vec2, -700f, biomes[0].treeZoneScale) > biomes[0].treeZoneThreashold) {
                    blockId = 7;
                    if (Noise.Get2DPerlinNoise(vec2, 1200f, biomes[0].treePlacementScale) > biomes[0].treePlacementThreashold) {
                        blockId = 1;
                        Structure.MakeTree(new Vector3i(x, y, z), modifications, 5, 9);
                    }
                }
            }

            return blockId;
        }
        public static uint GetGenBlock(Vector3i pos)
            => GetGenBlock(pos.X, pos.Y, pos.Z);

        private static bool IsChunkInWorld(int x, int z)
        {
            if (x >= 0 && x < BlockData.WorldSizeInChunks && z >= 0 && z < BlockData.WorldSizeInChunks)
                return true;
            else
                return false;
        }
        private static bool IsChunkInWorld(Flat2i pos)
            => IsChunkInWorld(pos.X, pos.Z);

        private static bool IsBlockInWorld(int x, int y, int z)
        {
            if (x >= 0 && x < BlockData.WorldSizeInBlocks
                && y >= 0 && y < BlockData.ChunkHeight
                && z >= 0 && z < BlockData.WorldSizeInBlocks)
                return true;
            else
                return false;
        }
        private static bool IsBlockInWorld(Vector3i pos)
         => IsBlockInWorld(pos.X, pos.Y, pos.Z);

        public static void Render(Shader s)
        {
            for (int x = 0; x < BlockData.WorldSizeInChunks; x++)
                for (int z = 0; z < BlockData.WorldSizeInChunks; z++)
                    if (chunks[x, z] != null)
                        chunks[x, z].Render(s);
        }
    }
}
