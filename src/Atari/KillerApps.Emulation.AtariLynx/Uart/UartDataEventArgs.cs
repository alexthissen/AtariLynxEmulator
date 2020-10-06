using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.AtariLynx
{
	public class UartDataEventArgs : EventArgs
	{
		public UartDataEventArgs()
		{
			Break = false;
			Data = 0x00;
			ParityBit = false;
			StopBitPresent = true;
		}

		public bool Break { get; set; }
		public byte Data { get; set; }
		public bool ParityBit { get; set; }
		public bool StopBitPresent { get; set; }

	}
}
