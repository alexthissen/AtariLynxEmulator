using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx;

namespace AtariLynx.Tests
{
	/// <summary>
	/// Summary description for TimerTest
	/// </summary>
	[TestClass]
	public class UartTest
	{
		Uart2 uart;

		[TestInitialize()]
		public void TestInitialize()
		{
			uart = new Uart2();
		}

		[TestMethod]
		public void ResetShouldSetSerialControlRegister()
		{
			uart.Reset();

			// Assert
			Assert.AreEqual<byte>(0x80, uart.SERCTL.ByteData, "SERCTL should have been reset to 0x00");
			Assert.IsTrue(uart.SERCTL.TransmitterBufferEmpty);
			Assert.IsFalse(uart.SERCTL.ReceiveReady);
			Assert.IsFalse(uart.SERCTL.TransmitterDone);
			Assert.IsFalse(uart.SERCTL.ParityError);
			Assert.IsFalse(uart.SERCTL.OverrunError);
			Assert.IsFalse(uart.SERCTL.FrameError);
			Assert.IsFalse(uart.SERCTL.ReceivedBreak);
			Assert.IsFalse(uart.SERCTL.ParityBit);
		}

		[TestMethod]
		public void TransmitSerialDataShouldPrepareTransmit()
		{
			byte dataToTransmit = 0xFF;

			// Act
			uart.TransmitSerialData(dataToTransmit);

			// Assert
			Assert.AreEqual<byte>(dataToTransmit, uart.transmitHoldingRegister, "Transmit holding register should be filled.");
			Assert.IsFalse(uart.SERCTL.TransmitterBufferEmpty);
			Assert.IsFalse(uart.SERCTL.TransmitterDone);
		}
	}
}
