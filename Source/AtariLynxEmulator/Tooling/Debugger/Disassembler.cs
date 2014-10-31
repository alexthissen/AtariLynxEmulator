using KillerApps.Emulation.Core;
using KillerApps.Emulation.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtariLynx.Tooling.Debugger
{
	public class Disassembler
	{
		public Instruction6502 Disassemble(IMemoryAccess<ushort, byte> memory, ushort address)
		{
			byte opcode = memory.Peek(address++);
			Instruction6502 instruction = Disassembler6502.InstructionSet[opcode];

			return null;
		}
	}

	public class Instruction6502WithOperand<TOperand> where TOperand : class
	{
		public Instruction6502WithOperand(byte opcode, ushort address, TOperand operand)
		{
			Opcode = opcode;
			MemoryLocation = address;
			Operand = operand;
		}
		public byte Opcode { get; private set; }
		public Instruction6502 Instruction { get { return Disassembler6502.InstructionSet[Opcode]; } }
		public ushort MemoryLocation { get; set; }
		public TOperand Operand { get; set; }
		public override string ToString()
		{
			return String.Format("{0:X4}\t{1}", MemoryLocation, Instruction.Mnemonic);
		}

		private string OperandToHexBytes()
		{
			string operandAsHexBytes = String.Empty;
			byte[] bytes = null; //BitConverter.GetBytes(Operand);
			foreach (byte b in bytes)
			{
				operandAsHexBytes += String.Format("{0:X2} ", b);
			}
			return operandAsHexBytes;
		}
	}
}
