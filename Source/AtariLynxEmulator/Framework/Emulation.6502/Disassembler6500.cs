using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;

namespace KillerApps.Emulation.Processors
{
	public class Disassembler6500
	{
		#region Instruction set
		public static Instruction6502[] InstructionSet = new Instruction6502[256]
			{
				new Instruction6502(0x00, "BRK", AddressingMode.Implicit),
				new Instruction6502(0x01, "ORA", AddressingMode.ZeroPageIndexedIndirectX),
				null,
				null,
				new Instruction6502(0x04, "TSB", AddressingMode.ZeroPage),
				new Instruction6502(0x05, "ORA", AddressingMode.ZeroPage),
				new Instruction6502(0x06, "ASL", AddressingMode.ZeroPage),
				null,
				new Instruction6502(0x08, "PHP", AddressingMode.Implicit), 
				new Instruction6502(0x09, "ORA", AddressingMode.Immediate),  
				new Instruction6502(0x0A, "ASL", AddressingMode.Accumulator), 
				null,
				new Instruction6502(0x0C, "TSB",AddressingMode.Accumulator),
				new Instruction6502(0x0D, "ORA",AddressingMode.Absolute),
				new Instruction6502(0x0E, "ASL",AddressingMode.Absolute), 
				null,
		
				new Instruction6502(0x10, "BPL",AddressingMode.Relative), 
				new Instruction6502(0x11, "ORA",AddressingMode.ZeroPageIndirectIndexedY), 
				new Instruction6502(0x12, "ORA",AddressingMode.ZeroPageIndirect),
				null,
				new Instruction6502(0x14, "TRB",AddressingMode.ZeroPage),
				new Instruction6502(0x15, "ORA",AddressingMode.ZeroPageX),  
				new Instruction6502(0x16, "ASL",AddressingMode.ZeroPageX), 
				null,
				new Instruction6502(0x18, "CLC", AddressingMode.Implicit), 
				new Instruction6502(0x19, "ORA",AddressingMode.AbsoluteY), 
				new Instruction6502(0x1A, "INC", AddressingMode.Accumulator), 
				null,
				new Instruction6502(0x1C, "TRB",AddressingMode.Absolute), 
				new Instruction6502(0x1D, "ORA",AddressingMode.AbsoluteX),  
				new Instruction6502(0x1E, "ASL",AddressingMode.AbsoluteX), 
				null,
		
				new Instruction6502(0x20, "AND",AddressingMode.ZeroPageIndexedIndirectX), 
				null,
				new Instruction6502(0x22, "JSR",AddressingMode.Absolute),  
				null,
				new Instruction6502(0x24, "BIT",AddressingMode.ZeroPage),  
				new Instruction6502(0x25, "AND",AddressingMode.ZeroPage), 
				new Instruction6502(0x26, "ROL",AddressingMode.ZeroPage), 
				null,
				new Instruction6502(0x28, "PLP",AddressingMode.Implicit),  
				new Instruction6502(0x29, "AND",AddressingMode.Immediate), 
				new Instruction6502(0x2A, "ROL",AddressingMode.Accumulator), 
				null,
				new Instruction6502(0x2C, "BIT",AddressingMode.Absolute),  
				new Instruction6502(0x2D, "AND",AddressingMode.Absolute), 
				new Instruction6502(0x2E, "ROL",AddressingMode.Absolute), 
				null,
		
				new Instruction6502(0x30, "BMI",AddressingMode.Relative),
				new Instruction6502(0x31, "AND",AddressingMode.ZeroPageIndirectIndexedY),
				new Instruction6502(0x32, "AND",AddressingMode.ZeroPageIndirect),
				null,
				new Instruction6502(0x34, "BIT", AddressingMode.ZeroPageX),
				new Instruction6502(0x35, "AND", AddressingMode.ZeroPageX), 
				new Instruction6502(0x36, "ROL", AddressingMode.ZeroPageX), 
				null,
				new Instruction6502(0x38, "SEC",AddressingMode.Implicit), 
				new Instruction6502(0x39, "AND",AddressingMode.AbsoluteY), 
				new Instruction6502(0x3A, "DEC",AddressingMode.Accumulator), 
				null,
				new Instruction6502(0x3C, "BIT",AddressingMode.AbsoluteX),
				new Instruction6502(0x3D, "AND",AddressingMode.AbsoluteX), 
				new Instruction6502(0x3E, "ROL",AddressingMode.AbsoluteX), 
				null,
		
				new Instruction6502(0x40, "RTI",AddressingMode.Implicit),
				new Instruction6502(0x41, "EOR",AddressingMode.ZeroPageIndexedIndirectX),
				null,
				null,
				null,
				new Instruction6502(0x45, "EOR",AddressingMode.ZeroPage),
				new Instruction6502(0x46, "LSR",AddressingMode.ZeroPage),
				null,
				new Instruction6502(0x48, "PHA",AddressingMode.Implicit), 
				new Instruction6502(0x49, "EOR",AddressingMode.Immediate), 
				new Instruction6502(0x4A, "LSR",AddressingMode.Accumulator), 
				null,
				new Instruction6502(0x4C, "JMP",AddressingMode.Absolute),
				new Instruction6502(0x4D, "EOR",AddressingMode.Absolute),
				new Instruction6502(0x4E, "LSR",AddressingMode.Absolute),
				null,
		
				new Instruction6502(0x50, "BVC" ,AddressingMode.Relative),
				new Instruction6502(0x51, "EOR",AddressingMode.ZeroPageIndirectIndexedY),
				new Instruction6502(0x52, "EOR",AddressingMode.ZeroPageIndirect),
				null,
				null,
				new Instruction6502(0x55, "EOR",AddressingMode.ZeroPageX),
				new Instruction6502(0x56, "LSR",AddressingMode.ZeroPageX),
				null,
				new Instruction6502(0x58, "CLI",AddressingMode.Implicit),
				new Instruction6502(0x59, "EOR",AddressingMode.AbsoluteY),
				new Instruction6502(0x5A, "PHY",AddressingMode.Implicit),
				null,
				null,
				new Instruction6502(0x5D, "EOR",AddressingMode.AbsoluteX),
				new Instruction6502(0x5E, "LSR",AddressingMode.AbsoluteX),
				null,
		
				new Instruction6502(0x60, "RTS",AddressingMode.Implicit),
				new Instruction6502(0x61, "ADC",AddressingMode.ZeroPageIndexedIndirectX),
				null,
				null,
				new Instruction6502(0x64, "STZ",AddressingMode.ZeroPage),
				new Instruction6502(0x65, "ADC",AddressingMode.ZeroPage),
				new Instruction6502(0x66, "ROR",AddressingMode.ZeroPage),
				null,
				new Instruction6502(0x68, "PLA",AddressingMode.Implicit),
				new Instruction6502(0x69, "ADC",AddressingMode.Immediate),
				new Instruction6502(0x6A, "ROR",AddressingMode.Accumulator),
				null,
				new Instruction6502(0x6C, "JMP",AddressingMode.AbsoluteIndirect),
				new Instruction6502(0x6D, "ADC",AddressingMode.Absolute),
				new Instruction6502(0x6E, "ROR",AddressingMode.Absolute),
				null,
		
				new Instruction6502(0x70, "BVS",AddressingMode.Relative),
				new Instruction6502(0x71, "ADC",AddressingMode.ZeroPageIndirectIndexedY),
				new Instruction6502(0x72, "ADC",AddressingMode.ZeroPageIndirect),
				null,
				new Instruction6502(0x74, "STZ",AddressingMode.ZeroPageX),
				new Instruction6502(0x75, "ADC",AddressingMode.ZeroPageX),
				new Instruction6502(0x76, "ROR",AddressingMode.ZeroPageX),
				null,
				new Instruction6502(0x78, "SEI",AddressingMode.Implicit), 
				new Instruction6502(0x79, "ADC",AddressingMode.AbsoluteY),
				new Instruction6502(0x7A, "PLY",AddressingMode.Implicit),
				null,
				new Instruction6502(0x7C, "JMP",AddressingMode.AbsoluteIndexedIndirectX),
				new Instruction6502(0x7D, "ADC",AddressingMode.AbsoluteX),
				new Instruction6502(0x7E, "ROR",AddressingMode.AbsoluteX),
				null,
		
				new Instruction6502(0x80, "BRA",AddressingMode.Relative),
				new Instruction6502(0x81, "STA",AddressingMode.ZeroPageIndexedIndirectX),
				null,
				null,
				new Instruction6502(0x84, "STY",AddressingMode.ZeroPage),
				new Instruction6502(0x85, "STA",AddressingMode.ZeroPage),
				new Instruction6502(0x86, "STX",AddressingMode.ZeroPage),
				null,
				new Instruction6502(0x88, "DEY",AddressingMode.Implicit),
				new Instruction6502(0x89, "BIT",AddressingMode.Immediate),
				new Instruction6502(0x8A, "TXA",AddressingMode.Implicit),
				null,
				new Instruction6502(0x8C, "STY",AddressingMode.Absolute),
				new Instruction6502(0x8D, "STA",AddressingMode.Absolute),
				new Instruction6502(0x8E, "STX",AddressingMode.Absolute),
				null,
		
				new Instruction6502(0x90, "BCC",AddressingMode.Relative),
				new Instruction6502(0x91, "STA",AddressingMode.ZeroPageIndirectIndexedY),
				new Instruction6502(0x92, "STA",AddressingMode.ZeroPageIndirect),
				null,
				new Instruction6502(0x94, "STY",AddressingMode.ZeroPageX),
				new Instruction6502(0x95, "STA",AddressingMode.ZeroPageX),
				new Instruction6502(0x96, "STX",AddressingMode.ZeroPageY),
				null,
				new Instruction6502(0x98, "TYA",AddressingMode.Implicit),
				new Instruction6502(0x99, "STA",AddressingMode.AbsoluteY),
				new Instruction6502(0x9A, "TXS",AddressingMode.Implicit),
				null,
				new Instruction6502(0x9C, "STZ",AddressingMode.Absolute),
				new Instruction6502(0x9D, "STA",AddressingMode.AbsoluteX),
				new Instruction6502(0x9E, "STZ",AddressingMode.AbsoluteX),
				null,
		
				new Instruction6502(0xA0, "LDY",AddressingMode.Immediate),
				new Instruction6502(0xA1, "LDA",AddressingMode.ZeroPageIndexedIndirectX),
				new Instruction6502(0xA2, "LDX",AddressingMode.Immediate),
				null,
				new Instruction6502(0xA4, "LDY",AddressingMode.ZeroPage),
				new Instruction6502(0xA5, "LDA",AddressingMode.ZeroPage),
				new Instruction6502(0xA6, "LDX",AddressingMode.ZeroPage),
				null,
				new Instruction6502(0xA8, "TAY",AddressingMode.Implicit),
				new Instruction6502(0xA9, "LDA",AddressingMode.Immediate),
				new Instruction6502(0xAA, "TAX",AddressingMode.Implicit),
				null,
				new Instruction6502(0xAC, "LDY",AddressingMode.Absolute),
				new Instruction6502(0xAD, "LDA",AddressingMode.Absolute),
				new Instruction6502(0xAE, "LDX",AddressingMode.Absolute),
				null,
		
				new Instruction6502(0xB0, "BCS",AddressingMode.Relative),
				new Instruction6502(0xB1, "LDA",AddressingMode.ZeroPageIndirectIndexedY),
				new Instruction6502(0xB2, "LDA",AddressingMode.ZeroPageIndirect),
				null,
				new Instruction6502(0xB4, "LDY",AddressingMode.ZeroPageX),
				new Instruction6502(0xB5, "LDA",AddressingMode.ZeroPageX),
				new Instruction6502(0xB6, "LDX",AddressingMode.ZeroPageY),
				null,
				new Instruction6502(0xB8, "CLV",AddressingMode.Implicit),
				new Instruction6502(0xB9, "LDA",AddressingMode.AbsoluteY),
				new Instruction6502(0xBA, "TSX",AddressingMode.Implicit),
				null,
				new Instruction6502(0xBC, "LDY",AddressingMode.AbsoluteX),
				new Instruction6502(0xBD, "LDA",AddressingMode.AbsoluteX),
				new Instruction6502(0xBE, "LDX",AddressingMode.AbsoluteY),
				null,
		
				new Instruction6502(0xC0, "CPY",AddressingMode.Immediate),
				new Instruction6502(0xC1, "CMP",AddressingMode.ZeroPageIndexedIndirectX),
				null,
				null,
				new Instruction6502(0xC4, "CPY",AddressingMode.ZeroPage),
				new Instruction6502(0xC5, "CMP",AddressingMode.ZeroPage),
				new Instruction6502(0xC6, "DEC",AddressingMode.ZeroPage),
				null,
				new Instruction6502(0xC8, "INY",AddressingMode.Implicit),
				new Instruction6502(0xC9, "CMP",AddressingMode.Immediate),
				new Instruction6502(0xCA, "DEX",AddressingMode.Implicit),
				null,
				new Instruction6502(0xCC, "CPY",AddressingMode.Absolute),
				new Instruction6502(0xCD, "CMP",AddressingMode.Absolute),
				new Instruction6502(0xCE, "DEC",AddressingMode.Absolute),
				null,
		
				new Instruction6502(0xD0, "BNE",AddressingMode.Relative),
				new Instruction6502(0xD1, "CMP",AddressingMode.ZeroPageIndirectIndexedY),
				new Instruction6502(0xD2, "CMP",AddressingMode.ZeroPageIndirect),
				null,
				null,
				new Instruction6502(0xD5, "CMP",AddressingMode.ZeroPageX),
				new Instruction6502(0xD6, "DEC",AddressingMode.ZeroPageX),
				null,
				new Instruction6502(0xD8, "CLD",AddressingMode.Implicit),
				new Instruction6502(0xD9, "CMP",AddressingMode.AbsoluteY),
				new Instruction6502(0xDA, "PHX",AddressingMode.Implicit),
				null,
				null,
				new Instruction6502(0xDD, "CMP",AddressingMode.AbsoluteX),
				new Instruction6502(0xDE, "DEC",AddressingMode.AbsoluteX),
				null,
		
				new Instruction6502(0xE0, "CPX",AddressingMode.Immediate),
				new Instruction6502(0xE1, "SBC",AddressingMode.ZeroPageIndexedIndirectX),
				null,
				null,
				new Instruction6502(0xE4, "CPX",AddressingMode.ZeroPage),
				new Instruction6502(0xE5, "SBC",AddressingMode.ZeroPage),
				new Instruction6502(0xE6, "INC",AddressingMode.ZeroPage),
				null,
				new Instruction6502(0xE8, "INX",AddressingMode.Implicit),
				new Instruction6502(0xE9, "SBC",AddressingMode.Immediate),
				new Instruction6502(0xEA, "NOP",AddressingMode.Implicit),
				null,
				new Instruction6502(0xEC, "CPX",AddressingMode.Absolute),
				new Instruction6502(0xED, "SBC",AddressingMode.Absolute),
				new Instruction6502(0xEE, "INC",AddressingMode.Absolute),
				null,
		
				new Instruction6502(0xF0, "BEQ",AddressingMode.Relative),
				new Instruction6502(0xF1, "SBC",AddressingMode.ZeroPageIndirectIndexedY),
				new Instruction6502(0xF2, "SBC",AddressingMode.ZeroPageIndirect),
				null,
				null,
				new Instruction6502(0xF5, "SBC",AddressingMode.ZeroPageX),
				new Instruction6502(0xF6, "INC",AddressingMode.ZeroPageX),
				null,
				new Instruction6502(0xF8, "SED",AddressingMode.Implicit),
				new Instruction6502(0xF9, "SBC",AddressingMode.AbsoluteY),
				new Instruction6502(0xFA, "PLX",AddressingMode.Implicit),
				null,
				null,
				new Instruction6502(0xFD, "SBC",AddressingMode.AbsoluteX),
				new Instruction6502(0xFE, "INC",AddressingMode.AbsoluteX),
				null
			};
		#endregion

		public int DisassembleSingleStatement(IMemoryAccess<ushort, byte> memory, ushort address, StringBuilder builder)
		{
			byte opcode = memory.Peek(address++);
			Instruction6502 instruction = InstructionSet[opcode];
			ushort operand16Bit = memory.PeekWord(address);
			byte operand8Bit = memory.Peek(address);

			switch (instruction.AddressingMode)
			{
				case AddressingMode.Accumulator: builder.AppendFormat("{0} A\n", instruction.Mnemonic); return 1;
				case AddressingMode.Implicit: builder.AppendFormat("{0}\n", instruction.Mnemonic); return 1;
						 
				case AddressingMode.Relative: 
					ushort jump = (ushort)(address + 2 + operand8Bit);
					if (operand8Bit < 0x80) jump -= 256;
					builder.AppendFormat("{0} ${1:X4}\n", instruction.Mnemonic, jump); 
					return 2;
			
				case AddressingMode.Immediate: builder.AppendFormat("{0} #${1:X2}\n", instruction.Mnemonic, operand8Bit); return 2;
				case AddressingMode.ZeroPage: builder.AppendFormat("{0} ${1:X2}\n", instruction.Mnemonic, operand8Bit); return 2;
				case AddressingMode.ZeroPageX: builder.AppendFormat("{0} ${1:X2},x\n", instruction.Mnemonic, operand8Bit); return 2;
				case AddressingMode.ZeroPageY: builder.AppendFormat("{0} ${1:X2},y\n", instruction.Mnemonic, operand8Bit); return 2;
				case AddressingMode.ZeroPageIndexedIndirectX: builder.AppendFormat("{0} (${1:X2},x)\n", instruction.Mnemonic, operand8Bit); return 2;
				case AddressingMode.ZeroPageIndirectIndexedY: builder.AppendFormat("{0} (${1:X2}),y\n", instruction.Mnemonic, operand8Bit); return 2;

				case AddressingMode.Absolute: builder.AppendFormat("{0} ${1:X4}\n", instruction.Mnemonic, operand16Bit); return 3;
				case AddressingMode.AbsoluteX: builder.AppendFormat("{0} ${1:X4},x\n", instruction.Mnemonic, operand16Bit); return 3;
				case AddressingMode.AbsoluteY: builder.AppendFormat("{0} ${1:X4},y\n", instruction.Mnemonic, operand16Bit); return 3;
				case AddressingMode.AbsoluteIndirect: builder.AppendFormat("{0} (${1:X4})\n", instruction.Mnemonic, operand16Bit); return 3;

				default: builder.AppendFormat(".db ${0:X2}", opcode); return 1;
			}
		}
	}

	public class Instruction6502
	{
		public Instruction6502(byte opcode, string mnemonic, AddressingMode mode)
		{
			this.Opcode = opcode;
			this.Mnemonic = mnemonic;
			this.AddressingMode = mode;
		}

		public byte Opcode { get; set; }
		public string Mnemonic { get; set; }
		public AddressingMode AddressingMode { get; set; }
	}
}
