using System;

using SimpleEngine.Core;
namespace SimpleEngine
{
    class Program
    {
        public static void Main(string[] args) 
        {
            using (Game game = new Game(1440, 900, "SimpleEngine"))
            {
                game.Run();
            }
        }
    }
}

