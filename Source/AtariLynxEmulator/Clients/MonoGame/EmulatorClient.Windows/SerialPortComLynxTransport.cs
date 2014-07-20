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
		private Transmitter2 transmitter;

		public SerialPortComLynxTransport(Transmitter2 transmitter, Receiver receiver)
		{
			this.transmitter = transmitter;
			this.receiver = receiver;
		}

		public void Initialize()
		{
			port.BaudRate = 62500;
			port.Parity = Parity.Odd;
			port.StopBits = StopBits.One;
			port.DataBits = 8;
			port.WriteBufferSize = WRITE_BUFFER_SIZE;
			port.ReadBufferSize = READ_BUFFER_SIZE;
			port.DataReceived += OnDataReceived;
			port.ErrorReceived += OnErrorReceived;
			port.Open();
		}

		void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
		{
			
		}

		void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			byte data = (byte)port.ReadByte();
			Receive(data);
		}

		public void Send(byte data)
		{
			buffer[0] = data;
			port.Write(buffer, 0, WRITE_BUFFER_SIZE);
		}

		public void Receive(byte data)
		{
			// TODO: Accept data
		}
	}
}
