using System;

namespace Minecraft
{
    public struct BlockType
    {
        public string blockName;
        public bool isSolid;

        public uint backFaceTexture;
        public uint frontFaceTexture;
        public uint topFaceTexture;
        public uint bottomFaceTexture;
        public uint leftFaceTexture;
        public uint rightFaceTexture;

        public BlockType(string name, bool solid, uint back, uint front, uint top, uint bottom, uint left, uint right)
        {
            blockName = name;
            isSolid = solid;
            backFaceTexture = back;
            frontFaceTexture = front;
            topFaceTexture = top;
            bottomFaceTexture = bottom;
            leftFaceTexture = left;
            rightFaceTexture = right;
        }

        public BlockType(string name, bool solid, uint tex) : this(name, solid, tex, tex, tex, tex, tex, tex)
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
