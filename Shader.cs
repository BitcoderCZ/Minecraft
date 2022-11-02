using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public class Shader
    {
        public int ProgramId;

        private string vertexShaderSource;
        private string fragmentShaderSource;

        public void Compile(string name)
        {
            string path = Environment.CurrentDirectory + "/Data/Shaders/" + name;
            vertexShaderSource = File.ReadAllText(path + ".vert");
            fragmentShaderSource = File.ReadAllText(path + ".frag");

            int vertex = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertex, vertexShaderSource);
            GL.CompileShader(vertex);

            GL.GetShader(vertex, ShaderParameter.CompileStatus, out int status);
            
            if (status == 0) {
                string err = GL.GetShaderInfoLog(vertex);
                Console.WriteLine($"ERROR::SHADER::VERTEX::COMPILATION_FAILED\n{ err}");
            }

            int fragment = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragment, fragmentShaderSource);
            GL.CompileShader(fragment);

            GL.GetShader(fragment, ShaderParameter.CompileStatus, out status);

            if (status == 0) {
                string err = GL.GetShaderInfoLog(vertex);
                Console.WriteLine($"ERROR::SHADER::FRAGMENT::COMPILATION_FAILED\n{err}");
            }

            ProgramId = GL.CreateProgram();
            GL.AttachShader(ProgramId, vertex);
            GL.AttachShader(ProgramId, fragment);
            GL.LinkProgram(ProgramId);

            GL.GetProgram(ProgramId, GetProgramParameterName.LinkStatus, out status);
            if (status == 0) {
                string err = GL.GetProgramInfoLog(vertex);
                Console.WriteLine($"ERROR::SHADER::PROGRAM::LINKING_FAILED\n{err}");
            }

            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);
        }

        public void Bind()
        {
            GL.UseProgram(ProgramId);
        }
    }
}
