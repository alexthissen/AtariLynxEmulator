using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class RomCartMemoryBank
	{
		internal byte[] cartBank;
		private ulong maskBank;
		private int shiftCount;
		private int countMask;
		
		public RomCartType CartType { get; private set; }
		public bool WriteEnabled { get; set; }
		
		public RomCartMemoryBank(int bankSize)
		{
			Initialize(bankSize);
			if (CartType != RomCartType.Unused) cartBank = new byte[maskBank + 1];
		}

		public int Size { get { return (int)(maskBank + 1); } }

		private void Initialize(int bankSize)
		{
			switch (bankSize)
			{
				case 0x000:
					CartType = RomCartType.Unused;
					maskBank = 0;
					shiftCount = 0;
					countMask = 0;
					break;
				case 0x100: // 64K
					CartType = RomCartType.ReadOnly64KB;
					maskBank = 0x00ffff;
					// "A 64k byte ROM Cart will have 8 bits of counted address and 8 bits of shifted address."
					countMask = 0x0ff;
					shiftCount = 8;
					break;
				case 0x200: // 128K
					CartType = RomCartType.ReadOnly128KB;
					maskBank = 0x01ffff;
					// "A 128k byte ROM Cart will have 9 bits of counted address and 8 bits of shifted address."
					countMask = 0x1ff;
					shiftCount = 9;
					break;
				case 0x400: // 256K
					CartType = RomCartType.ReadOnly256KB;
					maskBank = 0x03ffff;
					shiftCount = 10;
					countMask = 0x3ff;
					break;
				case 0x800:
					// "The maximum address size is (8+11) 19 bits which equates to 1/2 megabyte of ROM Cart 
					// address space. Since there are 2 strobes available to the cart, there is a total of 
					// 1 megabyte of address space without additional hardware support. "
					CartType = RomCartType.ReadOnly512KB;
					maskBank = 0x07ffff;
					shiftCount = 11;
					countMask = 0x7ff;
					break;
				default:
					LynxException ex = new LynxException("The size of the image is not recognized.");
					throw ex;
			}
		}

		public void Poke(int shiftRegister, int counter, byte value)
		{
			if (WriteEnabled)
			{
				ulong address = GetAddress(shiftRegister, counter);
				cartBank[address & maskBank] = value;
			}
		}

		internal ulong GetAddress(int shiftRegister, int counter)
		{
			// "A particular ROM Cart will be wired to the address generator such that the upper 8 bits 
			// of its address will come from the 8 bit shift register and the remaining lower bits of 
			// its address will come from the lower bits of the counter."
			ulong address = (ulong)(shiftRegister << shiftCount) + (ulong)(counter & countMask);
			return address;
		}

		public byte Peek(int shiftRegister, int counter)
		{
			ulong address = GetAddress(shiftRegister, counter);
			byte data = cartBank[address & maskBank];
			return data;
		}

		public void Load(Stream stream)
		{
			// TODO: More error handling on sizes
			int bytesRead = stream.Read(cartBank, 0, cartBank.Length);
		}
	}
}
