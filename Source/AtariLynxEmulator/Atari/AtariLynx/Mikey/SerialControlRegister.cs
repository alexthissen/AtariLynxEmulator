using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class SerialControlRegister
	{
		public SerialControlRegister(byte data)
		{
			ByteData = data;
			TransmitterDone = true;
		}

		public byte ByteData
		{
			get 
			{
				byte data = 0x00;
				if (TransmitterDone)
				{
					data |= TXRDYMask;
					data |= TXEMPTYMask;
				}
				if (ReceiveReady) data |= RXRDYMask;
				//if (TransmitterDone) data |= TXEMPTYMask;
				if (ParityError) data |= PARERRMask;
				if (OverrunError) data |= OVERRUNMask;
				if (FrameError) data |= FRAMERRMask;
				if (ReceivedBreak) data |= RXBRKMask;
				if (ParityBit) data |= PARBITMask;
				return data;
			}
			set 
			{
				TransmitterInterruptEnable = (value & TXINTENMask) == TXINTENMask;
				ReceiveInterruptEnable = (value & RXINTENMask) == RXINTENMask;
				TransmitParityEnable = (value & PARENMask) == PARENMask;
				ResetAllErrors = (value & RESETERRMask) == RESETERRMask;
				TransmitOpen = (value & TXOPENMask) == TXOPENMask;
				TransmitBreak = (value & TXBRKMask) == TXBRKMask;
				ParityEven = (value & PAREVENMask) == PAREVENMask;
			}
		}

		#region Write properties

		// "B7 = TXINTEN transmitter interrupt enable"
		public bool TransmitterInterruptEnable { internal get; set; }

		// "B6 = RXINTEN receive interrupt enable"
		public bool ReceiveInterruptEnable { internal get; set; }

		// "B4 = PAREN xmit parity enable (if 0, PAREVEN is the bit sent)"
		public bool TransmitParityEnable { internal get; set; }

		// "B3 = RESETERR reset all errors"
		public bool ResetAllErrors { internal get; set; }

		// "B2 = TXOPEN 1 open collector driver, 0 = TTL driver"
		public bool TransmitOpen { internal get; set; }
		
		// "B1 = TXBRK send a break (for as long as the bit is set)"
		public bool TransmitBreak { internal get; set; }

		// "B0 = PAREVEN send/rcv even parity"
		public bool ParityEven { internal get; set; }
 
	#endregion

		#region Read properties

		// "B7 = TXRDY transmitter buffer empty"
		public bool TransmitBufferEmpty { get; internal set; }

		// "B6 = RXRDY receive character ready"
		public bool ReceiveReady { get; internal set; }

		// "B5 = TXEMPTY transmitter totally done"
		public bool TransmitterDone { get; internal set; }

		// "B4 = PARERR received parity error"
		public bool ParityError { get; internal set; }

		// "B3 = OVERRUN received overrun error"
		public bool OverrunError { get; internal set; }

		// "B2 = FRAMERR received framing error"
		public bool FrameError { get; internal set; }

		// "B1 = RXBRK break recieved (24 bit periods)"
		public bool ReceivedBreak { get; internal set; }

		// "B0 = PARBIT 9th bit"
		public bool ParityBit { get; internal set; }
		
		#endregion

		private const byte TXINTENMask = 0x80;
		private const byte RXINTENMask = 0x40;
		private const byte PARENMask = 0x10;
		private const byte RESETERRMask = 0x08;
		private const byte TXOPENMask = 0x04;
		private const byte TXBRKMask = 0x02;
		private const byte PAREVENMask = 0x01; 
		
		private const byte TXRDYMask = 0x80;
		private const byte RXRDYMask = 0x40;
		private const byte TXEMPTYMask = 0x20;
		private const byte PARERRMask = 0x10;
		private const byte OVERRUNMask = 0x08;
		private const byte FRAMERRMask = 0x04;
		private const byte RXBRKMask = 0x02;
		private const byte PARBITMask = 0x01; 
	}
}
