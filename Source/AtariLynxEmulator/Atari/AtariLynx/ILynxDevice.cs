using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Processors;

namespace KillerApps.Emulation.Atari.Lynx
{
	public interface ILynxDevice : IResetable
	{
		RomCart Cartridge { get; set; }
		Mikey Mikey { get; }
		Suzy Suzy { get; }
		Cmos65SC02 Cpu { get; }
		Clock SystemClock { get; }
		bool CartridgePowerOn { get; set; }
		ulong NextTimerEvent { get; set; }
		Ram64KBMemory Ram { get; }
		RomBootMemory Rom { get; }
	}
}
