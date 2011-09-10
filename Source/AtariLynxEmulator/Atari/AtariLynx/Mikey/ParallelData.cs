using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	// "Mikey Parallel Data (sort of a R/W) 8 bits of general purpose I/O data"
	// "In the beginning, there was a general purpose 8 bit I/O port. As pins on Mikey became 
	// unavailable, the number of bits was reduced. 
	// Now all we have are the 5 bits of IODAT and they are not even pure read/write."
	public class ParallelData
	{
		public const byte RestMask = 0x08;
		public const byte AudioInMask = 0x10;
		public const byte NoExpansionMask = 0x04;
		public const byte CartAddressDataMask = 0x02;
		public const byte ExternalPowerMask = 0x01;
		
		public ParallelData(byte initialData)
		{
			// "B7 = NC
			//	B6 = NC
			//	B5 = NC"
			// "The other 3 bits in the byte are not connected to anything specific. 
			// Don't depend on them being any particular value."
			// TODO: Initialize with mask of 0x1f
			ByteData = initialData;
		}

		private byte byteData;

		public byte ByteData 
		{
			get { return byteData; }
			set { byteData = (byte)(value & 0x1f); }
		}

		// "This bit detects the presence of a powered plug. The ROM sets it to an output, 
		// so the system code must set it to an input."
		// "B0 = External Power input"
		public bool ExternalPower 
		{ 
			get { return (ByteData & ExternalPowerMask) == ExternalPowerMask;  } 
		}

		// "This bit must be set to an output. It has 2 functions. One is that it is the data pin for 
		// the shifter that holds the cartridge address. The other is that it controls power 
		// to the cartridge. Power is on when the bit is low, power is off when the bit is high."
		// "B1 = Cart Address Data output (0 turns cart power on)"
		public bool CartAddressData 
		{
			get { return (ByteData & CartAddressDataMask) == CartAddressDataMask; }
		}

		// "This bit must be set to an output. It has 2 functions. ... The other is that it controls power 
		// to the cartridge. Power is on when the bit is low, power is off when the bit is high."
		public bool CartPowerOff 
		{ 
			get 
			{ 
				// Functionality of Cart power off is same as for cart address data
				return CartAddressData; 
			}
		}

		// "This bit must be set to an output. ln addition, the data value of this bit must be set to 1. This bit controls the rest period of the LCD display."
		// "B3 = rest output"
		public bool Rest 
		{ 
			get { return (ByteData & RestMask) == RestMask; }
			//set
			//{
			//  ByteData &= RestMask ^ 0xFF;
			//  ByteData |= value ? RestMask : (byte)0;
			//}
		}

		// "This bit can be an input or an output. In its current use, it is the write enable line 
		// for writeable elements in the cartridge. It can also be used as an input from the 
		// cartridge such as 'audio in' for 'talking-listening' games. 
		// Whether it is set to input or output, the value read on this pin will depend on the 
		// electronics in the cartridge that is driving it."
		// "B4 = audin input"
		public bool AudioIn 
		{
			get { return (ByteData & AudioInMask) == AudioInMask; }
			set 
			{
 				// TODO: Implement audio in writing
				throw new NotImplementedException(); 
			}
		}

		// "This bit must be set to an input. It detects the presence of a plug in the expansion connector."
		// "B2 = noexp input"
		public bool NoExpansion
		{
			get { return (ByteData & NoExpansionMask) == NoExpansionMask; }
		}
	}
}
