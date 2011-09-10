using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	// "FFF9 = MAPCTL. Memory Map Control
	// (R/W) Mikey reset = 0,0,0,0,0,0,0,0
	// (W) Suzy reset x,x,x,x,x,x,x,0 (Only bit 0 is implemented)"
	public class MemoryMapControl : IResetable
	{
		public byte ByteData { get; set; }

		public void Reset()
		{
			// "All 8 bits are set to 0 at reset."
			ByteData = 0;
		}

		// "B0 = FC00 -> FCFF, Suzy Space"
		public bool SuzySpaceDisabled { get { return (ByteData & 0x01) == 0x01; } }
		// "B1 = FD00 -> FDFF, Mikey Space"
		public bool MikeySpaceDisabled { get { return (ByteData & 0x02) == 0x02; } }
		// "B2 = FE00 -> FFF7, ROM Space"
		public bool RomSpaceDisabled { get { return (ByteData & 0x04) == 0x04; } }
		// "B3 = FFFA -> FFFF, Vector Space"
		public bool VectorSpaceDisabled { get { return (ByteData & 0x08) == 0x08; } } 

		// "B7 = sequential disable. If set, the CPU will always use full cycles (5 ticks min), 
		// never a sequential cycle (4 ticks)."
		public bool SequentialDisable { get { return (ByteData & 0x80) == 0x80; } }
	}
}
