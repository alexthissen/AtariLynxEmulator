using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx.Debugger
{
	public struct Registers
	{
		public byte A, X, Y, PS;
		public ushort PC;
		public byte SP;
	}
}
