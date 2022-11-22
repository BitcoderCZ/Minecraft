using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public struct ItemStack
    {
        public uint id;
        public byte count;

        public ItemStack(uint _id, byte _count)
        {
            id = _id;
            count = _count;
        }
    }
}
