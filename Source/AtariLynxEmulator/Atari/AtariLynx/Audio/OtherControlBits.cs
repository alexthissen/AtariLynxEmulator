using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class OtherControlBits
	{
		public OtherControlBits(byte initialValue)
		{
			this.ByteData = initialValue;
		}

		public byte ByteData { get; set; }
		
		// "B7=shift register bit 11"
		public bool ShiftRegisterBit11 { get { return (ByteData & ShiftRegisterBit11Mask) == ShiftRegisterBit11Mask; } }

		// "B6=shift register bit 10"
		public bool ShiftRegisterBit10 { get { return (ByteData & ShiftRegisterBit10Mask) == ShiftRegisterBit10Mask; } }

		// "B5=shift register bit 9"
		public bool ShiftRegisterBit9 { get { return (ByteData & ShiftRegisterBit9Mask) == ShiftRegisterBit9Mask; } }

		// "B4=shift register bit 8"
		public bool ShiftRegisterBit8 { get { return (ByteData & ShiftRegisterBit8Mask) == ShiftRegisterBit8Mask; } }

		public const byte ShiftRegisterBit11Mask = 0x80;
		public const byte ShiftRegisterBit10Mask = 0x40;
		public const byte ShiftRegisterBit9Mask = 0x20;
		public const byte ShiftRegisterBit8Mask = 0x10;
		public const byte GlennKnowsMask = 0x08;
		public const byte LastClockStateMask = 0x04;
		public const byte BorrowInMask = 0x02;
		public const byte BorrowOutMask = 0x01;
	}
}
