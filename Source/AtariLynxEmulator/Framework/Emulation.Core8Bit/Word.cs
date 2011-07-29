using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace KillerApps.Emulation.Core
{
	[DebuggerDisplay("Word ({Value})")]
	public struct Word
	{
		public ushort Value
		{
			get 
			{
				return (ushort)((HighByte << 8) + LowByte); 
			}
			set
			{
				LowByte = (byte)(value & 0x00FF);
				HighByte = (byte)((value & 0xFF00) >> 8);
			}
		}

		public byte LowByte;
		public byte HighByte;
	}
}
