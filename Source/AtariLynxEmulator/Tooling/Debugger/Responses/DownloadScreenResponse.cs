using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Atari.Lynx.Debugger
{
	public class DownloadScreenResponse : DebugResponse<DownloadScreenRequest>
	{
		private const int PaletteSize = 32;

		public byte[] Memory
		{
			get
			{
				if (!IsComplete) throw new InvalidOperationException("Response is not complete.");
				return queue.Skip(Echo.ByteLength+PaletteSize).Take(Echo.ResponseLength - PaletteSize).ToArray();
			}
		}
		public byte[] Palette
		{
			get
			{
				if (!IsComplete) throw new InvalidOperationException("Response is not complete.");
				return queue.Skip(Echo.ByteLength).Take(PaletteSize).ToArray();
			}
		}
	}
}
