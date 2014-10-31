using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Atari.Lynx.Debugger
{
	public class DebugResponse<T> : IDebugResponse where T : DebugRequest, new()
	{
		protected List<byte> queue = new List<byte>();
		protected T Echo;

		public DebugResponse()
		{
			Echo = new T();
			Echo.Queue = queue;
		}

		public virtual int RemainingBytes
		{
			get
			{
				if (queue.Count < Echo.ByteLength) return Echo.ByteLength - queue.Count;
				if (!IsComplete) return Echo.ByteLength + Echo.ResponseLength - queue.Count;
				return 0;
			}
		}

		public virtual bool IsComplete
		{
			get
			{
				if (queue.Count < Echo.ByteLength) return false;
				return queue.Count == Echo.ByteLength + Echo.ResponseLength;
			}
		}

		public void AddBytes(IEnumerable<byte> bytes)
		{
			queue.AddRange(bytes);
		}
	}
}
