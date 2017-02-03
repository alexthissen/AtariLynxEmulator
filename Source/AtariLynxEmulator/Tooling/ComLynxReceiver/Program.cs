using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComLynxReceiver
{
	class Program
	{
		static void Main(string[] args)
		{
			SerialPort port = new SerialPort("COM4", 62500, Parity.Mark, 8, StopBits.One);
			port.ReceivedBytesThreshold = 256;
			port.ReadBufferSize = 256;
			port.DataReceived += OnDataReceived;
			port.Open();

			Console.WriteLine("Listening. Press ENTER to quit");
			Console.ReadLine();

			File.WriteAllBytes(@".\dump.bin", file);
		}

		static int totalBytes = 0;
		static int bytesRead = 0;
		static byte[] file = new byte[65536*8+1024];

		static void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			SerialPort port = (SerialPort)sender;
			byte[] buffer = new byte[256];
			bytesRead = port.Read(buffer, 0, 256);
			Array.Copy(buffer, 0, file, totalBytes, bytesRead);
			totalBytes += bytesRead;
			Console.Clear();
			Console.WriteLine("Read bytes: {0}", bytesRead);
			Console.WriteLine("Total bytes: {0}", totalBytes);
		}
	}
}
