using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Processors
{
	public enum AddressingMode
	{
		Illegal = 0,
		Implicit,
		Accumulator, // A
		Immediate, // #$12
		Relative, // $1234
		ZeroPage, // $12
		ZeroPageX, // $12,X
		ZeroPageY, // $12,Y
		ZeroPageIndirect, // ($12)
		ZeroPageIndexedIndirectX, // ($12,X)
		ZeroPageIndirectIndexedY, // ($12),Y
		Absolute, // $1234
		AbsoluteX, // $1234,X
		AbsoluteY, // $1234,Y
		AbsoluteIndirect, // ($1234)
		AbsoluteIndexedIndirectX // ($1234,X)
	}
}
