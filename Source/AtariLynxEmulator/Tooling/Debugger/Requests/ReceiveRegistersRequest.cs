using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Atari.Lynx.Debugger
{
	public class ReceiveRegistersRequest : DebugRequest
	{
		public ReceiveRegistersRequest() { }
		public ReceiveRegistersRequest(byte A, byte X, byte Y, byte PS, ushort PC, byte SP) 
			: base(DebugCommand.ReceiveRegisters)
		{
			this.A = A;
			this.X = X;
			this.Y = Y;
			this.PS = PS;
			this.PC = PC;
			this.SP = SP;
		}
		public byte SP
		{
			get { return Queue[1]; }
			set { Queue[1] = value; }
		}
		public ushort PC
		{
			get { return (ushort)((Queue[2] << 8) + Queue[3]); }
			set
			{
				Queue[2] = (byte)(value >> 8);
				Queue[3] = (byte)(value & 0xff);
			}
		}
		public byte PS
		{
			get { return Queue[4]; }
			set { Queue[4] = value; }
		}

		public byte Y
		{
			get { return Queue[5]; }
			set { Queue[5] = value; }
		}
		public byte X
		{
			get { return Queue[6]; }
			set { Queue[6] = value; }
		}
		public byte A
		{
			get { return Queue[7]; }
			set { Queue[7] = value; }
		}

		public override int ByteLength { get { return 8; } } 
	}
}
