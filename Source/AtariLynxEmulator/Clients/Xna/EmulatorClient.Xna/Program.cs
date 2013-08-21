using System;

namespace KillerApps.Gaming.Atari.Xna
{
#if WINDOWS || XBOX
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			using (EmulatorClient game = new EmulatorClient())
			{
				game.CommandLine = args;
				game.Run();
			}
		}
	}
#endif
}
