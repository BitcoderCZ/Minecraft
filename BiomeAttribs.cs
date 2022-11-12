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

        // Trees
        public float treeZoneScale;
        public float treeZoneThreashold;
        public float treePlacementScale;
        public float treePlacementThreashold;

        public Lode[] lodes;

        public BiomeAttribs(string _name, int _minH, int _terrainHeight, float _terrainScale, float _treeZoneScale, 
            float _treeZoneThreashold, float _treePlacementScale, float _treePlacementThreashold, params Lode[] _lodes)
        {
            name = _name;
            minHeight = _minH;
            terrainHeight = _terrainHeight;
            terrainScale = _terrainScale;

            treeZoneScale = _treeZoneScale;
            treeZoneThreashold = _treeZoneThreashold;
            treePlacementScale = _treePlacementScale;
            treePlacementThreashold = _treePlacementThreashold;

            if (_lodes == null)
                lodes = new Lode[0];
            else
                lodes = _lodes;
        }
    }
}
