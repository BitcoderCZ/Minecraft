using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    static class Program
    {
        public static Window Window;

        static void Main(string[] args)
        {
            Window = new Window();
            Window.Run(60d);
        }
    }
}
