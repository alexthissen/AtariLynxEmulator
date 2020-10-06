using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Eeproms
{
	public class InstructionDoneEventArgs: EventArgs
	{
		public Instruction ExecutedInstruction;

		public InstructionDoneEventArgs(Instruction instruction)
		{
			this.ExecutedInstruction = instruction;
		}
	}
}
