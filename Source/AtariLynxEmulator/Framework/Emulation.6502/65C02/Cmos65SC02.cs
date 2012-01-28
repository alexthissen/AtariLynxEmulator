using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;
using System.Diagnostics;

namespace KillerApps.Emulation.Processors
{
	public partial class Cmos65SC02 : Nmos6502
	{
		public Cmos65SC02(IMemoryAccess<ushort, byte> memory, Clock clock)
			: base(memory, clock)
		{	}

		protected override void ExecuteOpcode()
		{
			switch (Opcode)
			{
				case 0x00: BRK(); break;
				case 0x01: ZeroPageIndexedIndirectX(); ORA(); break;
				case 0x04: ZeroPage(); TSB(); break;
				case 0x05: ZeroPage(); ORA(); break;
				case 0x06: ZeroPage(); ASL(); break;
				case 0x08: PHP(); break;
				case 0x09: Immediate(); ORA(); break;
				case 0x0A: ASLA(); break;
				case 0x0C: Absolute(); TSB(); break;
				case 0x0D: Absolute(); ORA(); break;
				case 0x0E: Absolute(); ASL(); break;

				case 0x10: BPL(); break;
				case 0x11: ZeroPageIndirectIndexedY(); ORA(); break;
				case 0x12: ZeroPageIndirect(); ORA(); break;
				case 0x14: ZeroPage(); TRB(); break;
				case 0x15: ZeroPageX(); ORA(); break;
				case 0x16: ZeroPageX(); ASL(); break;
				case 0x18: CLC(); break;
				case 0x19: AbsoluteY(); ORA(); break;
				case 0x1A: INCA(); break;
				case 0x1C: Absolute(); TRB(); break;
				case 0x1D: AbsoluteX(); ORA(); break;
				case 0x1E: AbsoluteX(); ASL(); break;

				case 0x20: Absolute(); JSR(); break;
				case 0x21: ZeroPageIndexedIndirectX(); AND(); break;
				case 0x24: ZeroPage(); BIT(); break;
				case 0x25: ZeroPage(); AND(); break;
				case 0x26: ZeroPage(); ROL(); break;
				case 0x28: PLP(); break;
				case 0x29: Immediate(); AND(); break;
				case 0x2A: ROLA(); break;
				case 0x2C: Absolute(); BIT(); break;
				case 0x2D: Absolute(); AND(); break;
				case 0x2E: Absolute(); ROL(); break;

				case 0x30: BMI(); break;
				case 0x31: ZeroPageIndirectIndexedY(); AND(); break;
				case 0x32: ZeroPageIndirect(); AND(); break;
				case 0x34: ZeroPageX(); BIT(); break;
				case 0x35: ZeroPageX(); AND(); break;
				case 0x36: ZeroPageX(); ROL(); break;
				case 0x38: SEC(); break;
				case 0x39: AbsoluteY(); AND(); break;
				case 0x3A: DECA(); break;
				case 0x3C: AbsoluteX(); BIT(); break;
				case 0x3D: AbsoluteX(); AND(); break;
				case 0x3E: AbsoluteX(); ROL(); break;

				case 0x40: RTI(); break;
				case 0x41: ZeroPageIndexedIndirectX(); EOR(); break;
				case 0x45: ZeroPage(); EOR(); break;
				case 0x46: ZeroPage(); LSR(); break;
				case 0x48: PHA(); break;
				case 0x49: Immediate(); EOR(); break;
				case 0x4A: LSRA(); break;
				case 0x4C: Absolute(); JMP(); break;
				case 0x4D: Absolute(); EOR(); break;
				case 0x4E: Absolute(); LSR(); break;

				case 0x50: BVC(); break;
				case 0x51: ZeroPageIndirectIndexedY(); EOR(); break;
				case 0x52: ZeroPageIndirect(); EOR(); break;
				case 0x55: ZeroPageX(); EOR(); break;
				case 0x56: ZeroPageX(); LSR(); break;
				case 0x58: CLI(); break;
				case 0x59: AbsoluteY(); EOR(); break;
				case 0x5A: PHY(); break;
				case 0x5D: AbsoluteX(); EOR(); break;
				case 0x5E: AbsoluteX(); LSR(); break;

				case 0x60: RTS(); break;
				case 0x61: ZeroPageIndexedIndirectX(); ADC(); break;
				case 0x64: ZeroPage(); STZ(); break;
				case 0x65: ZeroPage(); ADC(); break;
				case 0x66: ZeroPage(); ROR(); break;
				case 0x68: PLA(); break;
				case 0x69: Immediate(); ADC(); break;
				case 0x6A: RORA(); break;
				// Bug in Indirect absolute addressing fixed for 65C02
				case 0x6C: AbsoluteIndirect(); JMP(); break;
				case 0x6D: Absolute(); ADC(); break;
				case 0x6E: Absolute(); ROR(); break;

				case 0x70: BVS(); break;
				case 0x71: ZeroPageIndirectIndexedY(); ADC(); break;
				case 0x72: ZeroPageIndirect(); ADC(); break;
				case 0x74: ZeroPageX(); STZ(); break;
				case 0x75: ZeroPageX(); ADC(); break;
				case 0x76: ZeroPageX(); ROR(); break;
				case 0x78: SEI(); break;
				case 0x79: AbsoluteY(); ADC(); break;
				case 0x7A: PLY(); break;
				case 0x7C: AbsoluteIndexedIndirectX(); JMP(); break;
				case 0x7D: AbsoluteX(); ADC(); break;
				case 0x7E: AbsoluteX(); ROR(); break;

				case 0x80: BRA(); break;
				case 0x81: ZeroPageIndexedIndirectX(); STA(); break;
				case 0x84: ZeroPage(); STY(); break;
				case 0x85: ZeroPage(); STA(); break;
				case 0x86: ZeroPage(); STX(); break;
				case 0x88: DEY(); break;
				case 0x89: Immediate(); BITImmediate(); break;
				case 0x8A: TXA(); break;
				case 0x8C: Absolute(); STY(); break;
				case 0x8D: Absolute(); STA(); break;
				case 0x8E: Absolute(); STX(); break;

				case 0x90: BCC(); break;
				case 0x91: ZeroPageIndirectIndexedY(); STA(); break;
				case 0x92: ZeroPageIndirect(); STA(); break;
				case 0x94: ZeroPageX(); STY(); break;
				case 0x95: ZeroPageX(); STA(); break;
				case 0x96: ZeroPageY(); STX(); break;
				case 0x98: TYA(); break;
				case 0x99: AbsoluteY(); STA(); break;
				case 0x9A: TXS(); break;
				case 0x9C: Absolute(); STZ(); break;
				case 0x9D: AbsoluteX(); STA(); break;
				case 0x9E: AbsoluteX(); STZ(); break;

				case 0xA0: Immediate(); LDY(); break;
				case 0xA1: ZeroPageIndexedIndirectX(); LDA(); break;
				case 0xA2: Immediate(); LDX(); break;
				case 0xA4: ZeroPage(); LDY(); break;
				case 0xA5: ZeroPage(); LDA(); break;
				case 0xA6: ZeroPage(); LDX(); break;
				case 0xA8: TAY(); break;
				case 0xA9: Immediate(); LDA(); break;
				case 0xAA: TAX(); break;
				case 0xAC: Absolute(); LDY(); break;
				case 0xAD: Absolute(); LDA(); break;
				case 0xAE: Absolute(); LDX(); break;

				case 0xB0: BCS(); break;
				case 0xB1: ZeroPageIndirectIndexedY(); LDA(); break;
				case 0xB2: ZeroPageIndirect(); LDA(); break;
				case 0xB4: ZeroPageX(); LDY(); break;
				case 0xB5: ZeroPageX(); LDA(); break;
				case 0xB6: ZeroPageY(); LDX(); break;
				case 0xB8: CLV(); break;
				case 0xB9: AbsoluteY(); LDA(); break;
				case 0xBA: TSX(); break;
				case 0xBC: AbsoluteX(); LDY(); break;
				case 0xBD: AbsoluteX(); LDA(); break;
				case 0xBE: AbsoluteY(); LDX(); break;

				case 0xC0: Immediate(); CPY(); break;
				case 0xC1: ZeroPageIndexedIndirectX(); CMP(); break;
				case 0xC4: ZeroPage(); CPY(); break;
				case 0xC5: ZeroPage(); CMP(); break;
				case 0xC6: ZeroPage(); DEC(); break;
				case 0xC8: INY(); break;
				case 0xC9: Immediate(); CMP(); break;
				case 0xCA: DEX(); break;
				case 0xCB: WAI(); break;
				case 0xCC: Absolute(); CPY(); break;
				case 0xCD: Absolute(); CMP(); break;
				case 0xCE: Absolute(); DEC(); break;

				case 0xD0: BNE(); break;
				case 0xD1: ZeroPageIndirectIndexedY(); CMP(); break;
				case 0xD2: ZeroPageIndirect(); CMP(); break;
				case 0xD5: ZeroPageX(); CMP(); break;
				case 0xD6: ZeroPageX(); DEC(); break;
				case 0xD8: CLD(); break;
				case 0xD9: AbsoluteY(); CMP(); break;
				case 0xDA: PHX(); break;
				case 0xDB: STP(); break;
				case 0xDD: AbsoluteX(); CMP(); break;
				case 0xDE: AbsoluteX(); DEC(); break;

				case 0xE0: Immediate(); CPX(); break;
				case 0xE1: ZeroPageIndexedIndirectX(); SBC(); break;
				case 0xE4: ZeroPage(); CPX(); break;
				case 0xE5: ZeroPage(); SBC(); break;
				case 0xE6: ZeroPage(); INC(); break;
				case 0xE8: INX(); break;
				case 0xE9: Immediate(); SBC(); break;
				case 0xEA: NOP(); break;
				case 0xEC: Absolute(); CPX(); break;
				case 0xED: Absolute(); SBC(); break;
				case 0xEE: Absolute(); INC(); break;

				case 0xF0: BEQ(); break;
				case 0xF1: ZeroPageIndirectIndexedY(); SBC(); break;
				case 0xF2: ZeroPageIndirect(); SBC(); break;
				case 0xF5: ZeroPageX(); SBC(); break;
				case 0xF6: ZeroPageX(); INC(); break;
				case 0xF8: SED(); break;
				case 0xF9: AbsoluteY(); SBC(); break;
				case 0xFA: PLX(); break;
				case 0xFD: AbsoluteX(); SBC(); break;
				case 0xFE: AbsoluteX(); INC(); break;

				default:
					break;
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
