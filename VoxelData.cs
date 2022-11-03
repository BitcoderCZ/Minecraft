using Minecraft.Math;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public static class VoxelData
    {
        public const int ChunkWidth = 16;
        public const int ChunkHeight = 64;
        public const int ChunkLayerLength = ChunkWidth * ChunkWidth;
		public const int WorldSizeInChunks = 5;

		public static readonly Vector3[] voxelVerts = new Vector3[8] {
			new Vector3(0.0f, 0.0f, 0.0f),
			new Vector3(1.0f, 0.0f, 0.0f),
			new Vector3(1.0f, 1.0f, 0.0f),
			new Vector3(0.0f, 1.0f, 0.0f),
			new Vector3(0.0f, 0.0f, 1.0f),
			new Vector3(1.0f, 0.0f, 1.0f),
			new Vector3(1.0f, 1.0f, 1.0f),
			new Vector3(0.0f, 1.0f, 1.0f)
		};

		public static readonly Vector3i[] faceChecks = new Vector3i[6] {
			new Vector3i(0, 0, -1),
			new Vector3i(0, 0, 1),
			new Vector3i(0, 1, 0),
			new Vector3i(0, -1, 0),
			new Vector3i(-1, 0, 0),
			new Vector3i(1, 0, 0)
		};

		public static readonly int[,] voxelTris = new int[6, 4] {
			// 0 1 2 2 1 3
			{0, 3, 1, 2}, // Back Face
			{5, 6, 4, 7}, // Front Face
			{3, 7, 2, 6}, // Top Face
			{1, 5, 0, 4}, // Bottom Face
			{4, 7, 0, 3}, // Left Face
			{1, 2, 5, 6} // Right Face
		};

		public static readonly Vector2[] voxelUvs = new Vector2[4] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.0f, 1.0f),
			new Vector2(1.0f, 0.0f),
			new Vector2(1.0f, 1.0f)
		};
	}
}
