using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Processors
{
	public enum AddressingMode
	{
		Illegal = 0,
		Accumulator, // A
		Immediate, // #$12
		Implicit,
		Relative, // $1234
		Absolute, // $1234
		AbsoluteIndirect, // ($1234)
		AbsoluteX, // $1234,X
		AbsoluteY, // $1234,Y
		AbsoluteIndexedIndirectX, // ($1234,X)
		ZeroPage, // $12
		ZeroPageX, // $12,X
		ZeroPageY, // $12,Y
		ZeroPageIndexedIndirectX, // ($12,X)
		ZeroPageIndirectIndexedY, // ($12),Y
		ZeroPageIndirect // ($12)
	}
}
