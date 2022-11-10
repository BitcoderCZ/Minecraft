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
            Console.WriteLine($"Main directory: {Environment.CurrentDirectory}/");
            Window = new Window();
            string version;
            try { version = OpenGLHelper.GetVersion(); } catch { version = "Failed to get version"; }
            Console.WriteLine($"OpenGL version: {version}, Min: 4.5.0");
            if (int.TryParse(version.Split(' ')[0].Split('.')[0], out int mainVer))
                if (mainVer < 4) {
                    Console.WriteLine("OpenGL version to low. Press any key to exit...");
                    Console.ReadKey(true);
                    Environment.Exit(2);
                } else if (int.TryParse(version.Split(' ')[0].Split('.')[1], out int subVer) && mainVer >= 4 && subVer < 5) {
                    Console.WriteLine("OpenGL version to low. Press any key to exit...");
                    Console.ReadKey(true);
                    Environment.Exit(2);
                }

            Window.Run(60d);
        }
    }
}
