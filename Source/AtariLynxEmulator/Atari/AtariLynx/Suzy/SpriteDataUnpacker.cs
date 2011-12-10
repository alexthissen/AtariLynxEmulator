using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Atari.Lynx;
using System.Diagnostics;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class SpriteDataUnpacker
	{
		private ShiftRegister register; 
		private byte BitsPerPixel;
		private bool totallyLiteral;
		private byte[] data;
		private ushort address;

		public enum OrdinaryDataPacketType
		{
			Packed,
			Literal,
			Unknown
		}

		public SpriteDataUnpacker(ShiftRegister register, byte[] data)
		{
			this.register = register;
			this.data = data;
		}

		public void Initialize(ushort startAddress, byte bitsPerPixel, bool totallyLiteral)
		{
			this.address = startAddress;
			this.BitsPerPixel = bitsPerPixel;
			this.totallyLiteral = totallyLiteral;
		}

		public byte ReadOffsetToNextLine()
		{
			return data[address++];
		}

		internal OrdinaryDataPacketType ReadPacketType()
		{
			byte value = register.GetBits(1);
			switch (value)
			{
				case 0:
					return OrdinaryDataPacketType.Packed;
				case 1:
					return OrdinaryDataPacketType.Literal;
				default:
					Debug.WriteLine("SpriteDataUnpacker::PixelsInLine - Data packet type not 0 or 1.");
					return OrdinaryDataPacketType.Unknown;
			} 
		}

		public void MoveToNextLine(byte offsetToNextLine)
		{
			address += offsetToNextLine;
		}

		public IEnumerable<byte> PixelsInLine(byte offsetToNextLine)
		{
			// Transfer data to register for shifting
			register.Initialize(new ArraySegment<byte>(data, address, offsetToNextLine));
			
			if (totallyLiteral)
			{
				int pixelCount = (offsetToNextLine * 8) / BitsPerPixel;
				
				// Read all pixels except last, to check for bug condition
				for (int index = 0; index < pixelCount - 1; index++) 
				{
					yield return register.GetBits(BitsPerPixel);
				}

				// "There is a bug in the hardware that requires that the last meaningful bit of the
				// data packet at the end of a scan line does not occur in the last bit of a byte (bit 0)."
				// Only return last pixel if value is not zero
				byte value = register.GetBits(BitsPerPixel);
				//if (value != 0x00) yield return value;
				if (register.BitsLeft == 0) yield break;

				// All done
				yield break;
			}
			else // Packed or literal packets
			{
				// Continue only if there are enough bits left
				while (register.BitsLeft >= 5)
				{
					OrdinaryDataPacketType packetType = ReadPacketType();
					byte value;

					switch (packetType)
					{
						case OrdinaryDataPacketType.Packed:
							byte repeatCount = (byte)(register.GetBits(4) + 1);

							// "A data Packet header of '00000' is used as an additional detector of the end of the line of sprite data."
							// "... There appears to be a bug with it."
							// TODO: Find out what bug is
							if (repeatCount == 1) yield break;
							if (!register.TryGetBits(BitsPerPixel, out value)) yield break;

							for (int count = 0; count < repeatCount; count++)
							{
								yield return value;
							}
							break;

						case OrdinaryDataPacketType.Literal:
							byte pixelCount = (byte)(register.GetBits(4) + 1);
							for (int count = 0; count < pixelCount; count++)
							{
								if (!register.TryGetBits(BitsPerPixel, out value)) yield break;
								yield return value;
							}
							break;

						case OrdinaryDataPacketType.Unknown:
							yield break;
					}
				}
			}
		}
	}
}