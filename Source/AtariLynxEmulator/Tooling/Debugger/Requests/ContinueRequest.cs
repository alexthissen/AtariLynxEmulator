using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Atari.Lynx.Debugger
{
	public class ContinueRequest : DebugRequest
	{
		public ContinueRequest() : base(DebugCommand.Continue) { }
	}
}
