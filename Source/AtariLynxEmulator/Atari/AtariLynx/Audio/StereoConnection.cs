using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class StereoConnection
	{
		public Ear LeftEar { get; private set; }
		public Ear RightEar { get; private set; }

		public StereoConnection()
		{
			LeftEar = new Ear();
			RightEar = new Ear();
		}
	}

	public class Ear
	{
		public bool[] AudioChannelDisabled = new bool[4];
	}
}
