using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx;

namespace AtariLynx.Tests
{
	[TestClass]
	public class SpriteSystemControlTest
	{
		[TestMethod]
		public void SpriteSystemControl1()
		{
			SpriteSystemControl SPRSYS = new SpriteSystemControl();

			SPRSYS.MathWarning = true;
			// read SPRSYS.SignedMath
			// read SPRSYS.Accumulate
		}
	}
}
