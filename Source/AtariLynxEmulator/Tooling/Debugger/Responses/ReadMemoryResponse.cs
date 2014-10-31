using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Atari.Lynx.Debugger
{
	public class ReadMemoryResponse : DebugResponse<ReadMemoryRequest>
	{
		public byte[] Memory
		{
			get
			{
				if (!IsComplete) throw new InvalidOperationException("Response is not complete.");
				return queue.Skip(Echo.ByteLength).Take(Echo.ResponseLength).ToArray();
			}
		}
	}
}
