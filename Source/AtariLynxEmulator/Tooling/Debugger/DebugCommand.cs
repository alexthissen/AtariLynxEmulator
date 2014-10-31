using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Atari.Lynx.Debugger
{
	public enum DebugCommand : byte
	{
		Continue = 0x82,
		ReceiveRegisters = 0x83,
		WriteMemory = 0x84,
		ReadMemory = 0x85,
		SendRegisters = 0x86,
		DownloadScreen = 0x87,
	}
}
