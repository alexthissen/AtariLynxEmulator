using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	internal class Uart: IResetable
	{
		// Comlynx related variables
		public byte SERDAT { get; set; }
		public SerialControlRegister SERCTL { get; set; }

		private Queue<byte> receiveBuffer = new Queue<byte>(32);
		//private Queue<byte> transmitBuffer = new Queue<byte>(32);
		private int transmitCountdown;
		private int receiveCountdown;
		// TODO: Factor out receive inactive flag
		private bool receiveInactive = true;

		public IComLynxTransport ComLynxTransport { get; set; }

		public Uart()
		{
			Initialize();
		}

		public void Initialize()
		{
			SERCTL = new SerialControlRegister(0x00); // "reset 0,0,0,0,0,0,0,0"
		}

		private void ComLynxTransmitLoopback(byte value)
		{
			if (receiveBuffer.Count <= 31)
			{
				receiveBuffer.Enqueue(value);
				// It will take 1 + 8 + 1 + 1 bits = 11 timer expirations for single byte to be transmitted
				receiveCountdown = 11;
				receiveInactive = false;
			}
			else
				// TODO: Check if this is correct
				SERCTL.OverrunError = true;
		}

		public void TransmitSerialData(byte data)
		{
			transmitCountdown = 11;
			SERCTL.TransmitterDone = false;
			//SERDAT = value;
			ComLynxTransmitLoopback(data);
			if (ComLynxTransport != null)
			{
				ComLynxTransport.Send(data);
			}
		}

		public void SetSerialControlRegister(byte value)
		{
			SERCTL.ByteData = value;

			// "Once received, these error bits remain set until they are cleared by writing to the control 
			// byte with the reset error bit set."
			if (SERCTL.ResetAllErrors)
			{
				SERCTL.OverrunError = false;
				SERCTL.FrameError = false;
			}
			if (SERCTL.TransmitBreak)
			{
				// TODO: Transmit break signal
			}
		}

		public void TransmitBreak() 
		{ 
			// TODO: Implement for real
		}

		public bool GenerateBaudRate()
		{
			bool fireInterrupt = false;
			if (receiveCountdown == 0 && !receiveInactive)
			{
				if (receiveBuffer.Count > 0)
					SERDAT = receiveBuffer.Dequeue();

				if (receiveBuffer.Count > 0)
				{
					receiveCountdown = 11 + 44;
					receiveInactive = false;
				}
				else
				{
					receiveInactive = true;
				}
				
				if (SERCTL.ReceiveReady)
					SERCTL.OverrunError = true;
				
				SERCTL.ReceiveReady = true;
			}
			else if (!receiveInactive)
			{
				receiveCountdown--;
			}

			if (transmitCountdown == 0 && !SERCTL.TransmitterDone)
			{
				if (SERCTL.TransmitBreak)
				{
					// TODO: Implement break transmission
				}
				else
					SERCTL.TransmitterDone = true;
			}
			else if (!SERCTL.TransmitterDone)
				transmitCountdown--;

			// "Well, we did screw something up after all. 
			// Both the transmit and receive interrupts are 'level' sensitive, rather than 'edge' sensitive. 
			// This means that an interrupt will be continuously generated as long as it is enabled and 
			// its UART buffer is ready. 
			// As a result, the software must disable the interrupt prior to clearing it. Sorry."

			// Emulate the UART bug where UART IRQ is level sensitive
			if ((SERCTL.TransmitterDone && SERCTL.TransmitterInterruptEnable) || (SERCTL.ReceiveReady && SERCTL.ReceiveInterruptEnable))
			{
				fireInterrupt = true;
			}

			return fireInterrupt;
		}

		public void Reset()
		{
			Initialize();
		}
	}
}