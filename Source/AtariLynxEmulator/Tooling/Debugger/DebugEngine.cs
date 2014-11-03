using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace KillerApps.Emulation.Atari.Lynx.Debugger
{
	public class DebugEngine
	{
		DebugPort port;
		ManualResetEvent signal = new ManualResetEvent(false);
		Queue<IDebugResponse> responses = new Queue<IDebugResponse>();
		BreakpointManager manager;
		Registers currentRegisters;
		bool breakMode = false, attached = false;
		public bool IsRunning { get { return !breakMode; } }

		public DebugEngine()
		{
			port = new DebugPort();
			manager = new BreakpointManager(this);
		}

		public void Attach(string portName, int baudrate, Parity parity)
		{
			Logger.Current.Info(String.Format("Attaching to port {0}.", portName));
			port.DebugResponseReceived += OnDebugResponseReceived;
			port.Start(portName, baudrate, parity);
			Logger.Current.Info(String.Format("Listening at {0} baud.", baudrate));
			attached = true;
			breakMode = false;
		}

		public void Detach()
		{
			Logger.Current.Info("Stopping debugging port.");
			port.Stop();
			attached = false;
		}

		public void Step()
		{
			EnsureAttached();
			EnsureInBreakmode();

			ushort nextBreakpointAddress;
			byte[] expressionBytes = InspectMemory(currentRegisters.PS, 3);

			// Find potential breakpoint
			DebugBreakpoint breakPoint = manager.FindBreakpoint(currentRegisters.PC);
			if (breakPoint != null)
			{
				expressionBytes[0] = breakPoint.Code;
			}

			// TODO: Determine new address
			nextBreakpointAddress = 0x0000;

			manager.SetBreakpoint(nextBreakpointAddress);
			Continue();
		}

		public void Continue()
		{
			EnsureAttached();
			EnsureInBreakmode();

			// Clear potential breakpoint at current location
			manager.ClearBreakpoint(currentRegisters.PC);

			// And off we go
			SendAndWait(new ContinueRequest());
			breakMode = false;
		}

		public byte[] InspectMemory(ushort startAddress, ushort size)
		{
			List<byte> memory = new List<byte>();

			while (size > 0)
			{
				byte length = (byte)(size < 256 ? size : 0);
				ReadMemoryResponse response = SendAndReceive<ReadMemoryResponse>(new ReadMemoryRequest(startAddress, length), 2000);
				ushort bytesRead = (ushort)response.Memory.Length;
				memory.AddRange(response.Memory);

				startAddress += bytesRead;
				size -= bytesRead;
			}

			return memory.ToArray();
		}

		void OnDebugResponseReceived(object sender, DebugResponseReceivedEventArgs e)
		{
			if (IsRunning)
			{
				TryEnterBreakmode(e.Response);
				return;
			}

			// Store last response in queue
			responses.Enqueue(e.Response);
			signal.Set();
		}

		private void TryEnterBreakmode(IDebugResponse response)
		{
			SendRegistersResponse registersResponse = response as SendRegistersResponse;
			if (registersResponse == null) return;

			currentRegisters = registersResponse.ToRegisters();
			breakMode = true;
		}

		internal T SendAndReceive<T>(DebugRequest request, int timeout = 1000) where T : class, IDebugResponse
		{
			signal.Reset();
			port.SendRequest(request);
			if (!signal.WaitOne(timeout))
			{
				Logger.Current.Warning("Response not received in time.");
				return null;
			}

			// Verify that request is echoed correctly
			// TODO: Implement equality operator for each request type
			//			if (lastResponse.Echo != request) return null;
			IDebugResponse response = responses.Dequeue();
			return response as T;
		}

		internal bool SendAndWait(DebugRequest request, int timeout = 1000)
		{
			signal.Reset();
			port.SendRequest(request);
			if (!signal.WaitOne(timeout))
			{
				Logger.Current.Warning("Response not received in time.");
				return false;
			}

			// Check whether right response was received
			while (responses.Count > 0)
			{
				IDebugResponse response = responses.Dequeue();
				if (VerifyResponse(response, request)) return true;
			}

			return false;
		}

		public bool VerifyResponse(IDebugResponse response, DebugRequest request)
		{
			// Type of echoed request must match
			if (response.RequestEcho.GetType() != request.GetType()) return false;

			// TODO: Compare contents of both request messages
			return true;
		}

		void EnsureAttached()
		{
			if (!attached) throw new InvalidOperationException("Debugger is not attached to debuggee.");
		}

		private void EnsureInBreakmode()
		{
			if (!breakMode) throw new InvalidOperationException("Debugger is not in break mode.");
		}
	}
}
