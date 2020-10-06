using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.AtariLynx
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
}
