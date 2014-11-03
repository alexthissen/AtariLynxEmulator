using System;
using System.Runtime.InteropServices;

namespace KillerApps.Emulation.Atari.Lynx.Debugger
{
	public class Logger
	{
		public Logger()
		{
			AttachConsole(-1);
		}

		private static Logger logger;
		public static Logger Current
		{
			get { return logger ?? (logger = new Logger()); }
		}

		[DllImport("kernel32.dll")]
		static extern bool AttachConsole(int dwProcessId);

		public void Info(string message)
		{
			ToConsole(message, ConsoleColor.DarkGray);
		}

		public void Warning(string message)
		{
			ToConsole(message, ConsoleColor.DarkBlue);
		}

		public void Error(string message)
		{
			ToConsole(message, ConsoleColor.Red);
		}

		private void ToConsole(string message, ConsoleColor color = ConsoleColor.White)
		{
			ConsoleColor restoreColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.ffff") + " - " + message);
			Console.ForegroundColor = restoreColor;
		}
	}
}

