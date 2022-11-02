using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    class Program
    {
        static void Main(string[] args)
        {
            Window window = new Window();
            window.Run(60d);
        }
    }
}
