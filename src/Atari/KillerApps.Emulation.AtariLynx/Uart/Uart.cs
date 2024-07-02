using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.AtariLynx
{
	public class Uart: IResetable
	{
		// TODO: Change back to internal
		public Transmitter Transmitter;
		public Receiver Receiver;
		internal SerialControlRegister SerialControlRegister;
		public event EventHandler BaudPulse;
		private IComLynxTransport transport = null;
		public string Name;

		public Uart() : this(new SerialControlRegister()) { }
		public Uart(SerialControlRegister controlRegister)
		{
			SerialControlRegister = controlRegister;
			Transmitter = new Transmitter(controlRegister);
			Receiver = new Receiver(controlRegister);

			BaudPulse += Transmitter.HandleBaudPulse;
			BaudPulse += Receiver.HandleBaudPulse;
			Transmitter.DataTransmitting += OnTransmitting;
			Transmitter.DataTransmitting += Receiver.HandleDataTransmitting;
		}

		public void Initialize()
		{
			//BaudPulse += Transmitter.HandleBaudPulse;
			//BaudPulse += Receiver.HandleBaudPulse;
			//Transmitter.DataTransmitting += Receiver.HandleDataTransmitting;
			//Transmitter.DataTransmitting += OnTransmitting;
		}

		private void OnTransmitting(object sender, UartDataEventArgs e)
		{
			if (transport != null) transport.Send(e.Data);
		}

		public void InsertCable(IComLynxTransport transport)
		{
			this.transport = transport;
			transport.Connect(Transmitter, Receiver);
			transport.Initialize();
		}

		protected virtual void OnBaudPulse()
		{
			if (BaudPulse != null) BaudPulse(this, EventArgs.Empty);
		}

		public bool GenerateBaudPulse()
		{
			// Pulse all subscribers to baud rate (transmitter and receiver)
			OnBaudPulse();

			// "Both the transmit and receive interrupts are 'level' sensitive, 
			// rather than 'edge' sensitive. This means that an interrupt will be 
			// continuously generated as long as it is enabled and its UART buffer is ready. 
			// As a result, the software must disable the interrupt prior to clearing it."
			if (SerialControlRegister.ReceiveInterruptEnable && SerialControlRegister.ReceiveReady)
				return true;

			// TODO (UART): Check whether interrupt fires when transmit buffer is empty, 
			// or when transmitter totally done
			// Seems to be on BufferEmpty, because that gives some results
			if (SerialControlRegister.TransmitterInterruptEnable && SerialControlRegister.TransmitterBufferEmpty)
				return true;

			// Otherwise no interrupt is triggered.
			return false;
		}

		public byte SERCTL
		{
			get { return SerialControlRegister.ByteData; }
			set
			{
				WriteSerialControlRegister(value);
			}
		}

		public byte SERDAT
		{
			get
			{
				SerialControlRegister.ReceiveReady = false;
				return Receiver.SerialData;
			}
			set
			{
				Transmitter.TransferToBuffer(value);
			}
		}

		private void WriteSerialControlRegister(byte data)
		{
			SerialControlRegister.ByteData = data;
			if ((data & SerialControlRegister.RESETERRMask) == SerialControlRegister.RESETERRMask)
			{
				SerialControlRegister.FrameError = false;
				SerialControlRegister.ParityError = false;
				SerialControlRegister.OverrunError = false;
			}
			if (transport != null)
			{
				// TODO: Remove hardcoded baud rate
				//transport.ChangeSettings(SerialControlRegister, 62500);
			}
		}

		public void Reset()
		{
			// "This bit is also set to '1' after a reset." refers to TXRDY
			SerialControlRegister.TransmitterBufferEmpty = true;
			SerialControlRegister.TransmitterTotallyDone = true;
			SerialControlRegister.ReceiveReady = false;
			SerialControlRegister.ParityError = false;
			SerialControlRegister.OverrunError = false;
			SerialControlRegister.FrameError = false;
			SerialControlRegister.ReceivedBreak = false;
			SerialControlRegister.ParityBit = false;
		}

		public static bool ComputeParityBit(byte data, SerialControlRegister register)
		{
			// "The 9th bit is always sent. It is either the result of a parity calculation on the transmit 
			// data byte or it is the value set in the parity select bit in the control register.
			// The choice is made by the parity enable bit in the control byte. For example :
			// If PAREN is '0', then the 9th bit will be whatever the state of PAREVEN is."
			if (!register.TransmitParityEnable) return register.ParityEven;

			// If PAREN is '1' and PAREVEN is '0', then the 9th bit will be the result of an 'odd' parity calculation 
			// on the transmit data byte.
			return CalculateParity(data, register.ParityEven);
		}

		public static bool CalculateParity(byte data, bool evenParity)
		{
			// "We have just discovered that the calculation for parity includes the parity bit itself. 
			// Most of us don't like that, but it is too late to change it."

			int count = 0;
			BitArray bits = new BitArray(new byte[] { data });
			for (int index = 0; index < 8; index++)
			{
				if (bits[index]) count++;
			}
			return (count % 2 == 0) ^ evenParity;
		}
	}
}
