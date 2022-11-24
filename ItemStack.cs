using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public class ItemStack
    {
        public uint id;
        public byte amount;

        public ItemStack(uint _id, byte _amount)
        {
            id = _id;
            amount = _amount;
        }
    }
}
