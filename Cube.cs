using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public struct Cube
    {
        public Vertex[] verts;
        public int vao;
        public int vbo;

        public static int[] indices = new int[36]
        {
            // Each set of 6 indices represents one quad
			1, 0, 2,    3, 1, 2,    // Front face
			5, 1, 3,    7, 5, 3,    // Right face
			7, 6, 4,    5, 7, 4,    // Back face
			0, 4, 6,    2, 0, 6,    // Left face
			5, 4, 0,    1, 5, 0,    // Top face
			3, 2, 6,    7, 3, 6     // Bottom face
        };
        static Cube defCube;

        static Cube()
        {
            Vector3[] verts = new Vector3[8];
            verts[0] = new Vector3(-0.5f, 0.5f, 0.5f);
            verts[1] = new Vector3(0.5f, 0.5f, 0.5f);
            verts[2] = new Vector3(-0.5f, -0.5f, 0.5f);
            verts[3] = new Vector3(0.5f, -0.5f, 0.5f);

            verts[4] = new Vector3(-0.5f, 0.5f, -0.5f);
            verts[5] = new Vector3(0.5f, 0.5f, -0.5f);
            verts[6] = new Vector3(-0.5f, -0.5f, -0.5f);
            verts[7] = new Vector3(0.5f, -0.5f, -0.5f);

            Vector2[] uvs = new Vector2[6];
            uvs[0] = new Vector2(1.0f, 1.0f);
            uvs[1] = new Vector2(0.0f, 1.0f);
            uvs[2] = new Vector2(0.0f, 0.0f);

            uvs[4] = new Vector2(1.0f, 1.0f);
            uvs[3] = new Vector2(1.0f, 0.0f);
            uvs[5] = new Vector2(0.0f, 0.0f);

            defCube = new Cube();
            defCube.verts = new Vertex[indices.Length];
            for (int i = 0; i < indices.Length; i++) {
                defCube.verts[i].position = verts[indices[i]];
                defCube.verts[i].uv = uvs[i % 6];
            }
            
            GL.GenVertexArrays(1, out defCube.vao);
            GL.BindVertexArray(defCube.vao);

            // Set up and buffer the vertex buffer (OpenGL 3.3 method)
            GL.GenBuffers(1, out defCube.vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, defCube.vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertex.Size * defCube.verts.Length, defCube.verts, BufferUsageHint.StaticDraw);

            // Set up and enable attribute 0 (OpenGL 3.3 method)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.Size, 0);
            GL.EnableVertexArrayAttrib(defCube.vao, 0);

            // Set up and enable attribute 1 (OpenGL 3.3 method)
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Vertex.Size, 3 * sizeof(float));
            GL.EnableVertexArrayAttrib(defCube.vao, 1);
        }

        public static void drawCube(Vector3 position, Texture texture, Shader shader)
		{
			// NOTE: We'll cover rotation and scaling of 3D objects another time
			// Upload transform
			Matrix4 transform = Matrix4.CreateTranslation(position);
            //GL.UniformMatrix4(0, false, ref transform);
            shader.UploadMat4("uTransform", ref transform);

			// Upload texture
			// Bind our texture to texture slot 0
			int textureSlot = 0;
            GL.ActiveTexture(TextureUnit.Texture0 + textureSlot);
            GL.BindTexture(TextureTarget.Texture2D, texture.id);
            // DSA version of the two lines above (requires OpenGL 4.5+)
            //glBindTextureUnit(textureSlot, texture.textureId);
            // Upload texture slot 0 to our uniform uTexture
            //GL.Uniform1(3, textureSlot);
            shader.UploadInt("uTexture", textureSlot);

            // Draw our cube
            GL.BindVertexArray(defCube.vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, defCube.verts.Length);
        }
    }
}
