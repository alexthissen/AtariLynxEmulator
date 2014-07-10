using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Processors
{
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
