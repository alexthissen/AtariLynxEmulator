using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.AtariLynx
{
	// "FC90 = SUZYBUSEN. Suzy Bus Enable (W)"
	public class SuzyBusEnable
	{
		public byte ByteData { get; set; }

		// "B0 = Suzy Bus Enable, 0=disabled"
		public bool BusEnabled
		{
			get { return (ByteData & BUSENABLEMask) == BUSENABLEMask; }
			set { ByteData = (byte)(value ? ByteData | BUSENABLEMask : ByteData & (BUSENABLEMask ^ 0xFF)); }
		}

		private const byte BUSENABLEMask = 0x01;
	}
}
