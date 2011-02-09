using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace KillerApps.Emulation.Core
{
	[Serializable]
	public class EmulationException : Exception
	{
		public EmulationException() { }
		public EmulationException(string message) : base(message) { }
		public EmulationException(string message, Exception inner) : base(message, inner) { }
		protected EmulationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
