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

		// Timing and sleep
		public bool IsAsleep { get; private set; }
		private ulong ScheduledWakeUpTime;
		private Clock SystemClock;
		public void TrySleep(ulong cyclesToSleep)
		{
			// Set time when we need to wake up because Suzy tells us to. 
			// We might get beaten by earlier IRQs from timers
			this.ScheduledWakeUpTime = SystemClock.CycleCount + cyclesToSleep;
			IsAsleep = true;
			Debug.WriteLine(String.Format("Nmos6502::Sleep: Entering sleep till cycle count {0}", ScheduledWakeUpTime));
		}

		// Irq and Mni related
		public bool IsSystemIrqActive { get; private set; }

		public Nmos6502(IMemoryAccess16BitBus memory, Clock clock)
		{
			SystemClock = clock;
			Memory = memory;
		}

		// Stack related functions
		internal void PushOnStack(byte m) { Memory.PokeByte((ushort)(0x0100 + SP), m); SP--; SP &= 0xff; }
		internal byte PullFromStack() { SP++; SP &= 0xff; return Memory.PeekByte((ushort)(SP + 0x0100)); }
		internal byte PeekStack(byte depth) { return Memory.PeekByte((ushort)((SP + 0x0100 - depth) & 0x01ff)); }

		public override ulong Execute(int cyclesToExecute)
		{
			// When Irq is triggered and inerrupts are not disabled, run interrupt sequence
			// "Then, the interrupt signal waits for the end of the current CPU cycle before actually interrupting the CPU."
			if (IsSystemIrqActive)
			{
				// "Regarding CPUSLEEP, even if the CPU has set the Interrupt Disable flag in the 
				// processor status byte, you'll wake up out of sleep."
				// "If an interrupt occurs while the CPU is asleep, it will wake up the CPU."
				IsAsleep = false;
				if (!I) RunInterruptSequence(InterruptType.Irq);
			}

			// When CPU is sleeping there is nothing to do here. 
			// Owner needs to increase cycle count to awake CPU.
			if (IsAsleep) return 0;

			// Fetch opcode
			Opcode = Memory.PeekByte(PC);
			Debug.WriteLineIf(true, String.Format("Nmos6502::Execute: PC {0:X4}, Opcode {1:X2} ", PC, Opcode));
			PC++;

			// Decode and execute
			switch (Opcode)
			{
				case 0x00:
					// Implied addressing
					BRK();
					break;

				case 0x40: // RTI
					// Implied addressing
					RTI();
					break;

				default:
					Debug.WriteLine(String.Format("Nmos6502::Execute: Unhandled opcode {0:X2}", Opcode));
					break;
			}

			// TODO: Implement timing of opcode execution and induced memory reads/writes
			return 0;
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
			bool active = args.Length > 0 ? (bool)args[0] : true;

			// Set flag to indicate a IRQ is being signaled or taken down. 
			// Active IRQs will be picked up at the next update of the CPU.
			IsSystemIrqActive = active;

			// Nothing to report back to the signaler
			return null;
		}

		// "The interrupt sequence takes two clocks for internal operations, two to push the 
		// return address onto the stack, one to push the processor status register, and two more 
		// to get the ISR's beginning address from $FFFE-FFFF (for IRQ) or $FFFA-FFFB (for NMI)
		// -- in that order."
		protected virtual void RunInterruptSequence(InterruptType irqType)
		{
			Debug.WriteLine(String.Format("IRQ sequence run at PC={0}", PC));
		
			// "The interrupt sequence pushes three bytes onto the stack. First is the high byte of 
			// the return address, followed by the low byte, and finally the status byte from 
			// the P processor status register."
			PushOnStack((byte)(PC >> 8));
			PushOnStack((byte)(PC & 0xff));
			PushOnStack((byte)(ProcessorStatus & 0xef));		// Clear B flag on stack

			// "It is important for the programmer to note that the interrupt-disable I flag is set, 
			// and that the decimal D flag is cleared on the 65C02 but not affected on the NMOS 6502"
			I = true; // Stop further interrupts
			D = false; // Clear decimal mode

			// Load Irq vector into program counter
			ushort vector = irqType == InterruptType.Irq ? VectorAddresses.IRQ_VECTOR : VectorAddresses.NMI_VECTOR;
			PC = Memory.PeekWord(vector);

			// Save sleep state as an IRQ has possibly woken up CPU
			// TODO: SystemCPUSleep_Saved = IsAsleep;
			IsAsleep = false;

			// Clear interrupt status line
			IsSystemIrqActive = false;
		}
	}
}