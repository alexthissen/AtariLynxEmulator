using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx;

namespace AtariLynx.Tests
{
	[TestClass]
	public class MemoryMapControlTest
	{
		MemoryMapControl map = null;
			
		[TestInitialize()]
		public void TestInitialize() 
		{
			map = new MemoryMapControl();
		}

		[TestMethod]
		public void MemoryMapControlShouldDisableSpacesCorrectly()
		{
			map.ByteData = 0x8f;

			Assert.IsTrue(map.RomSpaceDisabled, "Rom space should be disabled");
			Assert.IsTrue(map.VectorSpaceDisabled, "Vector space should be disabled");
			Assert.IsTrue(map.SuzySpaceDisabled, "Suzy space should be disabled");
			Assert.IsTrue(map.MikeySpaceDisabled, "Mikey space should be disabled");
			Assert.IsTrue(map.SequentialDisable, "Sequential access should be disabled");
		}

		[TestMethod]
		public void MemoryMapControlShouldEnableSpacesCorrectly()
		{
			map.ByteData = 0x00;
			Assert.IsFalse(map.RomSpaceDisabled, "Rom space should be enabled");
			Assert.IsFalse(map.VectorSpaceDisabled, "Vector space should be enabled");
			Assert.IsFalse(map.SuzySpaceDisabled, "Suzy space should be enabled");
			Assert.IsFalse(map.MikeySpaceDisabled, "Mikey space should be enabled");
			Assert.IsFalse(map.SequentialDisable, "Sequential access should be enabled");
		}
	}
}
