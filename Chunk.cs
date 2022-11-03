using Minecraft.Math;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
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

        public readonly uint[] blocks;

        public Vector2i pos;

        private int vao;
        private int vbo;
        private int ebo;

        public Chunk(Vector2i _position, uint[] _blocks)
        {
            pos = _position;
            blocks = _blocks;
            CreateMeshData();
            InitMesh();
        }

        public Chunk(Vector2i _position) : this(_position, new uint[VoxelData.ChunkLayerLength * VoxelData.ChunkHeight])
        { }

        public static Chunk Create(Vector2i _pos)
        {
            Chunk chunk = new Chunk(_pos);
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                    for (int y = 0; y < VoxelData.ChunkHeight; y++) {
                        if (y == 0)
                            chunk.SetBlock(x, y, z, 7);
                        else if (y != VoxelData.ChunkHeight - 1)
                            chunk.SetBlock(x, y, z, 3);
                        else
                            chunk.SetBlock(x, y, z, 2);
                    }
            chunk.CreateMeshData();
            chunk.CreateMesh();
            return chunk;
        }

        public void CreateMeshData()
        {
            vertices.Clear();
            triangles.Clear();
            vertexIndex = 0;
            for (int y = 0; y < VoxelData.ChunkHeight; y++)
                for (int x = 0; x < VoxelData.ChunkWidth; x++)
                    for (int z = 0; z < VoxelData.ChunkWidth; z++) {
                        uint blockId = GetBlock(x, y, z);
                        if (blockId != 0)
                            AddVoxelDataToChunk(new Vector3i(x, y, z), blockId);
                    }
        }

        void AddVoxelDataToChunk(Vector3i pos, uint blockId)
        {
            for (int p = 0; p < 6; p++) {
                if (GetBlock(pos + VoxelData.faceChecks[p]) == 0) {
                    uint texId = World.blocktypes[blockId].GetTextureID(p);
                    vertices.Add(new Vertex(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]], VoxelData.voxelUvs[0], texId));
                    vertices.Add(new Vertex(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]], VoxelData.voxelUvs[1], texId));
                    vertices.Add(new Vertex(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]], VoxelData.voxelUvs[2], texId));
                    vertices.Add(new Vertex(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]], VoxelData.voxelUvs[3], texId));
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
        }

        public void CreateMesh()
        {
            GL.NamedBufferData(ebo, triangles.Count * sizeof(uint), triangles.ToArray(), BufferUsageHint.DynamicDraw);
            GL.VertexArrayElementBuffer(vao, ebo);

            int vertexBindingPoint = 0;
            GL.NamedBufferData(vbo, vertices.Count * Vertex.Size, vertices.ToArray(), BufferUsageHint.DynamicDraw);
            GL.VertexArrayVertexBuffer(vao, vertexBindingPoint, vbo, IntPtr.Zero, Vertex.Size);

            // pos
            GL.VertexArrayAttribFormat(vao, 0, 3, VertexAttribType.Float, false, 0);
            GL.VertexArrayAttribBinding(vao, 0, vertexBindingPoint);
            GL.EnableVertexArrayAttrib(vao, 0);
            // uv
            GL.VertexArrayAttribFormat(vao, 1, 3, VertexAttribType.Float, false, 3 * sizeof(float));
            GL.VertexArrayAttribBinding(vao, 1, vertexBindingPoint);
            GL.EnableVertexArrayAttrib(vao, 1);
        }

        public void Render(Shader s)
        {
            Matrix4 transform = Matrix4.CreateTranslation(new Vector3(pos.X * VoxelData.ChunkWidth, 0f, pos.Y * VoxelData.ChunkWidth));
            s.UploadMat4("uTransform", ref transform);
            GL.BindTexture(TextureTarget.Texture2DArray, Texture.taid);
            GL.Uniform1(5, Texture.taid);

            GL.BindVertexArray(vao);
            GL.DrawElements(BeginMode.Triangles, triangles.Count, DrawElementsType.UnsignedInt, 0);
        }

        public void SetBlock(int X, int Y, int Z, uint id)
        {
            if (X < 0 || X >= VoxelData.ChunkWidth || Y < 0 || Y >= VoxelData.ChunkHeight || Z < 0 || Z >= VoxelData.ChunkWidth)
                return;
            blocks[X + (Z * VoxelData.ChunkWidth) + (Y * VoxelData.ChunkLayerLength)] = id;
        }
        
        public void SetBlock(Vector3i pos, uint id)
            => SetBlock(pos.X, pos.Y, pos.Z, id);

        public uint GetBlock(int X, int Y, int Z)
        {
            if (X < 0 || X >= VoxelData.ChunkWidth || Y < 0 || Y >= VoxelData.ChunkHeight || Z < 0 || Z >= VoxelData.ChunkWidth)
                return 0;

            return blocks[X + (Z * VoxelData.ChunkWidth) + (Y * VoxelData.ChunkLayerLength)];
        }
            
        public uint GetBlock(Vector3i pos)
            => GetBlock(pos.X, pos.Y, pos.Z);
    }
}
