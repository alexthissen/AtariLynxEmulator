using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	// "Mikey Parallel Data(sort of a R/W) 8 bits of general purpose I/O data"
	// "In the beginning, there was a general purpose 8 bit I/O port. As pins on Mikey became 
	// unavailable, the number of bits was reduced. 
	// Now all we have are the 5 bits of IODAT and they are not even pure read/write."
	public class ParallelData
	{
		public ParallelData(byte initialData)
		{
			// "B7 = NC
			//	B6 = NC
			//	B5 = NC"
			// "The other 3 bits in the byte are not connected to anything specific. 
			// Don't depend on them being any particular value. "
			ByteData = (byte)(initialData & 0x1f);
		}

		// TODO: Implement actual logic
		public byte ByteData { get; set; }

		// "This bit detects the presence of a powered plug. The ROM sets it to an output, 
		// so the system code must set it to an input."
		public bool ExternalPower { get; set; }

		// "This bit must be set to an output. It has 2 functions. One is that it is the data pin for 
		// the shifter that holds the cartridge address. The other is that it controls power 
		// to the cartridge. Power is on when the bit is low, power is off when the bit is high."
		public bool CartAddressData { get; set; }
		public bool CartPowerOff { get; set; }

		// "This bit must be set to an output. ln addition, the data value of this bit must be set to 1. This bit controls the rest period of the LCD display."
		public bool Rest { get; set; }

		// "This bit can be an input or an output. In its current use, it is the write enable line 
		// for writeable elements in the cartridge. It can also be used as an input from the 
		// cartridge such as 'audio in' for 'talking-listening' games. 
		// Whether it is set to input or output, the value read on this pin will depend on the 
		// electronics in the cartridge that is driving it."
		public bool AudioIn { get; set; }

		// "This bit must be set to an input. It detects the presence of a plug in the expansion connector."
		public bool NoExpansion { get; set; }
	}
}
