using System;

namespace PaceWindows
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (PaceGame game = new PaceGame())
            {
                game.Run();
            }
        }
    }
#endif
}

