using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Processors
{
	[Flags]
	public enum DisassemblyOutputOptions
	{
		None = 0x00,
		MnemonicInLowerCase = 0x01,
		IncludeSourceBytes = 0x02,
		IncludeAddress = 0x04
	}
}
