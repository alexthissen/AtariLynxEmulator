using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmulatorClient.Windows.Tools
{
	public class Options
	{
		[Option('r', "rom", Required = true, HelpText = "ROM.")]
		public string ROMFilename { get; set; }

		[Option('b', "bios", Required = true, HelpText = "BIOS.")]
		public string BiosFilename { get; set; }

		[Option('m', "magnification", DefaultValue = 1, HelpText = "Magnification.")]
		public int Magnification { get; set; }

		[Option('s', "server", DefaultValue = false, HelpText = "Set ComLynx TCP server mode.")]
		public bool Server { get; set; }

		[Option('c', "client", DefaultValue = false, HelpText = "Set ComLynx TCP client mode.")]
		public bool Client { get; set; }

		[Option('p', "port", DefaultValue = 0, HelpText = "Server's TCP port.")]
		public int Port { get; set; }

		[Option('i', "ip", DefaultValue = null, HelpText = "Server's IP address.")]
		public string IP { get; set; }

		[Option('n', "name", DefaultValue = null, HelpText = "Pipe name.")]
		public string PipeName { get; set; }

		[Option('u', "ui", DefaultValue = false, HelpText = "Start UI.")]
		public bool UI { get; set; }

		[Option('o', "com", DefaultValue = 0, HelpText = "COM port number.")]
		public int ComPort { get; set; }

		[ParserState]
		public IParserState LastParserState { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}
}
