using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    [Serializable]
    public class Settings
    {
        // Performance
        public int RenderDistance { get; set; }
        // Accesibility
        public float GUIScale { get; set; }
        // Controls
        public float MouseSensitivity { get; set; }
        // World Gen
        public int Seed { get; set; }
        // Other
        public bool AnimatedChunks { get; set; }
        public float AnimatedChunksSpeed { get; set; }

        public Settings()
        {
            RenderDistance = 3;
            GUIScale = 2.5f;
            MouseSensitivity = 1f;
            Seed = -1;
            AnimatedChunks = true;
            AnimatedChunksSpeed = 2f;
        }
    }
}
