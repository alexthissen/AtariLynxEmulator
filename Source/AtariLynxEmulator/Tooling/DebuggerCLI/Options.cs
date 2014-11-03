using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebuggerCLI
{
	public class Options
	{
		[Option('c', "com", Required = true, HelpText = "COM port number of serial device.")]
		public int ComPort { get; set; }

		[Option('b', "baudrate", Required = true, HelpText = "Baudrate for COM port.")]
		public int Baudrate { get; set; }

		[Option('p', "parity", Required = true, HelpText = "Parity for COM port.")]
		public Parity Parity { get; set; }

		[ParserState]
		public IParserState LastParserState { get; set; }
		public string GetUsage()
		{
			return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}

}
