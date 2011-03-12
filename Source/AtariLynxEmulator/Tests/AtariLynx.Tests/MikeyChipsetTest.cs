using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx;
using Moq;

namespace AtariLynx.Tests
{
	/// <summary>
	/// Summary description for MikeyChipsetTest
	/// </summary>
	[TestClass]
	public class MikeyChipsetTest
	{
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
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void PeekColorMapShouldReturnPokeValues()
		{
			// Arrange
			Mock<ILynxDevice> device = new Mock<ILynxDevice>();
			MikeyChipset mikey = new MikeyChipset(device.Object);

			// Act
			for (byte index = 0; index < 0x0F; index++)
			{
				mikey.Poke((ushort)(MikeyAddresses.GREEN0 + index), index);
				mikey.Poke((ushort)(MikeyAddresses.BLUERED0 + index), (byte)((index << 4) + index));
			}
			
			// Assert
			for (byte index = 0; index < 0x0F; index++)
			{
				Assert.AreEqual<byte>(index, mikey.Peek((ushort)(MikeyAddresses.GREEN0 + index)));
				Assert.AreEqual<byte>(index, (byte)(mikey.Peek((ushort)(MikeyAddresses.BLUERED0 + index)) >> 4));
				Assert.AreEqual<byte>(index, (byte)(mikey.Peek((ushort)(MikeyAddresses.BLUERED0 + index)) & 0x0F));
			}
		}

		[TestMethod]
		public void PokeColorMapShouldSetColorMapValues()
		{
			// Arrange
			Mock<ILynxDevice> device = new Mock<ILynxDevice>();
			MikeyChipset mikey = new MikeyChipset(device.Object);

			// Act
			for (byte index = 0; index < 0x0F; index++)
			{
				mikey.Poke((ushort)(MikeyAddresses.GREEN0 + index), index);
				mikey.Poke((ushort)(MikeyAddresses.BLUERED0 + index), (byte)((index << 4) + index));
			}

			// Assert
			for (byte index = 0; index < 0x0F; index++)
			{
				Assert.AreEqual<byte>(index, mikey.GreenColorMap[index], "Green map not set correctly.");
				Assert.AreEqual<byte>(index, mikey.BlueColorMap[index], "Blue map not set correctly.");
				Assert.AreEqual<byte>(index, mikey.RedColorMap[index], "Red color map not set correctly.");
			}
		}
	}
}
