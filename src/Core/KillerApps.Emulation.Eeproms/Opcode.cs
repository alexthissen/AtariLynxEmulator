using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Eeproms
{
	public enum Opcode
	{
		ERASE = 0xC,	// 11 ... AN ... A1 A0
		ERAL = 0x2,		// 00 1  0  X  X  X  X
		EWDS = 0x0,		// 00 0  0  X  X  X  X
		EWEN = 0x3,		// 00 1  1  X  X  X  X
		READ = 0x8,		// 10 ... AN ... A1 A0
		WRITE = 0x4,	// 01 ... AN ... A1 A0
		WRAL = 0x1		// 00 0  1  X  X  X  X
	};
}
