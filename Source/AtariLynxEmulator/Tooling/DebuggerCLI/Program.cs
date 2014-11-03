using CommandLine.Text;
using ConsoleCommandApi;
using KillerApps.Emulation.Atari.Lynx.Debugger;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebuggerCLI
{
	public class Program
	{
		private static DebugEngine engine;
		internal static DebugEngine Engine { get {  return engine ?? (engine = new DebugEngine()); } }
		static HelpText text = new HelpText(HeadingInfo.Default, Resources.CLIDescription);

		public static HelpText GetUsage(Options options)
		{
			text.AddDashesToOption = true;
			text.AdditionalNewLineAfterOption = false;
			text.AddOptions(options);
			HelpText.DefaultParsingErrorsHandler(options, text);
			return text;

			//return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
		}

		static void Main(string[] args)
		{
			Options options = new Options();
			bool parsed = CommandLine.Parser.Default.ParseArguments(args, options);
			
			if (!parsed)
			{
				GetUsage(options);
				Console.WriteLine(text);
				return;
			}

			Console.WriteLine(text);
			
			if (!TryAttach())
			{
				Logger.Current.Error(Resources.CouldNotAttachToDebuggee);
			}

			ConsoleCommander.Current.Prompt = "lynx:> ";
			ConsoleCommander.Current.Start();

			try
			{
				Engine.Detach();
			}
			catch { }
		}

		public static bool TryAttach()
		{
			try
			{
				Engine.Attach("COM3", 62500, Parity.Mark);
				return true;
			}
			catch { return false; }
		}
	}
}
