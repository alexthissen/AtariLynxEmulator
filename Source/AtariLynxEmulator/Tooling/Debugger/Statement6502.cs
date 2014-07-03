using KillerApps.Emulation.Core;
using KillerApps.Emulation.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AtariLynx.Tooling.Debugger
{
	public class Operand6502
	{
		public Operand6502(byte msb, byte lsb)
		{
			value = new byte[] { lsb, msb };
		}

		public Operand6502(byte single)
		{
			value = new byte[] { single };
		}

		public Operand6502(ushort word)
		{
			value = BitConverter.GetBytes(word);
		}

		private byte[] value;

		public object Value
		{
			get
			{
				if (value.Length == 2) return BitConverter.ToInt16(value, 0);
				if (value.Length == 1) return value[0];
				return null;
			}
		}

		public override string ToString()
		{
			string valueAsHexBytes = String.Empty;
			foreach (byte b in value)
			{
				valueAsHexBytes += String.Format("{0:X2} ", b);
			}
			return valueAsHexBytes;
		}
	}

	public class Disassembler
	{
		public Expression6502 Disassemble(IMemoryAccess<ushort, byte> memory, ushort address)
		{
			byte opcode = memory.Peek(address++);
			Instruction6502 instruction = Disassembler6502.InstructionSet[opcode];

			// Check for unknown opcodes
			if (instruction == null)
				return Expression6502.DataByte(opcode);

			int operandSize = Expression6502.DetermineOperandSize(instruction.AddressingMode);
			if (address + operandSize - 1 > ushort.MaxValue - 1)
				return Expression6502.DataByte(opcode);

			// Everything checks out, so create expression
			Expression6502 expression = new Expression6502(opcode);
			switch (operandSize)
			{
				case 1:
					expression.Operand = new Operand6502(memory.Peek(address));
					break;
				case 2:
					expression.Operand = new Operand6502(memory.PeekWord(address));
					break;
				default:
					expression.Operand = null;
					break;
			}

			return expression;
		}

		public static Expression6502 FromByteArray(byte[] bytes, int startIndex)
		{
			// Array size must be large enough for start index
			if (startIndex >= bytes.Length)
				throw new ArgumentException("Start index outside of byte range", "startIndex");

			int index = startIndex;
			byte opcode = bytes[index++];
			Instruction6502 instruction = Disassembler6502.InstructionSet[opcode];

			// Check for unknown opcodes
			if (instruction == null)
				return Expression6502.DataByte(opcode);
			
			int operandSize = Expression6502.DetermineOperandSize(instruction.AddressingMode);
			if (startIndex + operandSize > bytes.Length - 1)
				return Expression6502.DataByte(opcode);
			
			// Everything checks out, so create expression
			Expression6502 expression = new Expression6502(instruction);
			switch (operandSize)
			{
				case 1:
					expression.Operand = new Operand6502(bytes[index]);
					break;
				case 2:
					expression.Operand = new Operand6502(bytes[index+1], bytes[index]);
					break;
				default:
					expression.Operand = null;
					break;
			}

			return expression;
		}
	}

	public class Expression6502
	{
		internal static Expression6502 DataByte(byte opcode)
		{
			return new Expression6502(opcode);
		}

		public Expression6502(byte opcode)
		{
			Instruction = Disassembler6502.InstructionSet[opcode];
		}

		public Expression6502(Instruction6502 instruction)
		{
			this.Instruction = instruction;
		}

//		private byte[] bytes;
		public Instruction6502 Instruction { get; private set; }
		public Operand6502 Operand { get; internal set; }

		public int DetermineOperandSize()
		{
			return DetermineOperandSize(this.Instruction.AddressingMode);
		}

		public static int DetermineOperandSize(AddressingMode mode)
		{
			switch (mode)
			{
				case AddressingMode.Accumulator:
				case AddressingMode.Implicit:
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
				case AddressingMode.AbsoluteIndexedIndirectX:
				case AddressingMode.AbsoluteIndirect:
				case AddressingMode.AbsoluteX:
				case AddressingMode.AbsoluteY:
					return 2;

				default:
					return 0;
			}
		}

		public ushort ByteLength { get { return (ushort)(1 + DetermineOperandSize(Instruction.AddressingMode)); } }
		//	0200	A9 00		LDA #$09
	
	}

	public static class DebugExtensions
	{
		public static IEnumerable<Expression6502> Disassemble(this IMemoryAccess<ushort, byte> memory, ushort startAddress)
		{
			return Disassemble(memory, startAddress, ushort.MaxValue);
		}

		public static IEnumerable<Expression6502> Disassemble(this IMemoryAccess<ushort, byte> memory, 
			ushort startAddress, ushort toAddress)
		{
			ushort address = startAddress;
			while (disassembler.Disassemble(memory, address))
			{
				Expression6502 expression = disassembler.Disassemble(memory, address);
				yield return expression;

				// Check whether to continue to next instruction
				if (toAddress - expression.ByteLength <= address)
					yield break;

				address += expression.ByteLength;
			}
		}
	}

	public class Dis
	{
		public void Do()
		{
			StringBuilder builder = new StringBuilder();
			ushort startAddress = 0x1234;
			IMemoryAccess<ushort, byte> memory = null;
			foreach (Expression6502 expression in memory.Disassemble(startAddress).Take(20))
			{
				builder.Append(expression);
			}
		}
	}
}
