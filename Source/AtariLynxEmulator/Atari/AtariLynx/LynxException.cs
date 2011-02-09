using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;
using System.Runtime.Serialization;

namespace KillerApps.Emulation.Atari.Lynx
{
	[Serializable]
	public class LynxException : EmulationException
	{
		public LynxException() { }
		public LynxException(string message) : base(message) { }
		public LynxException(string message, Exception inner) : base(message, inner) { }
		protected LynxException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
