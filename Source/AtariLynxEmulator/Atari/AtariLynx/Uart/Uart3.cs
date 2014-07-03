using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	// TODO: Use and place comments at right spot
	
	// "The interrupt bit for timer 4 (UART baud rate) is driven by receiver or transmitter ready bit of the UART."

	// "The UART interrupt is not edge sensitive."

	public class Uart3: IResetable
	{
		// Comlynx related variables
		public SerialControlRegister SERCTL { get; private set; }
		private byte serialData;
		public byte SERDAT
		{
			get
			{
				// When serial data register is read, it 
				SERCTL.ReceiveReady = false;
				return serialData;
			}
			set 
			{ 
				serialData = value;
			} 
		}

		private Transmitter Transmitter = new Transmitter();

		private byte receiveHoldingRegister;
		private int receivePulseCountdown;

		private const byte RECEIVE_PERIODS = 11;

		private bool receiveInactive = true;

		public IComLynxTransport ComLynxTransport { get; set; }

		public Uart3()
		{
			Initialize();
		}

		public void Initialize()
		{
			// "The UART TXD signal powers up in TTL HIGH, instead of open collector."
			// meaning that TXOPEN is set to 0 (and consequently SERCTL.TransmitOpen to false)
			SERCTL = new SerialControlRegister(0x00); // "reset 0,0,0,0,0,0,0,0"
		}

		public void SetSerialControlRegister(byte value)
		{
			SERCTL.ByteData = value;

			// "Once received, these error bits remain set until they are cleared by writing to the control 
			// byte with the reset error bit set."
			if (SERCTL.ResetAllErrors)
			{
				SERCTL.ParityError = false;
				SERCTL.OverrunError = false;
				SERCTL.FrameError = false;
			}
		}

		public void OnBaudPulse()
		{
			// "A break of any length can be transmitted by setting the transmit break bit in the 
			// control register. The break will continue as long as the bit is set.
			if (SERCTL.TransmitBreak)
			{
				Transmitter.TransmitBreak();
				return;
			}
		}

		public byte ReadFromReceiveBuffer()
		{
			// "In addition, the parity of the received character is calculated and if it does not match 
			// the setting of the parity select bit in the control byte, the parity error bit will be set. 
			// Receive parity error can not be disabled. If you don't want it, don't read it."
			if (HasParityError()) SERCTL.ParityError = true;

			SERDAT = receiveHoldingRegister;
			SERCTL.ReceiveReady = true;

			return 0x00;
		}

		private bool HasParityError()
		{
			// For now we never have any parity errors
			return false;
		}

		public bool GenerateBaudRate()
		{
			bool fireInterrupt = false;
			
			// Check if frame has been received
			if (receivePulseCountdown == 0)
			{
				// When data was already present in SERDAT an overrun has occurred
				if (SERCTL.ReceiveReady) SERCTL.OverrunError = true;

				// Transfer data to SERDAT
				SERDAT = receiveHoldingRegister;
				SERCTL.ReceiveReady = true;
			}
			else 
			{
				receivePulseCountdown--;
			}

			//if (receiveCountdown == 0 && !receiveInactive)
			//{
			//  if (receiveBuffer.Count > 0)
			//    SERDAT = receiveBuffer.Dequeue();

			//  if (receiveBuffer.Count > 0)
			//  {
			//    receiveCountdown = 11 + 44;
			//    receiveInactive = false;
			//  }
			//  else
			//  {
			//    receiveInactive = true;
			//  }

			//  if (SERCTL.ReceiveReady)
			//    SERCTL.OverrunError = true;

			//  SERCTL.ReceiveReady = true;
			//}
			//else if (!receiveInactive)
			//{
			//  receiveCountdown--;
			//}

			//if (transmitCountdown == 0 && !SERCTL.TransmitterDone)
			//{
			//  if (SERCTL.TransmitBreak)
			//  {
			//    // TODO: Implement break transmission
			//  }
			//  else
			//    SERCTL.TransmitterDone = true;
			//}
			//else if (!SERCTL.TransmitterDone)
			//  transmitCountdown--;

			//// "Well, we did screw something up after all. 
			//// Both the transmit and receive interrupts are 'level' sensitive, rather than 'edge' sensitive. 
			//// This means that an interrupt will be continuously generated as long as it is enabled and 
			//// its UART buffer is ready. 
			//// As a result, the software must disable the interrupt prior to clearing it. Sorry."

			// "The interrupt bit for timer 4 (UART baud rate) is driven by receiver or transmitter ready bit of the UART."


			//// Emulate the UART bug where UART IRQ is level sensitive
			//if ((SERCTL.TransmitterDone && SERCTL.TransmitterInterruptEnable) || (SERCTL.ReceiveReady && SERCTL.ReceiveInterruptEnable))
			//{
			//  fireInterrupt = true;
			//}

			return fireInterrupt;
		}

		public void Reset()
		{
			Initialize();

			// "This bit is also set to '1' after a reset."
			// where 'this bit' is the TXRDY bit
			SERCTL.TransmitterDone = true;
		}

		public void WriteToTransmitBuffer(byte data)
		{
			transmitHoldingRegister = data;
			SERCTL.TransmitterBufferEmpty = false;
			// There is data to be sent
			SERCTL.TransmitterDone = false;
		}

		public void TransferHoldingToShiftRegister()
		{
			// "If TXRDY is a '1', then the contents of the transmit holding register have been loaded 
			// into the transmit shift register and the holding register is now available to be 
			// loaded with the next byte to be transmitted."
			transmitShiftRegister = transmitHoldingRegister;

			// Just emptied holding register
			SERCTL.TransmitterBufferEmpty = true;
		}
	}
}