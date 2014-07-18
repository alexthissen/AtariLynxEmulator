using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx;

namespace AtariLynx.Tests
{
	[TestClass]
	public class ReceiverTest
	{
		private Receiver receiver;
		private SerialControlRegister2 register;

		[TestInitialize]
		public void TestInitialize()
		{
			register = new SerialControlRegister2();
			receiver = new Receiver(register);
		}

		[TestMethod]
		public void ReceiverShouldStartWithEmptyBuffer()
		{
			// Assert
			Assert.IsFalse(register.ReceiveReady, "Receive buffer should not be ready.");
			Assert.IsFalse(register.FrameError, "At startup frame error should not be set.");
			Assert.IsFalse(register.ParityError, "At startup parity error should not be set.");
			Assert.IsFalse(register.OverrunError, "At startup receiver should not be overrun.");
		}

		[TestMethod]
		public void ReceiverShouldStartReceivingAfterAcceptingData()
		{
			// Arrange
			byte data = 0x54;
			
			// Act
			receiver.AcceptData(data);

			// Assert
			Assert.IsTrue(receiver.IsReceiving, "Receiver should be receiving after accepting data.");
			Assert.IsFalse(register.ReceiveReady, "Received data should not be ready yet.");
		}

		[TestMethod]
		public void ReceivingReceiverShouldGiveFramingErrorOnAccept()
		{
			// Arrange
			byte data = 0x43;
			receiver.AcceptData(data);
			
			// Act
			receiver.AcceptData(data);

			// Assert
			Assert.IsTrue(register.FrameError, "Accepting data during receiving should give framing error.");
		}

		[TestMethod]
		public void ElevenBaudPulsesShouldGiveReadyData()
		{
			// Arrange
			byte data = 0x43;
			receiver.AcceptData(data);

			// Act
			for (int count = 0; count < 11; count++)
			{
				receiver.HandleBaudPulse(null, EventArgs.Empty);
			}

			// Assert
			Assert.AreEqual<byte>(data, receiver.SerialData, "Ready data should be same as accepted data.");
			Assert.IsFalse(receiver.IsReceiving, "Receiver should not be receiving anymore.");
			Assert.IsTrue(register.ReceiveReady, "Received data should be ready.");
		}
	}
}
