using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx;

namespace AtariLynx.Tests
{
	/// <summary>
	/// Summary description for MemoryMapControl
	/// </summary>
	[TestClass]
	public class MemoryMapControlTest
	{
		MemoryMapControl map = null;
			
		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		[TestInitialize()]
		public void MyTestInitialize() 
		{
			map = new MemoryMapControl();
		}

		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

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
