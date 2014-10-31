using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Atari.Lynx.Debugger
{
	public interface IDebugResponse
	{
		int RemainingBytes { get; }
		bool IsComplete { get; }
		void AddBytes(IEnumerable<byte> bytes);
	}
}
