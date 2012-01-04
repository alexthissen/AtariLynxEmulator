using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	// TODO: Use and place comments at right spot
	
	// "The interrupt bit for timer 4 (UART baud rate) is driven by receiver or transmitter ready bit of the UART."

	// "The UART interrupt is not edge sensitive."

	public class Uart2: IResetable
	{
		// Comlynx related variables
		public SerialControlRegister SERCTL { get; private set; }
		public byte SERDAT { get; set; }
		public event EventHandler<UartDataEventArgs> DataTransmitted;

		private byte transmitHoldingRegister;
		private byte transmitShiftRegister;
		private byte receiveHoldingRegister;
		private int transmitPulseCountdown;
		private int receivePulseCountdown;

		private const byte TRANSMIT_PERIODS = 11;
		private const byte RECEIVE_PERIODS = 11;

		private bool receiveInactive = true;

		public IComLynxTransport ComLynxTransport { get; set; }

		public Uart2()
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
				TransmitBreak();
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

		public void TransmitBreak()
		{
			// "A 'break' is defined as a start bit, a data value of 0 (same as a permanent start bit), 
			// and the absence of a stop bit at the expected time."
			UartDataEventArgs args = new UartDataEventArgs() { Break = true, Data = 0x00, StopBitPresent = false };
			OnTransmit(args);
		}

		public void TransmitData(byte data)
		{
			// Complete sending current shift register contents
			UartDataEventArgs args = new UartDataEventArgs() { Break = false, Data = data };
			OnTransmit(args);

			// "If TXEMPTY is a '1', then BOTH the transmit holding register and the transmit shift register 
			// have been emptied and there are no more bits going out the serial data line."
			if (SERCTL.TransmitterBufferEmpty) SERCTL.TransmitterDone = true;
		}

		protected virtual void OnTransmit(UartDataEventArgs args)
		{
			args.ParityBit = CalculateParityBit(args.Data);
			if (DataTransmitted != null) DataTransmitted(this, args);
		}

		private bool CalculateParityBit(byte data)
		{
			// "The 9th bit is always sent. It is either the result of a parity calculation on the transmit 
			// data byte or it is the value set in the parity select bit in the control register.
			// The choice is made by the parity enable bit in the control byte. For example :
			// If PAREN is '0', then the 9th bit will be whatever the state of PAREVEN is."
			if (!SERCTL.ParityBit) return SERCTL.ParityEven;

			// If PAREN is '1' and PAREVEN is '0', then the 9th bit will be the result of an 'odd' parity calculation 
			// on the transmit data byte.
			return CalculateParity(data, SERCTL.ParityEven);
		}

		private bool CalculateParity(byte data, bool evenParity)
		{
			// "We have just discovered that the calculation for parity includes the parity bit itself. 
			// Most of us don't like that, but it is too late to change it."

			// TODO: Implement actual odd parity calculation
			return false;
		}

		public void Reset()
		{
			Initialize();

			// "This bit is also set to '1' after a reset."
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