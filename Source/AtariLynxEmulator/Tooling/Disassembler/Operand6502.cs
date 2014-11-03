using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Processors
{
	public class Operand6502
	{
		public Operand6502(byte msb, byte lsb)
		{
			value = new byte[] { lsb, msb };
		}

		public Operand6502(byte single)
		{
			value = new byte[] { single };
		}

		public Operand6502(ushort word)
		{
			value = BitConverter.GetBytes(word);
		}

		private byte[] value;

		public object Value
		{
			get
			{
				if (value.Length == 2) return BitConverter.ToInt16(value, 0);
				if (value.Length == 1) return value[0];
				return null;
			}
		}
	}
}
