using System;

namespace Minecraft
{
    public class BlockType
    {
        public string blockName;
        public bool isSolid;
        public bool renderNeighborFaces;
        public float transparency;
        public int item;

        public uint backFaceTexture;
        public uint frontFaceTexture;
        public uint topFaceTexture;
        public uint bottomFaceTexture;
        public uint leftFaceTexture;
        public uint rightFaceTexture;

        public BlockType(string name, bool solid, bool _renderNeighborFaces, float _transparency, uint back, uint front, uint top, uint bottom, uint left, uint right, int _item)
        {
            blockName = name;
            isSolid = solid;
            renderNeighborFaces = _renderNeighborFaces;
            transparency = _transparency;
            backFaceTexture = back;
            frontFaceTexture = front;
            topFaceTexture = top;
            bottomFaceTexture = bottom;
            leftFaceTexture = left;
            rightFaceTexture = right;
            item = _item;
        }

        public BlockType(string name, bool solid, bool _renderNeighborFaces, float _transparency, uint tex, int _item) 
            : this(name, solid, _renderNeighborFaces, _transparency, tex, tex, tex, tex, tex, tex, _item)
        { }

        public uint GetTextureID(int faceIndex)
        {
            switch (faceIndex) {
                case 0:
                    return backFaceTexture;
                case 1:
                    return frontFaceTexture;
                case 2:
                    return topFaceTexture;
                case 3:
                    return bottomFaceTexture;
                case 4:
                    return leftFaceTexture;
                case 5:
                    return rightFaceTexture;
                default:
                    Console.WriteLine($"Error in GetTextureID; invalid face index {faceIndex}");
                    return 0;
            }
        }
    }
}
