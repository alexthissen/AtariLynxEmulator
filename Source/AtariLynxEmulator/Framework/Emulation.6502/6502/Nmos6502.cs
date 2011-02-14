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
		public byte A { get; set; }	// Accumulator (8 bits)
		public byte X { get; set; }	// X index register (8 bits)
		public byte Y { get; set; }	// Y index register	(8 bits)
		public byte SP { get; set; } // Stack Pointer	(8 bits)
		
		public byte Opcode { get; set; } // Instruction opcode (8 bits)
		public ushort Operand { get; set; }	// Instructions operand	(16 bits)
		public ushort PC { get; set; } // Program Counter (16 bits)
		public byte ProcessorStatus
		{
			get
			{
				byte ps = 0x20;
				if (this.N) ps |= 0x80;
				if (this.V) ps |= 0x40;
				if (this.B) ps |= 0x10;
				if (this.D) ps |= 0x08;
				if (this.I) ps |= 0x04;
				if (this.Z) ps |= 0x02;
				if (this.C) ps |= 0x01;
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

		protected IMemoryAccess<ushort, byte> Memory { get; private set; }

		// Timing and sleep
		public bool IsAsleep { get; protected set; }
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

		public Nmos6502(IMemoryAccess<ushort, byte> memory, Clock clock)
		{
			SystemClock = clock;
			Memory = memory;
		}

		// Stack related functions
		internal void PushOnStack(byte m) { Memory.Poke((ushort)(0x0100 + SP), m); SP--; SP &= 0xff; }
		internal byte PullFromStack() { SP++; SP &= 0xff; return Memory.Peek((ushort)(SP + 0x0100)); }
		internal byte PeekStack(byte depth) { return Memory.Peek((ushort)((SP + 0x0100 - depth) & 0x01ff)); }

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
			Opcode = Memory.Peek(PC);
			Debug.WriteLineIf(true, String.Format("Nmos6502::Execute: PC {0:X4}, Opcode {1:X2} ", PC, Opcode));
			PC++;

			ExecuteOpcode();

			// TODO: Implement timing of opcode execution and induced memory reads/writes
			return 0;
		}

		protected virtual void ExecuteOpcode()
		{
			// Decode and execute
			switch (Opcode)
			{
				case 0x00: BRK(); break;
				case 0x01: ZeroPageIndexedIndirectX(); ORA(); break;
				case 0x05: ZeroPage(); ORA(); break;
				case 0x06: ZeroPage(); ASL(); break;
				case 0x08: PHP(); break;
				case 0x09: Immediate(); ORA(); break;
				case 0x0A: ASLA(); break;
				case 0x0D: Absolute(); ORA(); break;
				case 0x0E: Absolute(); ASL(); break;

				case 0x10: BPL(); break;
				case 0x11: ZeroPageIndirectIndexedY(); ORA(); break;
				case 0x15: ZeroPageX(); ORA(); break;
				case 0x16: ZeroPageX(); ASL(); break;
				case 0x18: CLC(); break;
				case 0x19: AbsoluteY(); ORA(); break;
				case 0x1D: AbsoluteX(); ORA(); break;
				case 0x1E: AbsoluteX(); ASL(); break;

				case 0x20: Absolute(); JSR(); break;
				case 0x21: ZeroPageIndexedIndirectX(); AND(); break;
				case 0x24: ZeroPage(); BIT(); break;
				case 0x25: ZeroPage(); AND(); break;
				case 0x26: ZeroPage(); ROL(); break;
				case 0x28: PLP(); break;
				case 0x29: Immediate(); AND(); break;
				case 0x2A: ROLA(); break;
				case 0x2C: Absolute(); BIT(); break;
				case 0x2D: Absolute(); AND(); break;
				case 0x2E: Absolute(); ROL(); break;

				case 0x30: BMI(); break;
				case 0x31: ZeroPageIndirectIndexedY(); AND(); break;
				case 0x35: ZeroPageX(); AND(); break;
				case 0x36: ZeroPageX(); ROL(); break;
				case 0x38: SEC(); break;
				case 0x39: AbsoluteY(); AND(); break;
				case 0x3D: AbsoluteX(); AND(); break;
				case 0x3E: AbsoluteX(); ROL(); break;

				case 0x40: RTI(); break;
				case 0x41: ZeroPageIndexedIndirectX(); EOR(); break;
				case 0x45: ZeroPage(); EOR(); break;
				case 0x46: ZeroPage(); LSR(); break;
				case 0x48: PHA(); break;
				case 0x49: Immediate(); EOR(); break;
				case 0x4A: LSRA(); break;
				case 0x4C: Absolute(); JMP(); break;
				case 0x4D: Absolute(); EOR(); break;
				case 0x4E: Absolute(); LSR(); break;

				case 0x50: BVC(); break;
				case 0x51: ZeroPageIndirectIndexedY(); EOR(); break;
				case 0x55: ZeroPageX(); EOR(); break;
				case 0x56: ZeroPageX(); LSR(); break;
				case 0x58: CLI(); break;
				case 0x59: AbsoluteY(); EOR(); break;
				case 0x5D: AbsoluteX(); EOR(); break;
				case 0x5E: AbsoluteX(); LSR(); break;

				case 0x60: RTS(); break;
				case 0x61: ZeroPageIndexedIndirectX(); ADC(); break;
				case 0x65: ZeroPage(); ADC(); break;
				case 0x66: ZeroPage(); ROR(); break;
				case 0x68: PLA(); break;
				case 0x69: Immediate(); ADC(); break;
				case 0x6A: RORA(); break;
				// Bug in Indirect absolute addressing
				case 0x6C: AbsoluteIndirectWithBug(); JMP(); break;
				case 0x6D: Absolute(); ADC(); break;
				case 0x6E: Absolute(); ROR(); break;

				case 0x70: BVS(); break;
				case 0x71: ZeroPageIndirectIndexedY(); ADC(); break;
				case 0x75: ZeroPageX(); ADC(); break;
				case 0x76: ZeroPageX(); ROR(); break;
				case 0x78: SEI(); break;
				case 0x79: AbsoluteY(); ADC(); break;
				case 0x7D: AbsoluteX(); ADC(); break;
				case 0x7E: AbsoluteX(); ROR(); break;

				case 0x81: ZeroPageIndexedIndirectX(); STA(); break;
				case 0x84: ZeroPage(); STY(); break;
				case 0x85: ZeroPage(); STA(); break;
				case 0x86: ZeroPage(); STX(); break;
				case 0x88: DEY(); break;
				case 0x8A: TXA(); break;
				case 0x8C: Absolute(); STY(); break;
				case 0x8D: Absolute(); STA(); break;
				case 0x8E: Absolute(); STX(); break;

				case 0x90: BCC(); break;
				case 0x91: ZeroPageIndirectIndexedY(); STA(); break;
				case 0x94: ZeroPageX(); STY(); break;
				case 0x95: ZeroPageX(); STA(); break;
				case 0x96: ZeroPageY(); STX(); break;
				case 0x98: TYA(); break;
				case 0x99: AbsoluteY(); STA(); break;
				case 0x9A: TXS(); break;
				case 0x9D: AbsoluteX(); STA(); break;

				case 0xA0: Immediate(); LDY(); break;
				case 0xA1: ZeroPageIndexedIndirectX(); LDA(); break;
				case 0xA2: Immediate(); LDX(); break;
				case 0xA4: ZeroPage(); LDY(); break;
				case 0xA5: ZeroPage(); LDA(); break;
				case 0xA6: ZeroPage(); LDX(); break;
				case 0xA8: TAY(); break;
				case 0xA9: Immediate(); LDA(); break;
				case 0xAA: TAX(); break;
				case 0xAC: Absolute(); LDY(); break;
				case 0xAD: Absolute(); LDA(); break;
				case 0xAE: Absolute(); LDX(); break;

				case 0xB0: BCS(); break;
				case 0xB1: ZeroPageIndirectIndexedY(); LDA(); break;
				case 0xB4: ZeroPageX(); LDY(); break;
				case 0xB5: ZeroPageX(); LDA(); break;
				case 0xB6: ZeroPageY(); LDX(); break;
				case 0xB8: CLV(); break;
				case 0xB9: AbsoluteY(); LDA(); break;
				case 0xBA: TSX(); break;
				case 0xBC: AbsoluteX(); LDY(); break;
				case 0xBD: AbsoluteX(); LDA(); break;
				case 0xBE: AbsoluteY(); LDX(); break;

				case 0xC0: Immediate(); CPY(); break;
				case 0xC1: ZeroPageIndexedIndirectX(); CMP(); break;
				case 0xC4: ZeroPage(); CPY(); break;
				case 0xC5: ZeroPage(); CMP(); break;
				case 0xC6: ZeroPage(); DEC(); break;
				case 0xC8: INY(); break;
				case 0xC9: Immediate(); CMP(); break;
				case 0xCA: DEX(); break;
				case 0xCC: Absolute(); CPY(); break;
				case 0xCD: Absolute(); CMP(); break;
				case 0xCE: Absolute(); DEC(); break;

				case 0xD0: BNE(); break;
				case 0xD1: ZeroPageIndirectIndexedY(); CMP(); break;
				case 0xD5: ZeroPageX(); CMP(); break;
				case 0xD6: ZeroPageX(); DEC(); break;
				case 0xD8: CLD(); break;
				case 0xD9: AbsoluteY(); CMP(); break;
				case 0xDD: AbsoluteX(); CMP(); break;
				case 0xDE: AbsoluteX(); DEC(); break;

				case 0xE0: Immediate(); CPX(); break;
				case 0xE1: ZeroPageIndexedIndirectX(); SBC(); break;
				case 0xE4: ZeroPage(); CPX(); break;
				case 0xE5: ZeroPage(); SBC(); break;
				case 0xE6: ZeroPage(); INC(); break;
				case 0xE8: INX(); break;
				case 0xE9: Immediate(); SBC(); break;
				case 0xEA: NOP(); break;
				case 0xEC: Absolute(); CPX(); break;
				case 0xED: Absolute(); SBC(); break;
				case 0xEE: Absolute(); INC(); break;

				case 0xF0: BEQ(); break;
				case 0xF1: ZeroPageIndirectIndexedY(); SBC(); break;
				case 0xF5: ZeroPageX(); SBC(); break;
				case 0xF6: ZeroPageX(); INC(); break;
				case 0xF8: SED(); break;
				case 0xF9: AbsoluteY(); SBC(); break;
				case 0xFD: AbsoluteX(); SBC(); break;
				case 0xFE: AbsoluteX(); INC(); break;

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
			Debug.WriteLine("Nmos6502::Reset()");

			A = 0;
			X = 0;
			Y = 0;
			SP = 0xff;
			Opcode = 0;
			Operand = 0;

			// After reset program counter will be set to WORD value at boot vector
			PC = Memory.PeekWord(VectorAddresses.BOOT_VECTOR);

			N = V = B = D = I = Z = C = false;
			V = false;
			B = false;
			D = false;
			I = true;
			Z = true;
			C = false;

			IsSystemIrqActive = false;
			IsAsleep = false;
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

			UpdateInterruptFlags();

			// Load IRQ vector into program counter
			ushort vector = irqType == InterruptType.Irq ? VectorAddresses.IRQ_VECTOR : VectorAddresses.NMI_VECTOR;
			PC = Memory.PeekWord(vector);

			// Clear interrupt status line
			IsSystemIrqActive = false;
		}

		protected virtual void UpdateInterruptFlags()
		{
			// "It is important for the programmer to note that the interrupt-disable I flag is set, 
			// and that the decimal D flag is cleared on the 65C02 but not affected on the NMOS 6502"
			I = true; // Stop further interrupts
		}
	}
}