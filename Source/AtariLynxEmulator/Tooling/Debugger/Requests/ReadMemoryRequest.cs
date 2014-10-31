using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Atari.Lynx.Debugger
{
	public class ReadMemoryRequest : DebugRequest
	{
		public ReadMemoryRequest() { }

		public ReadMemoryRequest(ushort address, byte length)
			: base(DebugCommand.ReadMemory)
		{
			this.Address = address;
			this.Length = length;
		}

		public ushort Address 
		{
			get { return (ushort)((Queue[1] << 8) + Queue[2]); }
			set
			{
				Queue[1] = (byte)(value >> 8);
				Queue[2] = (byte)(value & 0xff);
			}
		}

		public byte Length 
		{
			get { return Queue[3]; }
			set { Queue[3] = value; }
		}

		public override int ResponseLength { get { return Length == 0 ? 256 : Length; } }

		public override int ByteLength { get { return 4; } } // command (1) + address (2) + length (1)
	}
}
