using System;

namespace KillerApps.Gaming.Atari
{
#if WINDOWS || XBOX
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			using (PcesGame game = new PcesGame())
			{
				game.CommandLine = args;
				game.Run();
			}
		}
	}
#endif
}