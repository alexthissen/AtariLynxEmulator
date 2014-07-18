using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class Transmitter2
	{
		private SerialControlRegister2 controlRegister;
		
		private byte bufferRegister;
		private byte? shiftRegister;

		// Countdown per pulse
		private int transmitPulseCountdown;
		private const int TRANSMIT_PERIODS = 11;

		public event EventHandler<UartDataEventArgs> DataTransmitting;
		//public event EventHandler TransmitBufferReady;

		public Transmitter2(SerialControlRegister2 register)
		{
			this.controlRegister = register;
		}

		internal void TransferToBuffer(byte data)
		{
			bufferRegister = data;
			controlRegister.TransmitterBufferEmpty = false;
			controlRegister.TransmitterTotallyDone = false;
		}

		public byte Buffer { get { return bufferRegister; } }

		public void HandleBaudPulse(object sender, EventArgs e)
		{
			// When shift register is empty, start transfer now
			if (!shiftRegister.HasValue && !controlRegister.TransmitterBufferEmpty)
			{
				StartTransmission();
			}

			// Transmission already active, so decrease transfer counter
			transmitPulseCountdown--;

			if (transmitPulseCountdown == 0)
			{
				// No more data currently in shift register
				shiftRegister = null;

				// Check whether there is no more data to transfer
				if (controlRegister.TransmitterBufferEmpty) controlRegister.TransmitterTotallyDone = true;
			}
		}

		private void StartTransmission()
		{
			shiftRegister = bufferRegister;
			controlRegister.TransmitterBufferEmpty = true;
			controlRegister.TransmitterTotallyDone = false;
			transmitPulseCountdown = TRANSMIT_PERIODS;

			//OnTransmitBufferReady();

			// Immediately start with the countdown
			// TODO: Check whether there is an additional cycle required 
			// for transferring from buffer to shift register

			// Send actual data across ComLynx wire
			TransmitData(shiftRegister.Value);
		}

		//protected virtual void OnTransmitBufferReady()
		//{
		//	if (TransmitBufferReady != null) TransmitBufferReady(this, EventArgs.Empty);
		//}

		public void TransmitData(byte data)
		{
			// Complete sending current shift register contents
			UartDataEventArgs args = new UartDataEventArgs() 
			{ 
				Break = false, 
				Data = data, 
				StopBitPresent = true 
			};
			OnTransmit(args);
		}

		protected virtual void OnTransmit(UartDataEventArgs args)
		{
			args.ParityBit = Uart4.ComputeParityBit(args.Data, controlRegister);
			if (DataTransmitting != null) DataTransmitting(this, args);
		}
	}
}
