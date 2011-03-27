using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using KillerApps.Emulation.Core;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class SpriteEngine
	{
		public Word VIDADR, COLLADR;
		public Word STRETCH;
		public Word TILTACUM, TILT;
		public Word SPRDOFF;
		public Word SPRVPOS;
		public Word HSIZACUM, VSIZACUM;
		public Word TMPADR, SCBADR, PROCADR;

		public bool StretchingEnabled { get; set; }
		public bool SizingEnabled { get; set; }
		public bool TiltingEnabled { get; set; }

		// "The sprite engine consists of several 8 bit control registers, a 16 bit wide sprite 
		// control block register set and ALU, an address manipulator, an 8 byte deep source data FIFO, 
		// a 12 bit shift register for unpacking the data, a 16 nybble pen index palette, 
		// a pixel byte builder, an 8 word deep pixel data FIFO, a data merger, 
		// and assorted control logic."

		byte[] ramMemory = null;
		ShiftRegister shifter; // "a 12 bit shift register for unpacking the data"
		// "16 nybble (8 byte) pen index palette specific to each sprite"
		byte[] PenIndexPalette = new byte[16];
		// "... several 8 bit control registers, a 16 bit wide sprite control block register set"
		SpriteControlBlock scb = new SpriteControlBlock();
		SpriteDataUnpacker unpacker;

		public SpriteEngine(byte[] memory)
		{
			ramMemory = memory;
			shifter = new ShiftRegister(12); // "a 12 bit shift register for unpacking the data"
			unpacker = new SpriteDataUnpacker(shifter, ramMemory);

			TiltingEnabled = false;
			StretchingEnabled = false;
			SizingEnabled = false;
		}

		public ulong RenderSprites() { return 0; }
	}
}
