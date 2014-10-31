using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace EmulatorClient.Windows.Tools
{
	class Logger
	{
		public Logger()
		{
			AttachConsole(-1);
		}

		[DllImport("kernel32.dll")]
		static extern bool AttachConsole(int dwProcessId);

		public void Info(string message)
		{
			ToConsole(message);
		}

		public void Warning(string message)
		{
			ToConsole(message);
		}

		public void Error(string message)
		{
			ToConsole(message);
		}

		private void ToConsole(string message)
		{
#if DEBUG
			Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.ffff") + " - " + message);
#endif
		}
	}
}
