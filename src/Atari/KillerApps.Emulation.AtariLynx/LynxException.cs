using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Processors;

namespace KillerApps.Emulation.AtariLynx
{
	public class LynxException : EmulationException
	{
		public LynxException() { }
		public LynxException(string message) : base(message) { }
		public LynxException(string message, Exception inner) : base(message, inner) { }
	}
}
