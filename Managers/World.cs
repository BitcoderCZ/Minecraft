using Minecraft.Graphics;
using Minecraft.Graphics.UI;
using Minecraft.Math;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Minecraft.Managers
{
    public static class World
    {
        public static Settings Settings;

        public static readonly BlockType[] blocktypes = new BlockType[]
        {
            new BlockType("air", false, true, 1f, 0, -1), // 0
            new BlockType("stone", true, false, 0f, 1, 0), // 1
            new BlockType("grass_block", true, false, 0f, 3, 3, 2, 4, 3, 3, 1), // 2
            new BlockType("dirt", true, false, 0f, 4, 2), // 3
            new BlockType("cobblestone", true, false, 0f, 5, 3), // 4
            new BlockType("oak_planks", true, false, 0f, 6, 4), // 5
            new BlockType("oak_sapling", false, true, 1f, 7, 5), // 6
            new BlockType("bedrock", true, false, 0f, 8, 6), // 7
            new BlockType("sand", true, false, 0f, 9, 7), // 8
            new BlockType("granite", true, false, 0f, 10, 8), // 9
            new BlockType("polished_granite", true, false, 0f, 11, 9), // 10
            new BlockType("diorite", true, false, 0f, 12, 10), // 11
            new BlockType("polished_diorite", false, true, 0f, 13, 11), // 12
            new BlockType("andesite", true, false, 0f, 14, 12), // 13
            new BlockType("polished_andesite", false, true, 0f, 15, 13), // 14
            new BlockType("oak_log", true, false, 0f, 16, 16, 17, 17, 16, 16, 14), // 15
            new BlockType("oak_leaves", true, true, 0.8f, 18, 15), // 16
            new BlockType("glass_block", true, true, 1f, 19, 16), // 17
            new BlockType("cactus", true, false, 0f, 21, 21, 20, 20, 21, 21, 17), // 18
            new BlockType("cactus_top", true, false, 0f, 21, 21, 22, 20, 21, 21, 17), // 19
        };
        public static readonly BiomeAttribs[] biomes = new BiomeAttribs[]
        {
            new BiomeAttribs("Forest", 8752, 0.17f, 45, 0.3f, 2, 3, 0, 5, 9, 0.4f, 0.2f, 18f, 0.65f, 
                new Lode("cave", 0, false, 5, 90, 0.08f, 0.42f, -100f), new Lode("dirt", 3, false, 35, 65, 0.1f, 0.45f, 0f),
                new Lode("granite", 9, false, 16, 55, 0.12f, 0.46f, 100f)),
            new BiomeAttribs("Grass Lands", 1451, 0.042f, 30, 0.15f, 2, 3, -1, 0, 0, 0f, 0f, 0f, 0f, 
                new Lode("cave", 0, true, 5, 64, 0.08f, 0.42f, -100f), new Lode("dirt", 3, false, 35, 56, 0.12f, 0.45f, 600f),
                new Lode("granite", 9, false, 16, 56, 0.11f, 0.46f, 200f)),
            new BiomeAttribs("Desert", 6524, 0.058f, 16, 0.05f, 8, 8, 1, 3, 6, 0.4f, 0.1f, 18f, 0.75f,
                new Lode("cave", 0, false, 5, 54, 0.08f, 0.42f, -100f), new Lode("dirt", 3, false, 35, 54, 0.08f, 0.45f, 200f),
                new Lode("granite", 9, false, 16, 54, 0.13f, 0.46f, -720f)),
        };
        public static uint seed;

        public static Vector3 spawnPos;

        private static Chunk[,] chunks = new Chunk[BlockData.WorldSizeInChunks, BlockData.WorldSizeInChunks];
        private static List<Flat2i> activeChunks = new List<Flat2i>();
        public static Flat2i prevPlayerChunk;

        public static List<Flat2i> chunksToCreate = new List<Flat2i>();

        public static List<Flat2i> chunksToUpdate = new List<Flat2i>();

        public static ConcurrentQueue<BlockMod> modifications = new ConcurrentQueue<BlockMod>();

        private static Thread createChunksThread;
        private static Thread applyModificationsThread;

        public static bool Generated { get; private set; }

        public static bool InUI { get => GUI.Scene != 0; }

        private static int dispPlace = -1;

        public static float globalLight = 1.0f;
        private static Vector4 dayColor = new Vector4(0f, 1f, 0.98f, 1f);
        private static Vector4 nightColor = new Vector4(0f, 0f, 0.25f, 1f);
        public static Color4 SkyColor = Color.Cyan;

        public static void LoadSettings()
        {
            string settingsPath = Environment.CurrentDirectory + "/Data/Settings.json";
            if (!File.Exists(settingsPath))
                Settings = new Settings();
            else
                Settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(settingsPath));

            SaveSettings();
        }

        public static void SaveSettings()
        {
            string settingsPath = Environment.CurrentDirectory + "/Data/Settings.json";
            File.WriteAllBytes(settingsPath, JsonSerializer.SerializeToUtf8Bytes(Settings));
        }

        public static void Init()
        {
            Generated = false;
            Random r = new Random(DateTime.Now.Second * DateTime.Now.Millisecond / DateTime.Now.Hour);
            spawnPos = new Vector3(BlockData.WorldCenter, BlockData.ChunkHeight + 2f, BlockData.WorldCenter);

            LoadSettings();

            if (Settings.Seed == -1) {
                seed = (uint)r.Next();
                Noise.SetSeed(seed);
            } else {
                r = new Random(Settings.Seed);
                seed = (uint)r.Next();
                Noise.SetSeed(seed);
            }

            createChunksThread = new Thread(new ThreadStart(CreateChunks));
            createChunksThread.Start();

            applyModificationsThread = new Thread(new ThreadStart(ApplyModifications));
            applyModificationsThread.Start();
        }

        public static void Update(float delta)
        {
            if (dispPlace == -1)
                dispPlace = Console.CursorTop;
            int before = Console.CursorTop;
            Console.SetCursorPosition(0, dispPlace);
            Console.Write($"Create: {chunksToCreate.Count}, Update: {chunksToUpdate.Count}, Modify: {modifications.Count}, Scene: {GUI.Scene}        ");
            Console.SetCursorPosition(0, before);

            Vector4 lerp =  Vector4.Lerp(dayColor, nightColor, 1f - globalLight);
            SkyColor = new Color4(lerp.X, lerp.Y, lerp.Z, lerp.W);

            if (InUI)
                return;

            if (BlockToChunk(Player.Position) != prevPlayerChunk)
                CheckViewDistance();

            if (chunksToUpdate.Count > 0)
                UpdateChunks();

            for (int i = 0; i < activeChunks.Count; i++) {
                chunks[activeChunks[i].X, activeChunks[i].Z].Update(delta);
            }
        }

        private static void CreateChunk()
        {
            Flat2i pos = chunksToCreate[0];
            chunksToCreate.RemoveAt(0);
            if (!activeChunks.Contains(pos))
                activeChunks.Add(pos);
            chunks[pos.X, pos.Z].Init();
        }

        private static void UpdateChunks()
        {
            bool updated = false;
            int index = 0;

            while (!updated && index < chunksToUpdate.Count) {
                Flat2i pos = chunksToUpdate[index];
                if (chunks[pos.X, pos.Z].BlocksGenerated && chunks[pos.X, pos.Z].CreatedMesh != 0) {
                    chunks[pos.X, pos.Z].UpdateMesh();
                    chunksToUpdate.RemoveAt(index);
                    updated = true;
                }
                else
                    index++;
            }
        }

        private static void ApplyModifications()
        {
            while (Program.Window.Running) {
                if (!Generated) {
                    Thread.Sleep(10);
                    continue;
                }
                int count = 0;
                while (modifications.Count > 0) {
                    BlockMod mod;
                    while (!modifications.TryDequeue(out mod)) { }
                    Flat2i cp = BlockToChunk(mod.Pos);

                    if (chunks[cp.X, cp.Z] == null && !chunksToCreate.Contains(cp)) {
                        chunks[cp.X, cp.Z] = new Chunk(cp, false);
                        chunksToCreate.Add(cp);
                        modifications.Enqueue(mod);
                        continue;
                    }

                    chunks[cp.X, cp.Z].modifications.Enqueue(mod);

                    if (!chunksToUpdate.Contains(cp))
                        chunksToUpdate.Add(cp);

                    if (count > 100)
                        break;

                    count++;
                }
                Thread.Sleep(0);
            }
        }

        private static void CreateChunks()
        {
            while (Program.Window.Running) {
                if (chunksToCreate.Count > 0)
                    CreateChunk();

                Thread.Sleep(0);
            }
        }

        public static void Generate()
        {
            Console.WriteLine("WORLD:GENERATE:START");

            UIImage loadBar = (UIImage)GUI.elements[2];
            UIImage smallLoadBar = (UIImage)GUI.elements[5];

            float max = 800f;
            float length = 0f;
            float step = (1f / (((float)Settings.RenderDistance + 1f) * ((float)Settings.RenderDistance) + 1f)) * max;
            step /= 4f;

            float smallMax = 600f;
            float smallStep = (1f / 2f) * smallMax;
            float smallLength = 0f;

            for (int x = BlockData.WorldSizeInChunks / 2 - Settings.RenderDistance; x <= BlockData.WorldSizeInChunks / 2 + Settings.RenderDistance; x++)
                for (int z = BlockData.WorldSizeInChunks / 2 - Settings.RenderDistance; z <= BlockData.WorldSizeInChunks / 2 + Settings.RenderDistance; z++) {
                    if (IsChunkInWorld(x, z)) {
                        Flat2i pos = new Flat2i(x, z);
                        chunks[x, z] = new Chunk(pos, true);
                        activeChunks.Add(pos);

                        length += step;
                        loadBar.PixWidth = (int)length;
                    }
                }

            smallLength += smallStep;
            smallLoadBar.PixWidth = (int)smallLength;

            step = (1f / modifications.Count) * max;
            length = 0f;
            loadBar.PixWidth = (int)length;

            int modStartCount = modifications.Count;
            for (int i = 0; i < modStartCount; i++) {
                BlockMod mod;
                while (!modifications.TryDequeue(out mod)) { }
                Flat2i cp = BlockToChunk(mod.Pos);

                if (chunks[cp.X, cp.Z] == null) {
                    chunks[cp.X, cp.Z] = new Chunk(cp, true);
                    activeChunks.Add(cp);
                }

                chunks[cp.X, cp.Z].modifications.Enqueue(mod);

                if (!chunksToUpdate.Contains(cp))
                    chunksToUpdate.Add(cp);

                length += step;
                loadBar.PixWidth = (int)length;
            }

            smallLength += smallStep;
            smallLoadBar.PixWidth = (int)smallLength;

            while (chunksToUpdate.Count > 0) {
                Flat2i cp = chunksToUpdate[0];
                chunks[cp.X, cp.Z].UpdateMesh();
                chunksToUpdate.RemoveAt(0);
            }

            Player.Position = spawnPos + new Vector3(0.5f, 0.5f, 0.5f);
            Player.Position.Y = TerrainHeightAt(new Vector2(Player.Position.X, Player.Position.Z)) + 2.5f;
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

            activeChunks.Clear();

            for (int x = playerChunk.X - Settings.RenderDistance; x <= playerChunk.X + Settings.RenderDistance; x++)
                for (int z = playerChunk.Z - Settings.RenderDistance; z <= playerChunk.Z + Settings.RenderDistance; z++) {
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

        public static uint GetBlock(Vector3i pos)
        {
            Flat2i chunk = Flat2i.FromBlock(pos);

            if (!IsBlockInWorld(pos) || pos.Y < 0 || pos.Y > BlockData.ChunkHeight)
                return 0;

            if (chunks[chunk.X, chunk.Z] != null && chunks[chunk.X, chunk.Z].BlocksGenerated)
                return chunks[chunk.X, chunk.Z].GetBlockGlobalPos(pos).id;

            return GetGenBlock(pos);
        }

        public static bool CheckForBlock(Vector3 pos)
        {
            Flat2i chunk = Flat2i.FromBlock(pos);
            Vector3i iPos = (Vector3i)pos;

            if (!IsBlockInWorld(iPos) || iPos.Y < 0 || iPos.Y > BlockData.ChunkHeight)
                return false;

            if (chunks[chunk.X, chunk.Z] != null && chunks[chunk.X, chunk.Z].BlocksGenerated)
                return blocktypes[chunks[chunk.X, chunk.Z].GetBlockGlobalPos(iPos).id].isSolid;

            return blocktypes[GetGenBlock(iPos)].isSolid;
        }
        public static bool CheckForBlock(float x, float y, float z)
            => CheckForBlock(new Vector3(x, y, z));

        public static BlockState? GetBlockState(Vector3 pos)
        {
            Flat2i chunk = Flat2i.FromBlock(pos);
            Vector3i iPos = (Vector3i)pos;

            if (!IsBlockInWorld(iPos) || iPos.Y < 0 || iPos.Y > BlockData.ChunkHeight)
                return null;

            if (chunks[chunk.X, chunk.Z] != null && chunks[chunk.X, chunk.Z].BlocksGenerated)
                return chunks[chunk.X, chunk.Z].GetBlockGlobalPos(iPos);

            return new BlockState(GetGenBlock(iPos));
        }
        public static BlockState? GetBlockState(float x, float y, float z)
            => GetBlockState(new Vector3(x, y, z));

        private static int TerrainHeightAt (Vector2 vec2)
        {
            int solidGroundHeight = 42;
            float sumOfHeights = 0f;
            int count = 0;
            float strongestWeight = 0f;

            for (int i = 0; i < biomes.Length; i++) {
                float weight = Noise.Get2DPerlinNoise(vec2, biomes[i].offset, biomes[i].scale);

                if (weight > strongestWeight) {
                    strongestWeight = weight;
                }

                float height = Noise.Get2DPerlinNoise(vec2, 0f, biomes[i].terrainScale) * biomes[i].terrainHeight * weight;

                if (height > 0) {
                    sumOfHeights += height;
                    count++;
                }
            }

            if (count > 0)
                sumOfHeights /= (float)count;

            return (int)(sumOfHeights + solidGroundHeight);
        }

        public static int BiomeAt(Vector2 vec2)
        {
            float strongestWeight = 0f;
            int strongestBiomeIndex = 0;

            for (int i = 0; i < biomes.Length; i++) {
                float weight = Noise.Get2DPerlinNoise(vec2, biomes[i].offset, biomes[i].scale);

                if (weight > strongestWeight) {
                    strongestWeight = weight;
                    strongestBiomeIndex = i;
                }
            }

            return strongestBiomeIndex;
        }

        public static uint GetGenBlock(int x, int y, int z)
        {
            // Immutable pass
            if (!IsBlockInWorld(x, y, z))
                return 0;

            // Bedrock
            if (y == 0)
                return 7;

            Vector2 vec2 = new Vector2(x, z);

            // Biome Selection Pass
            int solidGroundHeight = 42;
            float sumOfHeights = 0f;
            int count = 0;
            float strongestWeight = 0f;
            int strongestBiomeIndex = 0;

            for (int i = 0; i < biomes.Length; i++) {
                float weight = Noise.Get2DPerlinNoise(vec2, biomes[i].offset, biomes[i].scale);

                if (weight > strongestWeight) {
                    strongestWeight = weight;
                    strongestBiomeIndex = i;
                }

                float height = Noise.Get2DPerlinNoise(vec2, 0f, biomes[i].terrainScale) * biomes[i].terrainHeight * weight;

                if (height > 0) {
                    sumOfHeights += height;
                    count++;
                }
            }

            int biomeIndex = strongestBiomeIndex;
            if (count > 0)
                sumOfHeights /= (float)count;

            // Basic terrain pass
            int terrainHeight = (int)(sumOfHeights + solidGroundHeight);
            uint blockId;

            if (y == terrainHeight)
                blockId = biomes[biomeIndex].surfaceBlock;
            else if (y < terrainHeight && y >= terrainHeight - 3)
                blockId = biomes[biomeIndex].subSurfaceBlock;
            else if (y < terrainHeight)
                blockId = 1;
            else
                return 0;

            // Second pass
            for (int i = 0; i < biomes[biomeIndex].lodes.Length; i++) {
                Lode lode = biomes[biomeIndex].lodes[i];
                if (!lode.reachTerrain && y >= terrainHeight - 3)
                    continue;
                if (y >= lode.minHeight && y <= lode.maxHeight) {
                    if (Noise.Get3DPerlin(new Vector3(x, y, z), lode.noiseOffset, lode.scale, lode.threshold))
                        return lode.blockID;
                }
            }

            // Tree pass
            if (y == terrainHeight && biomes[biomeIndex].majorFloarIndex != -1) {
                if (Noise.Get2DPerlinNoise(vec2, -700f, biomes[biomeIndex].majorFloraZoneScale) > biomes[biomeIndex].majorFloraZoneThreashold) {
                    if (Noise.Get2DPerlinNoise(vec2, 1200f, biomes[biomeIndex].majorFloraPlacementScale) > biomes[biomeIndex].majorFloraPlacementThreashold) {
                        Structure.GenerateMajorFlora(biomes[biomeIndex].majorFloarIndex, modifications, new Vector3i(x, y, z),
                            biomes[biomeIndex].minHeight, biomes[biomeIndex].maxHeight);
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
            s.UploadFloat("globalLight", globalLight);
            s.UploadFloat("minGlobalLightLevel", BlockData.minLightLevel);
            s.UploadFloat("maxGlobalLightLevel", BlockData.maxLightLevel);
            for (int x = 0; x < BlockData.WorldSizeInChunks; x++)
                for (int z = 0; z < BlockData.WorldSizeInChunks; z++)
                    if (chunks[x, z] != null)
                        chunks[x, z].Render(s);
        }
    }
}
