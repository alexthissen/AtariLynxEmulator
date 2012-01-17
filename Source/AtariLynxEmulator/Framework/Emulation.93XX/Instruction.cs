using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Eeproms
{
	public class Instruction
	{
		public Instruction(Opcode opcode, ushort address)
		{
			this.Opcode = opcode;
			this.Address = address;
		}

		public Opcode Opcode { get; private set; }
		public ushort Address { get; private set; }
	}
}
