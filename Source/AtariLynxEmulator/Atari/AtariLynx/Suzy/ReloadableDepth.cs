using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public enum ReloadableDepth : byte
	{ 
		None = 0, // "0=none."
		HV = 1, // "1=Hsize,Vsize."
		HVS = 2, // "2=Hsize,Vsize,Stretch."
		HVST = 3 // "3=Hsize, Vsize,Stretch, Tilt."
	}
}
