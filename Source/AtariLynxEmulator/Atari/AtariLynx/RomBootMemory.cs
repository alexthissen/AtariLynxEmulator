using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;
using System.IO;
using System.Diagnostics;

namespace KillerApps.Emulation.Atari.Lynx
{
	// "Its main and perhaps only function is to read in the initial data from the data input system 
	// and then execute that data."
	public class RomBootMemory : IMemoryAccess<ushort, byte>
	{
		// "The system ROM is embedded in Mikey. Its size is 512 bytes."
		public const ushort ROM_SIZE = 0x200;
		public const ushort ROM_ADDRESS_MASK = 0x01ff;
		public const byte DEFAULT_ROM_CONTENTS = 0x88;
		public const ushort ROM_BASEADDRESS = 0xfe00;
		
		private byte[] romData = new byte[ROM_SIZE];

		public RomBootMemory()
		{
			for (int index = 0; index < ROM_SIZE; index++) romData[index] = DEFAULT_ROM_CONTENTS;
		}

		public void LoadBootImage(Stream stream)
		{
			int bytesRead = stream.Read(romData, 0, ROM_SIZE);
			if (bytesRead != ROM_SIZE)
				throw new LynxException("Stream did not have exact size for ROM contents.");
			if (!VerifyBootImage())
				throw new LynxException("Boot image file appears to be fake.");
		}

		private bool VerifyBootImage()
		{
			byte[] romCheck = new byte[16] 
				{ 
					0x38, 0x80, 0x0a, 0x90, 0x04, 0x8e, 0x8b, 0xfd,
					0x18, 0xe8, 0x8e, 0x87, 0xfd, 0xa2, 0x02, 0x8e
				};
			for (int index = 0; index < romCheck.Length; index++)
			{
				if (romCheck[index] != romData[index]) return false;
			}
			return true;
		}

		public void Poke(ushort address, byte value)
		{
#if LYNXDEBUG
			throw new NotSupportedException("Writing to ROM is not support");
#endif
		}

		public byte Peek(ushort address)
		{
			Debug.Assert((address - ROM_BASEADDRESS) > 0, 
					String.Format("RomMemory::Peek: Address {0:X4} not in correct range for ROM.", address));
			return romData[address - ROM_BASEADDRESS];
		}

		public void Reset()
		{
			// Nothing to do
		}
	}
}
