using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;

namespace KillerApps.Emulation.Processors
{
	public partial class Cmos65SC02 : Nmos6502
	{
		public Cmos65SC02(IMemoryAccess<ushort, byte> memory, Clock clock)
			: base(memory, clock)
		{	}

		// TODO: Find way to hook in these new opcodes in NMOS6502 in a performant way
		public void Update()
		{
			switch (Opcode)
			{
				case 0x04: ZeroPage(); TSB(); break;
				case 0x0C: Absolute(); TSB(); break;

				case 0x12: ZeroPageIndirect(); ORA(); break;
				case 0x14: ZeroPage(); TRB(); break;
				case 0x1A: INA(); break;
				case 0x1C: Absolute(); TRB(); break;

				case 0x32: ZeroPageIndirect(); AND(); break;
				case 0x34: ZeroPageX(); BIT(); break;
				case 0x3A: DEA(); break;
				case 0x3C: AbsoluteX(); BIT(); break;

				case 0x52: ZeroPageIndirect(); EOR(); break;
				case 0x5A: PHY(); break;

				case 0x64: ZeroPage(); STZ(); break;
				// Bug in Indirect absolute addressing fixed for 65C02
				case 0x6C: AbsoluteIndirect(); JMP(); break;

				case 0x72: ZeroPageIndirect(); ADC(); break;
				case 0x74: ZeroPageX(); STZ(); break;
				case 0x7A: PLY(); break;
				case 0x7C: AbsoluteIndexedIndirectX(); JMP(); break;

				case 0x80: BRA(); break;
				case 0x89: Immediate(); BIT(); break;

				case 0x92: ZeroPageIndirect(); STA(); break;
				case 0x9C: Absolute(); STZ(); break;
				case 0x9E: AbsoluteX(); STZ(); break;

				case 0xB2: ZeroPageIndirect(); LDA(); break;

				case 0xCB: WAI(); break;

				case 0xD2: ZeroPageIndirect(); CMP(); break;
				case 0xDA: PHX(); break;
				case 0xDB: STP(); break;
				
				case 0xF2: ZeroPageIndirect(); SBC(); break;
				case 0xFA: PLX(); break;
			}
		}

		protected override void UpdateInterruptFlags()
		{
			// "It is important for the programmer to note that the interrupt-disable I flag is set, 
			// and that the decimal D flag is cleared on the 65C02 but not affected on the NMOS 6502"
			I = true; // Stop further interrupts
			D = false; // Clear decimal mode
		}
	}
}
