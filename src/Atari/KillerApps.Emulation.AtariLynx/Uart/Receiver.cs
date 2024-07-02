using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.AtariLynx
{
	public class Receiver
	{
		private SerialControlRegister controlRegister;
		private byte? receiveRegister;
		private int receivePulseCountdown;
		private const int RECEIVE_PERIODS = 11;
		private IComLynxTransport transport = null;
		public event EventHandler<UartDataEventArgs> DataReceived;
		private bool pendingFrameError;
		public string Name;

		public byte SerialData { get; private set; }

		public Receiver(SerialControlRegister register)
		{
			this.controlRegister = register;
		}

		public void AcceptData(byte data)
		{
			// Used for loopback scenarios and fast transports
			if (!IsReceiving)
			{
				// No framing error yet
				pendingFrameError = false;
				receivePulseCountdown = RECEIVE_PERIODS;
			}
			else
			{
				// Collision during receive. Mark framing error at receive
				pendingFrameError = true;
				//controlRegister.FrameError = true;
			}

			// Data is always accepted, even for framing errors
			receiveRegister = data;
		}

		public bool IsReceiving { get { return receiveRegister.HasValue; } }

		public void HandleBaudPulse(object sender, EventArgs e)
		{
			receivePulseCountdown--;

			if (receivePulseCountdown == 0 && receiveRegister.HasValue)
			{
				byte data = receiveRegister.Value;
				ReceiveData(data);
			}
		}

		public void ReceiveError(byte data, bool parity, bool overrun, bool framing)
		{
			if (parity) controlRegister.ParityError = true;
			if (overrun) controlRegister.OverrunError = true;
			if (framing) controlRegister.FrameError = true;

			// Data is received even if an error occurs
			ReceiveData(data);
		}

		public void ReceiveData(byte data)
		{
			// Data is ready to receive
			SerialData = data;

			// Remove data currently received from holding register
			receiveRegister = null;
			receivePulseCountdown = 0;

			// Check for overrun
			if (controlRegister.ReceiveReady)
			{
				// Previous data has not been read yet, so overrun 
				controlRegister.OverrunError = true;
			}

			// Mark receive ready
			controlRegister.ReceiveReady = true;

			if (pendingFrameError)
			{
				// Report framing error that occurred during receive
				controlRegister.FrameError = true;

				// TODO (UART): Check whether a second byte needs to be send because of the overlap in data
				// Second frame with bogus data
				// AcceptData(0x23);
				// This is an instant framing error, as it is the result of a previous overlap
				//pendingFrameError = true;
			}

			// Complete sending current shift register contents
			UartDataEventArgs args = new UartDataEventArgs()
				{
					Break = false,
					Data = data,
					StopBitPresent = false
				};
			OnReceived(args);
		}

		protected virtual void OnReceived(UartDataEventArgs args)
		{
			args.ParityBit = Uart.ComputeParityBit(args.Data, controlRegister);
			if (DataReceived != null) DataReceived(this, args);
		}

		public void HandleDataTransmitting(object sender, UartDataEventArgs e)
		{
			// When there is no physical or emulated transport, force a software loopback
			if (transport == null)
			{
				// TODO: Check for break signal
				AcceptData(e.Data);
			}
		}
	}
}
