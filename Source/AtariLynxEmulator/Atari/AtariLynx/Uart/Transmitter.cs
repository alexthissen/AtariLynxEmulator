using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	internal class Transmitter
	{
		private byte transmitHoldingRegister;
		private byte transmitShiftRegister;
		private int transmitPulseCountdown;

		private const byte TRANSMIT_PERIODS = 11;
		public event EventHandler<UartDataEventArgs> DataTransmitted;

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
		}

		protected virtual void OnTransmit(UartDataEventArgs args)
		{
			args.ParityBit = ComputeParityBit(args.Data);
			if (DataTransmitted != null) DataTransmitted(this, args);
		}

		private bool ComputeParityBit(byte data)
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
	}
}
