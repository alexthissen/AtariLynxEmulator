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
		Uart4 uart;
		SerialControlRegister register;

		[TestInitialize()]
		public void TestInitialize()
		{
			register = new SerialControlRegister();
			uart = new Uart4(register);
			uart.Initialize();
		}

		[TestMethod]
		public void ResetShouldSetSerialControlRegister()
		{
			uart.Reset();

			// Assert
			Assert.AreEqual<byte>(SerialControlRegister.TXEMPTYMask | SerialControlRegister.TXRDYMask, 
				uart.SERCTL, "SERCTL should have been reset to 0xA0");
		}

		[TestMethod]
		public void SettingResetAllErrorsShouldClearThreeErrorFlags()
		{
			// Arrange
			
			// Set individual errors straight to serial control register
			uart.SerialControlRegister.FrameError = true;
			uart.SerialControlRegister.ParityError = true;
			uart.SerialControlRegister.OverrunError = true;
			
			// Act
			uart.SERCTL = SerialControlRegister.RESETERRMask;

			// Assert
			Assert.IsFalse(uart.SerialControlRegister.FrameError, "Framing error should have been cleared.");
			Assert.IsFalse(uart.SerialControlRegister.ParityError, "Parity error should have been cleared.");
			Assert.IsFalse(uart.SerialControlRegister.OverrunError, "Overrun error should have been cleared.");
		}

		[TestMethod]
		public void ParityDisabledShouldReturnTrueParityEvenBit()
		{
			// Arrange
			// Disable parity calculation and set MARK for parity bit
			uart.SERCTL = SerialControlRegister.PAREVENMask | SerialControlRegister.TXOPENMask;

			// Act
			bool parityBit = Uart4.ComputeParityBit(0x42, uart.SerialControlRegister);

			// Assert
			Assert.IsTrue(parityBit, "Parity bit should be MARK (true).");
		}

		[TestMethod]
		public void ParityDisabledShouldReturnFalseParityEvenBit()
		{
			// Arrange
			// Disable parity calculation and set SPACE for parity bit (by not enabled PAREVEN bit)
			uart.SERCTL = SerialControlRegister.TXOPENMask;

			// Act
			bool parityBit = Uart4.ComputeParityBit(0x42, uart.SerialControlRegister);

			// Assert
			Assert.IsFalse(parityBit, "Parity bit should be MARK (true).");
		}

		[TestMethod]
		public void OddParityEnabledShouldReturnCorrectParityBit()
		{
			// Arrange
			// Enable parity calculation and set ODD for parity bit (by not enabling PAREVEN bit)
			uart.SERCTL = SerialControlRegister.PARENMask | SerialControlRegister.TXOPENMask;

			// Act
			bool parityBit = Uart4.ComputeParityBit(0x42, uart.SerialControlRegister);

			// Assert
			Assert.IsTrue(parityBit, "Parity bit should be set.");
		}

		[TestMethod]
		public void EvenParityEnabledShouldReturnCorrectParityBit()
		{
			// Arrange
			// Enable parity calculation and set EVEN for parity bit (by enabling PAREVEN bit)
			uart.SERCTL = SerialControlRegister.PARENMask | SerialControlRegister.PAREVENMask | 
				SerialControlRegister.TXOPENMask;

			// Act
			bool parityBit = Uart4.ComputeParityBit(0x42, uart.SerialControlRegister);

			// Assert
			Assert.IsFalse(parityBit, "Parity bit should not be set.");
		}

		[TestMethod]
		public void BaudPulseOnTransmitterShouldStartReceiver()
		{
			// Arrange
			byte data = 0x42;
			uart.Transmitter.TransferToBuffer(data);

			// Act
			uart.Transmitter.HandleBaudPulse(null, EventArgs.Empty);

			// Assert
			Assert.IsTrue(uart.Receiver.IsReceiving, "Transmitter should be done after eleven pulses.");
		}

		[TestMethod]
		public void ElevenBaudPulsesShouldReceiveData()
		{
			// Arrange
			byte data = 0x42;
			uart.Transmitter.TransferToBuffer(data);
			uart.Receiver.DataReceived +=
				(sender, args) =>
				{
					// Assert
					Assert.AreEqual<byte>(data, args.Data, "Received data should be same as transmitted data.");
				};

			// Act
			for (int count = 0; count < 11; count++)
			{
				uart.Transmitter.HandleBaudPulse(null, EventArgs.Empty);
			} 
		}
	}
}
