using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx;

namespace AtariLynx.Tests
{
	[TestClass]
	public class SerialControlRegisterTest
	{
		private SerialControlRegister register;

		[TestInitialize()]
		public void TestInitialize() 
		{
			register = new SerialControlRegister();
		}

		[TestMethod]
		public void SettingAllBitsShouldReturnCorrectByteValue()
		{
			// Act
			register.TransmitterBufferEmpty = true;
			register.TransmitterTotallyDone = true;
			register.ReceiveReady = true;
			register.ParityError = true;
			register.OverrunError = true;
			register.FrameError = true;
			register.ReceivedBreak = true;
			register.ParityBit = true;

			// Assert
			Assert.AreEqual<byte>(0xff, register.ByteData, "Byte data is not set correctly by individual bits.");
		}

		[TestMethod]
		public void SettingNoBitsShouldReturnCorrectByteValue()
		{
			// Act
			register.TransmitterBufferEmpty = false;
			register.TransmitterTotallyDone = false;
			register.ReceiveReady = false;
			register.ParityError = false;
			register.OverrunError = false;
			register.FrameError = false;
			register.ReceivedBreak = false;
			register.ParityBit = false;

			// Assert 
			Assert.AreEqual<byte>(0x00, register.ByteData, "Byte data is not set correctly by individual bits.");
		}

		[TestMethod]
		public void SettingFullByteValueShouldSetCorrectBits()
		{
			// Act
			register.ByteData = 0xff;

			// Assert
			Assert.IsTrue(register.TransmitterInterruptEnable);
			Assert.IsTrue(register.ReceiveInterruptEnable);
			Assert.IsTrue(register.TransmitParityEnable);
			Assert.IsTrue(register.ResetAllErrors);
			Assert.IsTrue(register.TransmitOpenCollector);
			Assert.IsTrue(register.TransmitBreak);
			Assert.IsTrue(register.ParityEven);
		}

		[TestMethod]
		public void SettingZeroByteValueShouldSetCorrectBits()
		{
			// Act
			register.ByteData = 0x00;

			// Assert
			Assert.IsFalse(register.TransmitterInterruptEnable);
			Assert.IsFalse(register.ReceiveInterruptEnable);
			Assert.IsFalse(register.TransmitParityEnable);
			Assert.IsFalse(register.ResetAllErrors);
			Assert.IsFalse(register.TransmitOpenCollector);
			Assert.IsFalse(register.TransmitBreak);
			Assert.IsFalse(register.ParityEven);
		}
	}
}
