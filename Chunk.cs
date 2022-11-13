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
        uint transparentVertexIndex = 0;
        List<Vertex> vertices = new List<Vertex>();
        List<Vertex> transparentVertices = new List<Vertex>();
        List<uint> triangles = new List<uint>();
        List<uint> transparentTriangles = new List<uint>();
        Vertex[] vertsA;
        Vertex[] vertsB;
        uint[] trisA;
        uint[] trisB;

        public readonly uint[] blocks;

        public Queue<BlockMod> modifications = new Queue<BlockMod>();

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
        private int transpVao;
        private int transpVbo;
        private int transpEbo;

        public Chunk(Flat2i _position, bool generate)
        {
            active = false;
            BlocksGenerated = false;
            CreatedMesh = 0;
            CreatedMeshArrays = false;
            chunkPos = _position;
            pos = new Flat2i(chunkPos.X * BlockData.ChunkWidth, chunkPos.Z * BlockData.ChunkWidth);
            blocks = new uint[BlockData.ChunkLayerLength * BlockData.ChunkHeight];

            if (generate)
                Init();
        }

        public void Init()
        {
            GenerateBlocks();
            ClearMeshData();
            CreateMeshData();
            vertsA = vertices.ToArray();
            vertsB = transparentVertices.ToArray();
            trisA = triangles.ToArray();
            trisB = transparentTriangles.ToArray();
            CreatedMeshArrays = true;
            active = true;
        }

        public void UpdateMesh() // UpdateChunk
        {
            while (modifications.Count > 0) {
                BlockMod mod = modifications.Dequeue();
                Vector3i _pos = mod.Pos - pos;
                SetBlock(_pos, mod.Id, false);
            }

            ClearMeshData();
            CreateMeshData();
            vertsA = vertices.ToArray();
            vertsB = transparentVertices.ToArray();
            trisA = triangles.ToArray();
            trisB = transparentTriangles.ToArray();
            if (CreatedMesh != 0)
                CreatedMesh = 2;
        }

        private void GenerateBlocks()
        {
            for (int x = 0; x < BlockData.ChunkWidth; x++)
                for (int z = 0; z < BlockData.ChunkWidth; z++)
                    for (int y = 0; y < BlockData.ChunkHeight; y++) {
                        SetBlock(x, y, z, World.GetGenBlock(new Vector3i(x, y, z) + pos), false);
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
            transparentVertices.Clear();
            triangles.Clear();
            transparentTriangles.Clear();
            vertexIndex = 0;
            transparentVertexIndex = 0;
        }

        void AddBlockMeshData(Vector3i pos, uint blockId)
        {
            bool transparent = World.blocktypes[GetBlock(pos)].isTransparent;
            for (int p = 0; p < 6; p++) {
                if (CheckBlock(pos + BlockData.faceChecks[p]) && GetBlock(pos) != World.GetBlock(pos + BlockData.faceChecks[p] + this.pos)) {
                    uint texId = World.blocktypes[blockId].GetTextureID(p);
                    
                    if (!transparent) {
                        vertices.Add(new Vertex(pos + BlockData.blockVerts[BlockData.blockTris[p, 0]], BlockData.voxelUvs[0], texId));
                        vertices.Add(new Vertex(pos + BlockData.blockVerts[BlockData.blockTris[p, 1]], BlockData.voxelUvs[1], texId));
                        vertices.Add(new Vertex(pos + BlockData.blockVerts[BlockData.blockTris[p, 2]], BlockData.voxelUvs[2], texId));
                        vertices.Add(new Vertex(pos + BlockData.blockVerts[BlockData.blockTris[p, 3]], BlockData.voxelUvs[3], texId));
                        triangles.Add(vertexIndex);
                        triangles.Add(vertexIndex + 1);
                        triangles.Add(vertexIndex + 2);
                        triangles.Add(vertexIndex + 2);
                        triangles.Add(vertexIndex + 1);
                        triangles.Add(vertexIndex + 3);
                        vertexIndex += 4;
                    } else {
                        transparentVertices.Add(new Vertex(pos + BlockData.blockVerts[BlockData.blockTris[p, 0]], BlockData.voxelUvs[0], texId));
                        transparentVertices.Add(new Vertex(pos + BlockData.blockVerts[BlockData.blockTris[p, 1]], BlockData.voxelUvs[1], texId));
                        transparentVertices.Add(new Vertex(pos + BlockData.blockVerts[BlockData.blockTris[p, 2]], BlockData.voxelUvs[2], texId));
                        transparentVertices.Add(new Vertex(pos + BlockData.blockVerts[BlockData.blockTris[p, 3]], BlockData.voxelUvs[3], texId));
                        transparentTriangles.Add(transparentVertexIndex);
                        transparentTriangles.Add(transparentVertexIndex + 1);
                        transparentTriangles.Add(transparentVertexIndex + 2);
                        transparentTriangles.Add(transparentVertexIndex + 2);
                        transparentTriangles.Add(transparentVertexIndex + 1);
                        transparentTriangles.Add(transparentVertexIndex + 3);
                        transparentVertexIndex += 4;
                    }
                }
            }

        }

        void InitMesh()
        {
            GL.CreateVertexArrays(1, out transpVao);
            GL.BindVertexArray(transpVao);
            GL.CreateBuffers(1, out transpEbo);
            GL.CreateBuffers(1, out transpVbo);

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


            GL.NamedBufferData(transpEbo, transparentTriangles.Count * sizeof(uint), trisB, BufferUsageHint.DynamicDraw);
            GL.VertexArrayElementBuffer(transpVao, transpEbo);

            GL.NamedBufferData(transpVbo, transparentVertices.Count * Vertex.Size, vertsB, BufferUsageHint.DynamicDraw);
            GL.VertexArrayVertexBuffer(transpVao, vertexBindingPoint, transpVbo, IntPtr.Zero, Vertex.Size);

            // pos
            GL.VertexArrayAttribFormat(transpVao, 0, 3, VertexAttribType.Float, false, 0);
            GL.VertexArrayAttribBinding(transpVao, 0, vertexBindingPoint);
            GL.EnableVertexArrayAttrib(transpVao, 0);
            // uv
            GL.VertexArrayAttribFormat(transpVao, 1, 3, VertexAttribType.Float, false, 3 * sizeof(float));
            GL.VertexArrayAttribBinding(transpVao, 1, vertexBindingPoint);
            GL.EnableVertexArrayAttrib(transpVao, 1);
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

            Matrix4 transform = Matrix4.CreateTranslation(new Vector3(pos.X, 0f, pos.Z));
            s.UploadMat4("uTransform", ref transform);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2DArray, Texture.taid);
            GL.Uniform1(5, Texture.taid);

            GL.Enable(EnableCap.CullFace);
            GL.BindVertexArray(vao);
            GL.DrawElements(BeginMode.Triangles, triangles.Count, DrawElementsType.UnsignedInt, 0);

            GL.Disable(EnableCap.CullFace);
            GL.BindVertexArray(transpVao);
            GL.DrawElements(BeginMode.Triangles, transparentTriangles.Count, DrawElementsType.UnsignedInt, 0);
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

            blocks[X + (Z * BlockData.ChunkWidth) + (Y * BlockData.ChunkLayerLength)] = id;

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

            return blocks[X + (Z * BlockData.ChunkWidth) + (Y * BlockData.ChunkLayerLength)];
        }
        public uint GetBlock(Vector3i pos)
            => GetBlock(pos.X, pos.Y, pos.Z);

        public bool CheckBlock(int X, int Y, int Z)
        {
            if (!IsBlockInChunk(X, Y, Z))
                return World.CheckIfBlockTransparent(X + pos.X, Y, Z + pos.Z);

            return World.blocktypes[blocks[X + (Z * BlockData.ChunkWidth) + (Y * BlockData.ChunkLayerLength)]].isTransparent;
        }
        public bool CheckBlock(Vector3i pos)
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

        public uint GetBlockGlobalPos(Vector3i _pos)
        {
            _pos.X -= pos.X;
            _pos.Z -= pos.Z;

            return GetBlock(_pos);
        }

        public void SetBlockGlobalPos(Vector3i _pos, uint id, bool updateMesh)
        {
            _pos.X -= pos.X;
            _pos.Z -= pos.Z;
            SetBlock(_pos, id, updateMesh);
        }
    }
}
