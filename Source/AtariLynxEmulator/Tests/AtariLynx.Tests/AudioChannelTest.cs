using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx;

namespace AtariLynx.Tests
{
	/// <summary>
	/// Summary description for AudioChannelTest
	/// </summary>
	[TestClass]
	public class AudioChannelTest
	{
		AudioChannel channel;

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
		public void TestInitialize() 
		{
			channel = new AudioChannel();
		}

		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void PokeOtherControlBitsShouldSetTopBitsShiftRegister()
		{
			// Act
			channel.OtherControlBits = 0xF0;

			// Assert
			Assert.AreEqual<ushort>(0x0F00, channel.ShiftRegister, "Upper nybble of OtherControlBits should be bit 11-8 of ShiftRegister");
		}

		[TestMethod]
		public void PokeOtherControlBitsShouldSetDynamicControlBits()
		{
			byte other = 0x0F;

			// Act
			channel.OtherControlBits = other;

			// Assert
			Assert.AreEqual<ushort>(0x0F, channel.DynamicControlBits.ByteData, "Lower nybble of OtherControlBits should be dynamic control");
		}

		[TestMethod]
		public void PeekOtherControlBitsShouldGetTopBitsShiftRegister()
		{
			byte other = 0xF0;
			channel.OtherControlBits = other;
			
			// Act
			byte actual = channel.OtherControlBits;

			// Assert
			Assert.AreEqual<byte>(other, actual);
		}

		[TestMethod]
		public void PokeLowerShiftRegisterShouldSetLowerByteShiftRegister()
		{
			byte lower = 0xFF;

			// Act
			channel.LowerShiftRegister = lower;

			// Assert 
			Assert.AreEqual<ushort>(0x00FF, channel.ShiftRegister, "LowerShiftRegister should set 12-bit ShiftRegister");
		}

		[TestMethod]
		public void PeekLowerShiftRegisterShouldGetLowerByteShiftRegister()
		{
			byte lower = 0xFF;
			channel.LowerShiftRegister = lower;

			// Act
			byte actual = channel.LowerShiftRegister;

			// Assert 
			Assert.AreEqual<byte>(lower, actual, "LowerShiftRegister should get lower byte of 12-bit ShiftRegister");
		}

		[TestMethod]
		public void MyTestMethod()
		{
			
		}
	}
}
