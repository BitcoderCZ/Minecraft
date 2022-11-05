using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public static class GUI
    {
        public static void Init()
        {
            Texture.CreateUITA();
        }

        public static void Render(Shader uiShader)
        {
            uiShader.Bind();
        }
    }
}
