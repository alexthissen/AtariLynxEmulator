using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Atari.Lynx.Debugger
{
	public class WriteMemoryRequest : DebugRequest
	{
		public WriteMemoryRequest() 
		{
				
		}

		public WriteMemoryRequest(ushort address, byte length, byte[] memory)
		{
			int size = length == 0 ? 256 : length;
			if (memory.Length != size)
				throw new ArgumentException("Invalid memory size. Valid range is 1 to 256.", "memory");
			Queue = new List<byte>(new byte[4]);
			Queue[0] = (byte)DebugCommand.WriteMemory;
			this.Address = address;
			this.Length = length;
			Queue.AddRange(memory.Take(size));
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

		public byte[] Memory 
		{
			get { return Queue.Skip(4).Take(Length).ToArray(); }
		}

		public override int ByteLength 
		{ 
			get 
			{
				if (Queue.Count < 4) return 4;
				return 4 + (Length == 0 ? 256 : Length);
			}
		}

		public override byte[] ToByteArray()
		{
			return base.ToByteArray();
		}
	}
}
