using System;

using SimpleEngine.Core;
namespace SimpleEngine
{
    class Program
    {
        public static void Main(string[] args) 
        {
            using (Game game = new Game(800, 600, "SimpleEngine"))
            {
                game.Run();
            }
        }
    }
}

