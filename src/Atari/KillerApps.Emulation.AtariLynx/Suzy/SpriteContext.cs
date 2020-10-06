using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Processors;

namespace KillerApps.Emulation.AtariLynx
{
	public class SpriteContext
	{
		public Word HOFF, VOFF;
		public Word VIDBAS, COLLBAS;
		public Word COLLOFF;
		public Word HSIZOFF, VSIZOFF;

		public bool VStretch;
		public bool DontCollide;
		public bool EveronEnabled;
	}
}
