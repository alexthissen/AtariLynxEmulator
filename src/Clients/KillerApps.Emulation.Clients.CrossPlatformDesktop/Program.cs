using System;

namespace KillerApps.Emulation.Clients.CrossPlatformDesktop
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new EmulatorClient())
                game.Run();
        }
    }
}
