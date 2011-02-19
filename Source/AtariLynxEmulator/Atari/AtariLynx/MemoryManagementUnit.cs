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

		private MemoryMapControl MAPCTL { get; set; }

		private IMemoryAccess<ushort, byte> Suzy;
		private IMemoryAccess<ushort, byte> Mikey;
		private IMemoryAccess<ushort, byte> Ram;
		private IMemoryAccess<ushort, byte> Rom;

		public MemoryManagementUnit(IMemoryAccess<ushort, byte> rom, IMemoryAccess<ushort, byte> ram, IMemoryAccess<ushort, byte> mikey, IMemoryAccess<ushort, byte> suzy)
		{
			this.Ram = ram;
			this.Rom = rom;
			this.Suzy = suzy;
			this.Mikey = mikey;

			MAPCTL = new MemoryMapControl();
		}

		public void Reset()
		{
			// "(R/W)Mikey reset = 0,0,0,0,0,0,0,0
			// (W) Suzy reset x,x,x,x,x,x,x,0
			// (Only bit 0 is implemented)"
			ConfigureMemoryMapControl(0);			
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
				ConfigureMemoryMapControl(value);
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

		private void ConfigureMemoryMapControl(byte value)
		{
			// "Both Mikey and Suzy accept a write at those addresses but only Mikey responds to a read."
			MAPCTL.ByteData = value;

			// "Any address space bit that is set to a 1 will cause 
			// its related address space to access RAM instead of the hardware or ROM normally accessed."
			SuzySpace = MAPCTL.SuzySpaceDisabled ? Ram : Suzy;
			MikeySpace = MAPCTL.MikeySpaceDisabled ? Ram : Mikey;
			VectorSpace = MAPCTL.VectorSpaceDisabled ? Ram : Rom;
			RomSpace = MAPCTL.RomSpaceDisabled ? Ram : Rom;
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
				return MAPCTL.ByteData;
			}

			// For details on address ranges see Poke() implementation
			if (address >= 0xfffa) { return VectorSpace.Peek(address); }
			if (address >= 0xfe00) { return RomSpace.Peek(address); }
			if (address >= 0xfd00) { return MikeySpace.Peek(address); }
			if (address >= 0xfc00) { return SuzySpace.Peek(address); }

			Debug.WriteLine(String.Format("MemoryManagementUnit::Peek: Unknown address {0}", address));
			return 0;
		}
	}
}
