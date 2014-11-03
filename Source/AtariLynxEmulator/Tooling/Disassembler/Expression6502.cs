using KillerApps.Emulation.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Processors
{
	public struct Registers
	{
		public byte A, X, Y, PS;
		public ushort PC;
		public byte SP;
	}

	public class Expression6502
	{
		private IMemoryAccess<ushort, byte> memory;
		private ushort address;

		public Expression6502(IMemoryAccess<ushort, byte> memory, ushort address)
		{
			this.memory = memory;
			this.address = address;
			switch (DetermineOperandSize(Instruction.AddressingMode))
			{
				case 1:
					Operand = new Operand6502(memory.Peek(++address));
					break;
				case 2:
					Operand = new Operand6502(memory.PeekWord(++address));
					break;
				default:
					Operand = null;
					break;
			}
		}

		public ushort Address { get { return address; } }
		public Instruction6502 Instruction
		{
			get
			{
				byte opcode = memory.Peek(address);
				return Instruction6502.InstructionSet[opcode] ?? Instruction6502.Data(opcode);
			}
		}
		public Operand6502 Operand { get; internal set; }

		public int DetermineOperandSize()
		{
			return DetermineOperandSize(this.Instruction.AddressingMode);
		}

		public static int DetermineOperandSize(AddressingMode mode)
		{
			switch (mode)
			{
				case AddressingMode.Implicit:
				case AddressingMode.Accumulator:
					return 0;

				case AddressingMode.Immediate:
				case AddressingMode.Relative:
				case AddressingMode.ZeroPage:
				case AddressingMode.ZeroPageIndexedIndirectX:
				case AddressingMode.ZeroPageIndirect:
				case AddressingMode.ZeroPageIndirectIndexedY:
				case AddressingMode.ZeroPageX:
				case AddressingMode.ZeroPageY:
					return 1;

				case AddressingMode.Absolute:
				case AddressingMode.AbsoluteX:
				case AddressingMode.AbsoluteY:
				case AddressingMode.AbsoluteIndexedIndirectX:
				case AddressingMode.AbsoluteIndirect:
					return 2;

				default:
					return 0;
			}
		}

		public ushort ByteLength { get { return (ushort)(1 + DetermineOperandSize(Instruction.AddressingMode)); } }
		//	0200	A9 00		LDA #$09

		public override string ToString()
		{
			return ToString(DisassemblyOutputOptions.IncludeAddress | DisassemblyOutputOptions.IncludeSourceBytes);
		}

		public string ToString(DisassemblyOutputOptions options)
		{
			StringBuilder builder = new StringBuilder();

			// Create various segments of disassembled expression
			if (HasOutputFlag(options, DisassemblyOutputOptions.IncludeAddress))
				BuildAddress(builder);
			if (HasOutputFlag(options, DisassemblyOutputOptions.IncludeSourceBytes))
				BuildSourceBytes(builder);
			BuildMnemonic(builder, HasOutputFlag(options, DisassemblyOutputOptions.MnemonicInLowerCase));
			BuildOperand(builder);

			return builder.ToString();
		}

		private bool HasOutputFlag(DisassemblyOutputOptions value, DisassemblyOutputOptions option)
		{
			return (value & option) == option;
		}

		private void BuildSourceBytes(StringBuilder builder)
		{
			string sourceBytes = String.Empty;
			for (ushort index = address; index < address + ByteLength; index++)
			{
				sourceBytes += String.Format("{0:X2} ", memory.Peek(index));
			}
			builder.AppendFormat("{0,-10}", sourceBytes);
		}

		private void BuildMnemonic(StringBuilder builder, bool useLowerCase)
		{
			string mnemonic = Instruction.Mnemonic.ToUpper();
			builder.Append(useLowerCase ? mnemonic.ToLower() : mnemonic);
		}

		private string[] AddressingFormats = new string[] {
			"${0:X2}",
			String.Empty,
			"A",
			"#${0:X2}",
			"${0:X4}",
			"${0:X2}",
			"${0:X2},X",
			"${0:X2},Y",
			"(${0:X2})",
			"(${0:X2},X)",
			"(${0:X2}),Y",
			"${0:X4}",
			"${0:X4},X",
			"${0:X4},Y",
			"(${0:X4})",
			"(${0:X4},X)"
		};

		private void BuildOperand(StringBuilder builder)
		{
			if (Operand == null) return;

			string format = AddressingFormats[(int)Instruction.AddressingMode];
			object value = null;

			switch (Instruction.AddressingMode)
			{
				case AddressingMode.Illegal:
					value = Instruction.Opcode;
					break;
				case AddressingMode.Relative:
					ushort jump = (ushort)(address + 2 + (byte)Operand.Value);
					if ((byte)Operand.Value > 0x80) jump -= 256;
					value = jump;
					break;
				default:
					value = Operand.Value; //format = AddressingFormats[(int)Instruction.AddressingMode];
					break;
			}
			builder.Append(" ");
			builder.AppendFormat(format, value);
		}

		private void BuildAddress(StringBuilder builder)
		{
			builder.AppendFormat("{0,-6:X4}", address);
		}
	}
}
