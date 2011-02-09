using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;
using System.Diagnostics;

namespace KillerApps.Emulation.Atari.Lynx
{
	// "... and Suzy is only a sprite generation engine. Some non-sprite functions (the switch readers 
	// and the ROM reader) are in Suzy due to pin limitations. In addition the math functions are part 
	// of Suzys sprite engine."
	public class SuzyChipset : IMemoryAccess<ushort, byte>
	{
		public const int SUZY_START = 0xfc00;
		public const int SUZY_SIZE = 0x100;
		public const int SCREEN_WIDTH = 160;
		public const int SCREEN_HEIGHT = 102;
		public const int SPRITE_READWRITE_CYCLE = 3;

		public const int SUZYHARDWARE_READ = 5;
		public const int SUZYHARDWARE_WRITE = 5;
		public const int AVAILABLEHARDWARE_READWRITE = 5;

		// "The video and refresh generators in Mikey and the sprite engine in Suzy see the entire 64K byte range as RAM."
		private IMemoryAccess<ushort, byte> Ram;

		public SuzyChipset(IMemoryAccess<ushort, byte> ram)
		{
			this.Ram = ram;
		}

		public ulong PaintSprites() 
		{
			Debug.WriteLineIf(true, "SuzyChipset::PaintSprites");
			return 0; 
		}

		public void Poke(ushort address, byte value)
		{
			throw new NotImplementedException();
		}

		public byte Peek(ushort address)
		{
			throw new NotImplementedException();
		}
	}
}
