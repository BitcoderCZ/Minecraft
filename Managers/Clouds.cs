using Minecraft.Graphics;
using Minecraft.VertexTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.Managers
{
    public static class Clouds
    {
        public static int cloudHeight = 100;

        private static bool[,] cloudData;
        private static List<TexVertex> verts;
        private static List<int> tris;

        public static void Init()
        {
            verts = new List<TexVertex>();
            tris = new List<int>();
        }

        public static void Render(Shader s)
        {

        }
    }
}
