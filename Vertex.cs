using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public const int Size = 3 * sizeof(float);

        public Vector3 position;
    }
}
