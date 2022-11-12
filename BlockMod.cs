using Minecraft.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public struct BlockMod
    {
        public Vector3i Pos;
        public uint Id;

        public BlockMod(Vector3i pos, uint id)
        {
            Pos = pos;
            Id = id;
        }
    }
}
