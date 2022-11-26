using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public struct BlockState
    {
        public uint id;
        public float globalLightPercent;

        public BlockState(uint _id)
        {
            id = _id;
            globalLightPercent = 0f;
        }
    }
}
