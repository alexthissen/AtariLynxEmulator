using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Atari.Lynx.Debugger
{
	public class DebugPort
	{
		public void Start()
		{
			SerialPort port = new SerialPort("COM6", 62500, Parity.Space, 8, StopBits.One);
			port.ReceivedBytesThreshold = 1;
			port.ReadBufferSize = 256;
			port.WriteBufferSize = 256;
			//port.DataReceived += OnDataReceived;

			var observablePort = Observable.FromEventPattern<SerialDataReceivedEventHandler, SerialDataReceivedEventArgs>(
				ev => port.DataReceived += ev,
				ev => port.DataReceived -= ev).Select(args =>
				{
					int bytesToRead = port.BytesToRead;
					byte[] buffer = new byte[bytesToRead];
					int bytesRead = port.Read(buffer, 0, bytesToRead);
					return buffer;
				});

			// Naieve implementation
			//Queue<byte> queue = new Queue<byte>();
			//observablePort.Subscribe(buffer => buffer.ToList().ForEach(b => queue.Enqueue(b)));

			var observableSerialData = Observable.Create<IDebugResponse>(observer =>
			{
				IDebugResponse response = null;
				var subscription = observablePort.Subscribe(buffer =>
				{
					// Check how long buffer of received bytes is
					var bytesRead = buffer.Length;
					int index = 0;

					// 
					while (index < bytesRead)
					{
						if (response == null)
						{
							// Create response based on prolog byte. 
							response = DebugResponseFactory.CreateResponse(buffer[index++]);

							//If not present logic will skip bytes until non-null response is returned
						}
						else
						{
							// Take as many bytes as are needed to complete current response
							int availableBytes = Math.Min(bytesRead - index, response.RemainingBytes);
							response.AddBytes(buffer.Skip(index).Take(availableBytes));
							index += availableBytes;

							// Even though correct number of bytes might have been added, response could still be 
							// incomplete because length of data to receive has changed (when length byte is available)
							if (response.IsComplete)
							{
								// Current response is ready, so return to observers
								observer.OnNext(response);
								response = null;
							}
						}
					}
				});
				return Disposable.Empty;
			}
			);
		}
	}
}
