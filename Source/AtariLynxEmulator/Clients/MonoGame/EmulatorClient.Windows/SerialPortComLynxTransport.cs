using KillerApps.Emulation.Atari.Lynx;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace EmulatorClient.Windows
{
	public class SerialPortComLynxTransport: IComLynxTransport
	{
		public const int WRITE_BUFFER_SIZE = 1;
		public const int READ_BUFFER_SIZE = 1;
	
		private SerialPort port = new SerialPort("COM5");
		private byte[] buffer = new byte[WRITE_BUFFER_SIZE];
		private Receiver receiver;	
		private Transmitter transmitter;

		public void Connect(Transmitter transmitter, Receiver receiver)
		{
			this.transmitter = transmitter;
			this.receiver = receiver;
		}

		public void Initialize()
		{
			port.BaudRate = 62500;// 9600;
			port.Parity = Parity.Odd; //Mark;
			port.StopBits = StopBits.One;
			port.DataBits = 8;
			port.DataReceived += OnDataReceived;
			port.ErrorReceived += OnErrorReceived;
			port.Open();
		}

		public void ChangeSettings(SerialControlRegister register, int baudrate)
		{
			if (port.IsOpen) port.Close();

			port.BaudRate = baudrate;
			if (register.TransmitParityEnable)
			{
				port.Parity = register.ParityEven ? Parity.Even : Parity.Odd;
			}
			else
			{
				port.Parity = register.ParityEven ? Parity.Mark : Parity.Space;
			}
			port.Open();
		}

		void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
		{
			// TODO: Check whether data is received on error
			SerialError error = e.EventType;
			receiver.ReceiveError(0x00,
				error == SerialError.RXParity,
				error == SerialError.Overrun,
				error == SerialError.Frame);
		}

		void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			byte data = (byte)port.ReadByte();
			//port.DiscardInBuffer();
			Receive(data);
		}

		public void Send(byte data)
		{
			buffer[0] = data;
			port.Write(buffer, 0, WRITE_BUFFER_SIZE);
		}

		public void Receive(byte data)
		{
			// When data has arrived through serial port, it is available immediately
			receiver.ReceiveData(data);
		}
	}
}
