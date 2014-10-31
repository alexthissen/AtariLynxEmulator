using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Atari.Lynx.Debugger
{
	public class DownloadScreenRequest : DebugRequest
	{
		public DownloadScreenRequest() : base(DebugCommand.DownloadScreen) { }

		public override int ResponseLength
		{
			get
			{
				return 32 + 160/2 * 102;
			}
		}
	}
}
