using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;
using System.Diagnostics;

namespace KillerApps.Emulation.Atari.Lynx
{
	// "The above listed adress ranges (except for FFF8 and FFF9) can, under control of the CPU, 
	// have RAM overlayed on them. These overlays are controlled by the bits in the hardware register at FFF9. 
	// Both Mikey and Suzy accept a write at those addresses but only Mikey responds to a read."
	public class MemoryManagementUnit: IMemoryAccess<ushort, byte>
	{
		public IMemoryAccess<ushort, byte> SuzySpace { get; private set; }
		public IMemoryAccess<ushort, byte> MikeySpace { get; private set; }
		public IMemoryAccess<ushort, byte> VectorSpace { get; private set; }
		public IMemoryAccess<ushort, byte> RomSpace { get; private set; }

		public MemoryMapControl MAPCNTL { get; set; }

		private SuzyChipset Suzy;
		private MikeyChipset Mikey;
		private IMemoryAccess<ushort, byte> Ram;
		private IMemoryAccess<ushort, byte> Rom;

		public MemoryManagementUnit(IMemoryAccess<ushort, byte> rom, IMemoryAccess<ushort, byte> ram, MikeyChipset mikey, SuzyChipset suzy)
		{
			this.Ram = ram;
			this.Rom = rom;
			this.Suzy = suzy;
			this.Mikey = mikey;
		}

		public void Poke(ushort address, byte value)
		{
			// Regular RAM 
			if (address < 0xfc00)
			{
				Ram.Poke(address, value);
				return;
			}

			// "These overlays are controlled by the bits in the hardware register at FFF9."
			if (address == 0xfff9) // Special case for memory map control register
			{
				// "Both Mikey and Suzy accept a write at those addresses but only Mikey responds to a read."
				MAPCNTL.ByteData = value;

				// "Any address space bit that is set to a 1 will cause 
				// its related address space to access RAM instead of the hardware or ROM normally accessed."
				SuzySpace = MAPCNTL.SuzySpaceDisabled ? Ram : Suzy;
				MikeySpace = MAPCNTL.MikeySpaceDisabled ? Ram : Mikey;
				VectorSpace = MAPCNTL.VectorSpaceDisabled ? Ram : Rom;
				RomSpace = MAPCNTL.RomSpaceDisabled ? Ram : Rom;

				return;
			}

			// "FFFE, FFFF CPU Interrupt Vector (RAM or ROM)
			// FFFC, FFFD CPU Reset Vector (RAM or ROM) 
			// FFFA, FFFB CPU NMI Vector (RAM or ROM)"
			if (address > 0xfffa) { VectorSpace.Poke(address, value); return; }

			// "FE00 thru FFF7 ROM Space"
			if (address >= 0xfe00) { RomSpace.Poke(address, value); return; }
			
			// "FD00 thru FDFF Mikey Space" 
			if (address >= 0xfd00) { MikeySpace.Poke(address, value); return; }
			
			// "FC00 thru FCFF Suzy Space"
			if (address >= 0xfc00) { SuzySpace.Poke(address, value); return; }
		}

		public byte Peek(ushort address)
		{
			// Regular RAM 
			if (address < 0xfc00)
			{
				return Ram.Peek(address);
			}

			// "These overlays are controlled by the bits in the hardware register at FFF9."
			if (address == 0xfff9) // Special case for memory map control register
			{
				// "Both Mikey and Suzy accept a write at those addresses but only Mikey responds to a read."
				// Since we will be passing regular RAM memory to Suzy, it is OK to always return value
				// because only Mikey will be going through this MMU.
				return MAPCNTL.ByteData;
			}

			// For details on address ranges see Poke() implementation
			if (address >= 0xfffa) { return VectorSpace.Peek(address); }
			if (address >= 0xfe00) { RomSpace.Peek(address); }
			if (address >= 0xfd00) { MikeySpace.Peek(address); }
			if (address >= 0xfc00) { SuzySpace.Peek(address); }

			Debug.WriteLine(String.Format("MemoryManagementUnit::Peek: Unknown address {0}", address));
			return 0;
		}
	}
}
