using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Core
{
	public class EmulationException : Exception
	{
		public EmulationException() { }
		public EmulationException(string message) : base(message) { }
		public EmulationException(string message, Exception inner) : base(message, inner) { }
	}
}
