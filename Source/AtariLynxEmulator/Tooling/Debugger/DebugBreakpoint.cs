using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Atari.Lynx.Debugger
{
	public class DebugBreakpoint
	{
		public DebugBreakpoint(ushort address, byte code)
		{
			this.Address = address;
			this.Code = code;
		}

		public ushort Address { get; private set; }
		public byte Code { get; private set; }
	}
}
