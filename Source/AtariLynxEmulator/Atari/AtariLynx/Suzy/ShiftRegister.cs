using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class ShiftRegister
	{
		private int counter, index;
		private ushort register = 0;
		ArraySegment<byte> data;
		public int Size { get; private set; }

		public ShiftRegister(int size)
		{
			this.Size = size;
		}
		
		public void Initialize(ArraySegment<byte> data)
		{
			this.data = data;
			counter = 0;
			index = data.Offset;
			register = 0;
			
			// Add initial set of bytes
			AddBytes();
		}

		public bool TryGetBits(int bitCount, out byte value)
		{
			Contract.Assert(bitCount >= 0 && bitCount <= 8);

			value = 0;
			if (counter < bitCount)
			{
				// Check if queue has enough data 
				if (BitsLeft < bitCount) return false;
				AddBytes();
			}

			value = (byte)((register >> (counter - bitCount)) & ((1 << bitCount) - 1));
			counter -= bitCount;

			return true;
		}

		public byte GetBits(int bitCount)
		{
			byte value;
			bool result = TryGetBits(bitCount, out value);
			if (!result)
			{
				throw new ArgumentException("Number of bits was larger than remaining bits", "bitCount");
			}
			return value;
		}

		public int BitsLeft
		{
			get { return counter + (data.Count + data.Offset - index) * 8; }
		}

		private void AddBytes()
		{
			int increment = Math.Min(data.Count + data.Offset - index, Size / 8);
			register <<= increment * 8;
			for (int shift = increment; shift > 0; shift--)
			{
				register |= (byte)(data.Array[index++] << ((shift - 1) * 8));
			}
			counter += increment * 8;
		}
	}
}
