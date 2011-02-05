using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;
using System.Diagnostics;

namespace KillerApps.Emulation.Processors
{
	public partial class Nmos6502: ProcessorBase
	{
		// Processor registers
		public byte A { get; set; }				// Accumulator								8 bits
		public byte X { get; set; }				// X index register						8 bits
		public byte Y { get; set; }				// Y index register						8 bits
		public byte SP { get; set; }				// Stack Pointer							8 bits
		
		public byte Opcode { get; set; }		// Instruction opcode					8 bits
		public ushort Operand { get; set; }	// Instructions operand				16 bits
		public ushort PC { get; set; }				// Program Counter						16 bits
		public byte ProcessorStatus
		{
			get
			{
				byte ps = 0x20;
				if (this.N) ps |= 0x80;
				if (this.B) ps |= 0x10;
				if (this.D) ps |= 0x08;
				if (this.I) ps |= 0x04;
				if (this.Z) ps |= 0x02;
				if (this.C) ps |= 0x01;
				if (this.V) ps |= 0x40;
				return ps;
			}
			set
			{
				this.N = (value & 0x80) == 0x80;
				this.V = (value & 0x40) == 0x40;
				this.B = (value & 0x10) == 0x10;
				this.D = (value & 0x08) == 0x08;
				this.I = (value & 0x04) == 0x04;
				this.Z = (value & 0x02) == 0x02;
				this.C = (value & 0x01) == 0x01;
			}
		}

		// Processor flags
		public bool N; // Negative flag for processor status register
		public bool V; // OVerflow flag for processor status register
		public bool B; // Break flag for processor status register
		public bool D; // Decimal flag for processor status register
		public bool I; // Interrupt disable flag for processor status register
		public bool Z; // Zero flag for processor status register
		public bool C; // Carry flag for processor status register

		protected IMemoryAccess16BitBus Memory { get; private set; }
		
		public Nmos6502(IMemoryAccess16BitBus memory)
		{
			Memory = memory;
		}

		// Stack related functions
		internal void PushOnStack(byte m) { Memory.PokeByte((ushort)(0x0100 + SP), m); SP--; SP &= 0xff; }
		internal int PullFromStack() { SP++; SP &= 0xff; return Memory.PeekByte((ushort)(SP + 0x0100)); }

		public override void Execute(int cyclesToExecute)
		{
			// Fetch opcode
			Opcode = Memory.PeekByte(PC);
			Debug.WriteLineIf(true, String.Format("Nmos6502::Execute: PC {0:X4}, Opcode {1:X2} ", PC, Opcode));
			PC++;

			// Decode and execute
			switch (Opcode)
			{
				default:
					Debug.WriteLine(String.Format("Nmos6502::Execute: Unhandled opcode {0:X2}", Opcode));
					break;
			}
		}

		public override void Initialize()
		{
			throw new NotImplementedException();
		}

		public override void Reset()
		{
			throw new NotImplementedException();
		}

		public override object SignalInterrupt(params object[] args)
		{
			throw new NotImplementedException();
		}
	}
}
