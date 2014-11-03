using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Atari.Lynx.Debugger
{
	public class SendRegistersResponse : DebugResponse<SendRegistersRequest>
	{
		public byte SP { get { return queue[1]; } }
		public byte PCHi { get { return queue[2]; } }
		public byte PCLo { get { return queue[3]; } }
		public ushort PC { get { return (ushort)((queue[2] << 8) + queue[3]); } }
		public byte PS { get { return queue[4]; } }
		public byte Y { get { return queue[5]; } }
		public byte X { get { return queue[6]; } }
		public byte A { get { return queue[7]; } }

		public Registers ToRegisters()
		{
			Registers registers;
			registers.A = A;
			registers.X = X;
			registers.Y = Y;
			registers.PC = PC;
			registers.SP = SP;
			registers.PS = PS;
			return registers;
		}
	}
}
