using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public struct BiomeAttribs
    {
        public string name;

        public int minHeight;
        public int terrainHeight;
        public float terrainScale;

        public BiomeAttribs(string _name, int _minH, int _terrainHeight, float _terrainScale)
        {
            name = _name;
            minHeight = _minH;
            terrainHeight = _terrainHeight;
            terrainScale = _terrainScale;
        }
    }
}
