using Minecraft.Graphics;
using Minecraft.Math;
using Minecraft.VertexTypes;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Platform.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public class Chunk
    {
        uint vertexIndex = 0;
        List<Vertex> vertices = new List<Vertex>();
        List<uint> triangles = new List<uint>();
        Vertex[] vertsA;
        uint[] trisA;

        public readonly BlockState[] blocks;

        public Queue<BlockMod> modifications = new Queue<BlockMod>();

        // Rise Anim
        private float yOffset;

        public Flat2i chunkPos;
        public Flat2i pos;
        public bool Active
        {
            get => active;
            set {
                active = value;
            }
        }
        private bool active;
        public bool BlocksGenerated;
        public byte CreatedMesh;
        private bool CreatedMeshArrays;

        private int vao;
        private int vbo;
        private int ebo;

        public Chunk(Flat2i _position, bool _init)
        {
            active = false;
            BlocksGenerated = false;
            CreatedMesh = 0;
            CreatedMeshArrays = false;
            chunkPos = _position;
            pos = new Flat2i(chunkPos.X * BlockData.ChunkWidth, chunkPos.Z * BlockData.ChunkWidth);
            blocks = new BlockState[BlockData.ChunkLayerLength * BlockData.ChunkHeight];

            if (_init)
                Init();
            else if(!_init && World.Settings.AnimatedChunks)
                yOffset = -BlockData.ChunkHeight;
        }

        public void Init()
        {
            GenerateBlocks();
            ClearMeshData();
            CalcLight();
            CreateMeshData();
            vertsA = vertices.ToArray();
            trisA = triangles.ToArray();
            CreatedMeshArrays = true;
            active = true;
        }

        public void Update(float delta)
        {
            if (CreatedMesh != 0 && yOffset < -0.05f) {
                yOffset = Vector3.Lerp(new Vector3(0f, yOffset, 0f), Vector3.Zero, delta * World.Settings.AnimatedChunksSpeed).Y;
            }
            else if (CreatedMesh != 0 && yOffset >= -0.05f)
                yOffset = 0f;
        }

        public void UpdateMesh() // UpdateChunk
        {
            while (modifications.Count > 0) {
                BlockMod mod = modifications.Dequeue();
                Vector3i _pos = mod.Pos - pos;
                SetBlock(_pos, mod.Id, false);
            }

            ClearMeshData();
            CalcLight();
            CreateMeshData();
            vertsA = vertices.ToArray();
            trisA = triangles.ToArray();
            if (CreatedMesh != 0)
                CreatedMesh = 2;
        }

        private void CalcLight()
        {
            Queue<Vector3i> litBlocks = new Queue<Vector3i>();

            for (int x = 0; x < BlockData.ChunkWidth; x++)
                for (int z = 0; z < BlockData.ChunkWidth; z++) {
                    float lightRay = 1f;

                    for (int y = BlockData.ChunkHeight - 1; y >= 0; y--) {
                        int index = x + (z * BlockData.ChunkWidth) + (y * BlockData.ChunkLayerLength);
                        BlockState thisBlock = blocks[index];

                        if (thisBlock.id > 0 && World.blocktypes[thisBlock.id].transparency < lightRay)
                            lightRay = World.blocktypes[thisBlock.id].transparency;

                        thisBlock.globalLightPercent = lightRay;
                        blocks[index] = thisBlock;

                        if (lightRay > BlockData.lightFalloff)
                            litBlocks.Enqueue(new Vector3i(x, y, z));
                    }
                }

            while (litBlocks.Count > 0) {
                Vector3i v = litBlocks.Dequeue();
                int vIndex = v.X + (v.Z * BlockData.ChunkWidth) + (v.Y * BlockData.ChunkLayerLength);

                for (int p = 0; p < 6; p++) {
                    Vector3i neighbor = v + BlockData.faceChecks[p];
                    int neighborIndex = neighbor.X + (neighbor.Z * BlockData.ChunkWidth) + (neighbor.Y * BlockData.ChunkLayerLength);

                    if (IsBlockInChunk(neighbor)) {
                        if (blocks[neighborIndex].globalLightPercent < blocks[vIndex].globalLightPercent - BlockData.lightFalloff) {
                            blocks[neighborIndex].globalLightPercent = blocks[vIndex].globalLightPercent - BlockData.lightFalloff;

                            if (blocks[neighborIndex].globalLightPercent > BlockData.lightFalloff)
                                litBlocks.Enqueue(neighbor);
                        }
                    }
                }
            }
        }

        private void GenerateBlocks()
        {
            for (int x = 0; x < BlockData.ChunkWidth; x++)
                for (int z = 0; z < BlockData.ChunkWidth; z++)
                    for (int y = 0; y < BlockData.ChunkHeight; y++) {
                        blocks[x + (z * BlockData.ChunkWidth) + (y * BlockData.ChunkLayerLength)] =
                            new BlockState(World.GetGenBlock(new Vector3i(x, y, z) + pos));
                        //SetBlock(x, y, z, World.GetGenBlock(new Vector3i(x, y, z) + pos), false);
                    }

            BlocksGenerated = true;
        }

        public void CreateMeshData()
        {
            for (int y = 0; y < BlockData.ChunkHeight; y++)
                for (int x = 0; x < BlockData.ChunkWidth; x++)
                    for (int z = 0; z < BlockData.ChunkWidth; z++) {
                        uint blockId = GetBlock(x, y, z);
                            
                        if (blockId != 0)
                            AddBlockMeshData(new Vector3i(x, y, z), blockId);
                    }
        }

        public void ClearMeshData()
        {
            vertices.Clear();
            triangles.Clear();
            vertexIndex = 0;
        }

        void AddBlockMeshData(Vector3i pos, uint blockId)
        {
            for (int p = 0; p < 6; p++) {
                BlockState? neighbor = CheckBlock(pos + BlockData.faceChecks[p]);

                if (neighbor.HasValue && World.blocktypes[neighbor.Value.id].renderNeighborFaces && GetBlock(pos) != World.GetBlock(pos + BlockData.faceChecks[p] + this.pos)) {
                    uint texId = World.blocktypes[blockId].GetTextureID(p);

                    float lightLevel = neighbor.Value.globalLightPercent;

                    Vector4 color = new Vector4(0f, 0f, 0f, lightLevel);

                    vertices.Add(new Vertex(pos + BlockData.blockVerts[BlockData.blockTris[p, 0]], BlockData.voxelUvs[0], texId, color));
                    vertices.Add(new Vertex(pos + BlockData.blockVerts[BlockData.blockTris[p, 1]], BlockData.voxelUvs[1], texId, color));
                    vertices.Add(new Vertex(pos + BlockData.blockVerts[BlockData.blockTris[p, 2]], BlockData.voxelUvs[2], texId, color));
                    vertices.Add(new Vertex(pos + BlockData.blockVerts[BlockData.blockTris[p, 3]], BlockData.voxelUvs[3], texId, color));
                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 3);
                    vertexIndex += 4;
                }
            }

        }

        void InitMesh()
        {
            GL.CreateVertexArrays(1, out vao);
            GL.BindVertexArray(vao);
            GL.CreateBuffers(1, out ebo);
            GL.CreateBuffers(1, out vbo);
            CreateMesh();
            CreatedMesh = 1;
        }

        public void CreateMesh()
        {
            GL.NamedBufferData(ebo, triangles.Count * sizeof(uint), trisA, BufferUsageHint.DynamicDraw);
            GL.VertexArrayElementBuffer(vao, ebo);

            int vertexBindingPoint = 0;
            GL.NamedBufferData(vbo, vertices.Count * Vertex.Size, vertsA, BufferUsageHint.DynamicDraw);
            GL.VertexArrayVertexBuffer(vao, vertexBindingPoint, vbo, IntPtr.Zero, Vertex.Size);

            // pos
            GL.VertexArrayAttribFormat(vao, 0, 3, VertexAttribType.Float, false, 0);
            GL.VertexArrayAttribBinding(vao, 0, vertexBindingPoint);
            GL.EnableVertexArrayAttrib(vao, 0);
            // uv
            GL.VertexArrayAttribFormat(vao, 1, 3, VertexAttribType.Float, false, 3 * sizeof(float));
            GL.VertexArrayAttribBinding(vao, 1, vertexBindingPoint);
            GL.EnableVertexArrayAttrib(vao, 1);
            // uv
            GL.VertexArrayAttribFormat(vao, 2, 4, VertexAttribType.Float, false, 6 * sizeof(float));
            GL.VertexArrayAttribBinding(vao, 2, vertexBindingPoint);
            GL.EnableVertexArrayAttrib(vao, 2);
        }

        public void Render(Shader s)
        {
            if (!Active || !CreatedMeshArrays)
                return;

            if (CreatedMesh == 0)
                InitMesh();
            else if (CreatedMesh == 2) {
                CreateMesh();
                CreatedMesh = 1;
            }

            Matrix4 transform = Matrix4.CreateTranslation(new Vector3(pos.X, yOffset, pos.Z));
            s.UploadMat4("uTransform", ref transform);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2DArray, Texture.taid);
            GL.Uniform1(5, Texture.taid);

            GL.BindVertexArray(vao);
            GL.DrawElements(BeginMode.Triangles, triangles.Count, DrawElementsType.UnsignedInt, 0);
        }

        private void UpdateSurroundingBlocks(int x, int y, int z)
        {
            Vector3i thisBlock = new Vector3i(x, y, z);
            for (int p = 0; p < 6; p++) {
                Vector3i currentBlock = thisBlock + BlockData.faceChecks[p];
                if (!IsBlockInChunk(currentBlock)) {
                    World.GetChunkFromBlock(currentBlock + pos).UpdateMesh();
                }
            }
        }

        public void SetBlock(int X, int Y, int Z, uint id, bool updateMesh)
        {
            if (!IsBlockInChunk(X, Y, Z))
                return;

            blocks[X + (Z * BlockData.ChunkWidth) + (Y * BlockData.ChunkLayerLength)].id = id;

            if (updateMesh) {
                UpdateMesh();
                UpdateSurroundingBlocks(X, Y, Z);
            }
        }
        public void SetBlock(Vector3i pos, uint id, bool updateMes)
            => SetBlock(pos.X, pos.Y, pos.Z, id, updateMes);

        public uint GetBlock(int X, int Y, int Z)
        {
            if (!IsBlockInChunk(X, Y, Z))
                return 0;

            return blocks[X + (Z * BlockData.ChunkWidth) + (Y * BlockData.ChunkLayerLength)].id;
        }
        public uint GetBlock(Vector3i pos)
            => GetBlock(pos.X, pos.Y, pos.Z);

        public BlockState? CheckBlock(int X, int Y, int Z)
        {
            if (!IsBlockInChunk(X, Y, Z))
                return World.GetBlockState(X + pos.X, Y, Z + pos.Z);

            return blocks[X + (Z * BlockData.ChunkWidth) + (Y * BlockData.ChunkLayerLength)];
        }
        public BlockState? CheckBlock(Vector3i pos)
            => CheckBlock(pos.X, pos.Y, pos.Z);

        public bool IsBlockInChunk(int X, int Y, int Z)
        {
            if (X < 0 || X >= BlockData.ChunkWidth || Y < 0 || Y >= BlockData.ChunkHeight || Z < 0 || Z >= BlockData.ChunkWidth)
                return false;
            else
                return true;
        }
        public bool IsBlockInChunk(Vector3i pos)
           => IsBlockInChunk(pos.X, pos.Y, pos.Z);

        public BlockState GetBlockGlobalPos(Vector3i _pos)
        {
            _pos.X -= pos.X;
            _pos.Z -= pos.Z;

            return blocks[_pos.X + (_pos.Z * BlockData.ChunkWidth) + (_pos.Y * BlockData.ChunkLayerLength)];
        }

        public void SetBlockGlobalPos(Vector3i _pos, uint id, bool updateMesh)
        {
            _pos.X -= pos.X;
            _pos.Z -= pos.Z;
            SetBlock(_pos, id, updateMesh);
        }
    }
}
