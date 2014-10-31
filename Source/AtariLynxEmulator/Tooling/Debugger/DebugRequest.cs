using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Atari.Lynx.Debugger
{
	public class DebugRequest
	{
		//protected byte[] data;
		public List<byte> Queue { get; internal set; }

		public DebugRequest(DebugCommand command)
		{
			// In this case message owns list
			Queue = new List<byte>(new byte[ByteLength]);
			Queue[0] = (byte)command;
		}
		
		public DebugRequest()
		{
			// Message does not own list
		}

		public DebugCommand Command { get { return (DebugCommand)Queue[0]; } }
		public virtual int ByteLength { get { return 1; } }
		public virtual int ResponseLength { get { return 0; } }

		public virtual byte[] ToByteArray()
		{
			return Queue.ToArray();
		}

	}
}
