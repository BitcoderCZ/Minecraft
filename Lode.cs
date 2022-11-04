using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public struct Lode // ores, andesite, ect...
    {
        public string name;
        public byte blockID;
        public bool reachTerrain;
        public int minHeight;
        public int maxHeight;
        public float scale;
        public float threshold;
        public float noiseOffset;

        public Lode(string _name, byte _blockID, bool _reachTerrain, int _minHeight, int _maxHeight, float _scale, float _threshold, float _noiseOffset)
        {
            name = _name;
            blockID = _blockID;
            reachTerrain = _reachTerrain;
            minHeight = _minHeight;
            maxHeight = _maxHeight;
            scale = _scale;
            threshold = _threshold;
            noiseOffset = _noiseOffset;
        }
    }
}
