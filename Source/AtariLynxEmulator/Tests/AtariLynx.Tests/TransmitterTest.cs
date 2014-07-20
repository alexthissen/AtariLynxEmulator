using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx;

namespace AtariLynx.Tests
{
	/// <summary>
	/// Summary description for TransmitterTest
	/// </summary>
	[TestClass]
	public class TransmitterTest
	{
		private Transmitter2 transmitter;
		private SerialControlRegister2 register;
 
		[TestInitialize()]
		public void TestInitialize() 
		{
			register = new SerialControlRegister2();
			transmitter = new Transmitter2(register);
		}

		[TestMethod]
		public void TransferringDataToBufferShouldSetRegisterFlags()
		{
			byte data = 0x42;
			transmitter.TransferToBuffer(data);

			Assert.IsFalse(register.TransmitterBufferEmpty);
			Assert.IsFalse(register.TransmitterTotallyDone);
			Assert.AreEqual<byte>(data, transmitter.Buffer);
		}

		[TestMethod]
		public void TransmitterShouldBeEmptyAndIdleAtStart()
		{
			// Assert
			Assert.IsTrue(register.TransmitterBufferEmpty, "Transmit buffer should be empty at start.");
			Assert.IsTrue(register.TransmitterTotallyDone, "Transmitter should be idle at start.");
		}

		[TestMethod]
		public void BaudPulseOnEmptyTransmitterShouldDoNothing()
		{
			// Act
			transmitter.HandleBaudPulse(null, EventArgs.Empty);

			// Assert
			Assert.IsTrue(register.TransmitterBufferEmpty, "Transmit buffer should still be empty.");
			Assert.IsTrue(register.TransmitterTotallyDone, "Transmitter should still be done.");
		}

		[TestMethod]
		public void BaudPulseOnFilledBufferShouldStartTransfer()
		{
			byte data = 0x42;
			bool transmitting = false;
			UartDataEventArgs arguments = null;

			transmitter.TransferToBuffer(data);
			transmitter.DataTransmitting +=
				(object sender, UartDataEventArgs args) => 
					{
						arguments = args;
						transmitting = true;
					};

			// Act
			transmitter.HandleBaudPulse(null, EventArgs.Empty);

			// Assert
			Assert.IsTrue(register.TransmitterBufferEmpty, "Transmit buffer should be empty again.");
			Assert.IsTrue(transmitting, "Transmitting should have started.");
			Assert.IsNotNull(arguments, "Transmit arguments must be not null.");
			Assert.AreEqual<byte>(data, arguments.Data, "Data transmitting should be same as data put into buffer.");
			Assert.IsFalse(register.TransmitterTotallyDone, "Transmitter should not be done yet.");
		}

		[TestMethod]
		public void BaudPulseOnFilledBufferShouldGiveArguments()
		{
			// Arrange
			byte data = 0x42;
			UartDataEventArgs arguments = null;

			transmitter.TransferToBuffer(data);
			transmitter.DataTransmitting +=
				(object sender, UartDataEventArgs args) =>
				{
					arguments = args;
				};

			// Act
			transmitter.HandleBaudPulse(null, EventArgs.Empty);

			// Assert
			Assert.IsNotNull(arguments, "Transmit arguments must be not null.");
			Assert.AreEqual<byte>(data, arguments.Data, "Data transmitting should be same as data put into buffer.");
			Assert.IsTrue(arguments.StopBitPresent, "Stop bit should only be absent for breaks.");
		}

		[TestMethod]
		public void TransferToBufferWithFilledBufferShouldOverwriteBuffer()
		{
			// Arrange
			byte data1 = 0x42, data2 = 0x84;
			transmitter.TransferToBuffer(data1);

			// Act
			transmitter.TransferToBuffer(data2);
			
			// Assert
			Assert.AreEqual<byte>(data2, transmitter.Buffer, "Transmit buffer should have been overwritten.");
		}

		[TestMethod]
		public void ElevenBaudPulsesShouldGiveIdleTransmitter()
		{
			// Arrange
			byte data = 0x42;
			transmitter.TransferToBuffer(data);
			
			// Act
			for (int count = 0; count < 11; count++)
			{
				transmitter.HandleBaudPulse(null, EventArgs.Empty);
			}

			// Assert
			Assert.IsTrue(register.TransmitterTotallyDone, "Transmitter should be done after eleven pulses.");
			Assert.IsTrue(register.TransmitterBufferEmpty, "Transmitter buffer should be empty when no new data was transferred.");
		}

		[TestMethod]
		public void TenBaudPulsesShouldGiveIdleTransmitter()
		{
			// Arrange
			byte data = 0x42;
			transmitter.TransferToBuffer(data);
			
			// Act
			for (int count = 0; count < 10; count++)
			{
				transmitter.HandleBaudPulse(null, EventArgs.Empty);
			}

			// Assert
			Assert.IsFalse(register.TransmitterTotallyDone, "Transmitter should not be done after ten pulses.");
			Assert.IsTrue(register.TransmitterBufferEmpty, "Transmitter buffer should be empty when no new data was transferred.");
		}
	}
}
