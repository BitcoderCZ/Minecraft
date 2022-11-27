using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public struct BiomeAttribs
    {
        //
        public string name;
        public int offset;
        public float scale;

        public int terrainHeight;
        public float terrainScale;

        public uint surfaceBlock;
        public uint subSurfaceBlock;

        // Major Flora
        public int majorFloarIndex;
        public float majorFloraZoneScale;
        public float majorFloraZoneThreashold;
        public float majorFloraPlacementScale;
        public float majorFloraPlacementThreashold;

        public int maxHeight;
        public int minHeight;

        public Lode[] lodes;

        public BiomeAttribs(string _name, int _offset, float _scale, int _terrainHeight, float _terrainScale, uint _surfaceBlock,
            uint _subSurfaceBlock, int _majorFloarIndex, int _minH, int _maxH, float _majorFloraZoneScale,
            float _majorFloraZoneThreashold, float _majorFloraPlacementScale, float _majorFloraPlacementThreashold, 
            params Lode[] _lodes)
        {
            name = _name;
            offset = _offset;
            scale = _scale;

            terrainHeight = _terrainHeight;
            terrainScale = _terrainScale;

            surfaceBlock = _surfaceBlock;
            subSurfaceBlock = _subSurfaceBlock;

            majorFloarIndex = _majorFloarIndex;
            minHeight = _minH;
            maxHeight = _maxH;
            majorFloraZoneScale = _majorFloraZoneScale;
            majorFloraZoneThreashold = _majorFloraZoneThreashold;
            majorFloraPlacementScale = _majorFloraPlacementScale;
            majorFloraPlacementThreashold = _majorFloraPlacementThreashold;

            if (_lodes == null)
                lodes = new Lode[0];
            else
                lodes = _lodes;
        }
    }
}
