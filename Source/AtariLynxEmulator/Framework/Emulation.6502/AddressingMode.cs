using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Processors
{
	public enum AddressingMode
	{
		Illegal = 0,
		Accumulator, //acc A
		Immediate, //imm #$12
		Absolute, //absl $1234
		ZeroPage, //zp $12
		ZeroPageX, //zpx $12,X
		ZeroPageY, //zpy $12,Y
		AbsoluteX, //absx $1234,X
		AbsoluteY, //absy $1234,Y
		IndirectAbsoluteX, //iabsx ($1234,X)
		Implicit, 
		Relative, //rel $1234
		ZeroPageRelative,//zrel $12, $1234
		IndexedIndirect, //indx ($12,X)
		IndirectIndexed, //indy ($12),Y
		IndirectAbsolute, //iabs ($1234)
		Indirect //ind ($12)
	}
}
