using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.Graphics
{
    public interface IRenderObject
    {
        Vector3 Position { get; }
        bool Active { get; set; }

        /*int vao { get; }
        int vbo { get; }
        int ebo { get; }*/

        void Render(Shader s);
    }
}
