using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class SerialControlRegister
	{
		public SerialControlRegister()
		{
			TransmitterBufferEmpty = true;
			TransmitterTotallyDone = true;
		}

		public byte ByteData
		{
			get 
			{
				byte data = 0x00;

				if (TransmitterTotallyDone) data |= TXEMPTYMask;
				if (TransmitterBufferEmpty) data |= TXRDYMask;
				if (ReceiveReady) data |= RXRDYMask;
				if (ParityError) data |= PARERRMask;
				if (OverrunError) data |= OVERRUNMask;
				if (FrameError) data |= FRAMERRMask;
				if (ReceivedBreak) data |= RXBRKMask;
				if (ParityBit) data |= PARBITMask;

				return data;
			}
			set 
			{
				// "B5 = 0 (for future compatibility)"
				value &= 0xdf;

				TransmitterInterruptEnable = (value & TXINTENMask) == TXINTENMask;
				ReceiveInterruptEnable = (value & RXINTENMask) == RXINTENMask;
				TransmitParityEnable = (value & PARENMask) == PARENMask;
				ResetAllErrors = (value & RESETERRMask) == RESETERRMask;
				TransmitOpenCollector = (value & TXOPENMask) == TXOPENMask;
				TransmitBreak = (value & TXBRKMask) == TXBRKMask;
				ParityEven = (value & PAREVENMask) == PAREVENMask;
			}
		}

		#region Write properties
		// Values can only be written by accessing ByteData, i.e. Poke to SERCTL

		// "B7 = TXINTEN transmitter interrupt enable"
		public bool TransmitterInterruptEnable { get; private set; }

		// "B6 = RXINTEN receive interrupt enable"
		public bool ReceiveInterruptEnable { get; private set; }

		// "B4 = PAREN xmit parity enable (if 0, PAREVEN is the bit sent)"
		public bool TransmitParityEnable { get; private set; }

		// "B3 = RESETERR reset all errors"
		public bool ResetAllErrors { get; private set; }

		// "B2 = TXOPEN 1 open collector driver, 0 = TTL driver"
		public bool TransmitOpenCollector { get; private set; }
		
		// "B1 = TXBRK send a break (for as long as the bit is set)"
		public bool TransmitBreak { get; private set; }

		// "B0 = PAREVEN send/rcv even parity"
		public bool ParityEven { get; private set; }
 
	#endregion

		#region Read properties

		// "There are 2 status bits for the transmitter, TXRDY (transmit buffer ready) and TXEMPTY (transmitter totally done)."
		// "B7 = TXRDY transmitter buffer empty"
		public bool TransmitterBufferEmpty { get; internal set; }

		// "B6 = RXRDY receive character ready"
		public bool ReceiveReady { get; internal set; }

		// "B5 = TXEMPTY transmitter totally done"
		public bool TransmitterTotallyDone { get; internal set; }

		// "There are 3 receive errors, parity error (already explained), framing error, and overrun error."
		public bool ParityError { get; internal set; } // "B4 = PARERR received parity error"
		public bool OverrunError { get; internal set; } // "B3 = OVERRUN received overrun error"
		public bool FrameError { get; internal set; } // "B2 = FRAMERR received framing error"

		// "B1 = RXBRK break recieved (24 bit periods)"
		public bool ReceivedBreak { get; internal set; }

		// "The state of the 9th bit is always available for read in the control byte."
		// "B0 = PARBIT 9th bit"
		public bool ParityBit { get; internal set; }
		
		#endregion

		public const byte TXINTENMask = 0x80;
		public const byte RXINTENMask = 0x40;
		public const byte PARENMask = 0x10;
		public const byte RESETERRMask = 0x08;
		public const byte TXOPENMask = 0x04;
		public const byte TXBRKMask = 0x02;
		public const byte PAREVENMask = 0x01; 
		
		public const byte TXRDYMask = 0x80;
		public const byte RXRDYMask = 0x40;
		public const byte TXEMPTYMask = 0x20;
		public const byte PARERRMask = 0x10;
		public const byte OVERRUNMask = 0x08;
		public const byte FRAMERRMask = 0x04;
		public const byte RXBRKMask = 0x02;
		public const byte PARBITMask = 0x01; 
	}
}
