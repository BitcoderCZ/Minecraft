using System;

namespace Minecraft
{
    public struct BlockType
    {
        public string blockName;
        public bool isSolid;
        public bool isTransparent;
        public int item;

        public uint backFaceTexture;
        public uint frontFaceTexture;
        public uint topFaceTexture;
        public uint bottomFaceTexture;
        public uint leftFaceTexture;
        public uint rightFaceTexture;

        public BlockType(string name, bool solid, bool transparent, uint back, uint front, uint top, uint bottom, uint left, uint right, int _item)
        {
            blockName = name;
            isSolid = solid;
            isTransparent = transparent;
            backFaceTexture = back;
            frontFaceTexture = front;
            topFaceTexture = top;
            bottomFaceTexture = bottom;
            leftFaceTexture = left;
            rightFaceTexture = right;
            item = _item;
        }

        public BlockType(string name, bool solid, bool transparent, uint tex, int _item) : this(name, solid, transparent, tex, tex, tex, tex, tex, tex, _item)
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
