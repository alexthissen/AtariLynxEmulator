#region Using Statements
using EmulatorClient.Windows.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace EmulatorClient.Windows
{
	/// <summary>
	/// The main class.
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Options options = new Options();

			bool parsed = CommandLine.Parser.Default.ParseArguments(args, options);

			if (!parsed)
			{
				Logger log = new Logger();
				log.Error(options.GetUsage());
				return;
			}
			using (var game = new EmulatorClient(options))
				game.Run();
		}
	}
}
