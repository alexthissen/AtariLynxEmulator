using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.AtariLynx
{
	public struct BllRomImageHeader
	{
		public ushort Jump;
		public ushort LoadAddress;
		public ushort Size;
	}
}
