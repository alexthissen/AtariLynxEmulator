using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class SystemControlBits1
	{
		public const byte PowerMask = 0x02;
		public const byte CartAddressStrobeMask = 0x01;
		
		public SystemControlBits1(byte initialData)
		{
			ByteData = initialData;
		}

		public byte ByteData { private get; set; }

		public bool Power 
		{ 
			get 
			{
				return (ByteData & PowerMask) == PowerMask;
			}
		}

		public bool CartAddressStrobe 
		{
			get { return (ByteData & CartAddressStrobeMask) == CartAddressStrobeMask; }
		}
	}
}
