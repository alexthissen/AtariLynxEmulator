using KillerApps.Emulation.Core;
using KillerApps.Emulation.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Processors
{
	public class Disassembler6502
	{
		public string Disassemble(IMemoryAccess<ushort, byte> memory, ushort address, int range)
		{
			Dictionary<ushort, Expression6502> expressions = new Dictionary<ushort, Expression6502>();
			StringBuilder builder = new StringBuilder();

			foreach (Expression6502 expression in memory.Disassemble(address).Take(range))
			{
				builder.Append(expression);
				expressions.Add(expression.Address, expression);
			}
			return builder.ToString();
		}
	}
}

