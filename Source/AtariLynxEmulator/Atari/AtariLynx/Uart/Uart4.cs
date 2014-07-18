using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class Uart4: IResetable
	{
		private Transmitter2 transmitter;
		private Receiver receiver;
		internal SerialControlRegister2 SerialControlRegister;
		public event EventHandler BaudPulse;

		public Uart4(): this(new SerialControlRegister2()) { }
		public Uart4(SerialControlRegister2 controlRegister)
		{
			SerialControlRegister = controlRegister;
			transmitter = new Transmitter2(controlRegister);
			receiver = new Receiver(controlRegister);
		}

		public void Initialize()
		{
			BaudPulse += transmitter.HandleBaudPulse;
			BaudPulse += receiver.HandleBaudPulse;
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
			// TODO: Check whether interrupt fires when transmit buffer is empty, 
			// or when transmitter totally done
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

		private void WriteSerialControlRegister(byte data)
		{
			SerialControlRegister.ByteData = data;
			if ((data & SerialControlRegister2.RESETERRMask) == SerialControlRegister2.RESETERRMask)
			{
				SerialControlRegister.FrameError = false;
				SerialControlRegister.ParityError = false;
				SerialControlRegister.OverrunError = false;
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

		public static bool ComputeParityBit(byte data, SerialControlRegister2 register)
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
