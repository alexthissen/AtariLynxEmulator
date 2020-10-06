using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Processors;
using System.Diagnostics;
using System.IO;

namespace KillerApps.Emulation.AtariLynx
{
	public class LynxHandheld: ILynxDevice
	{
		public ICartridge Cartridge { get; private set; }
		public Ram64KBMemory Ram { get; private set; }
		public RomBootMemory Rom { get; private set; }
		internal MemoryManagementUnit Mmu { get; private set; }
		public Mikey Mikey { get; private set; }
		public Suzy Suzy { get; private set; }
		public Cmos65SC02 Cpu { get; private set; }
		public Clock SystemClock { get; private set; }
		public int[] LcdScreenDma;

		public Stream BootRomImage { get; set; }
		public Stream CartRomImage { get; set; }

		public bool CartridgePowerOn { get; set; }
		public ulong NextTimerEvent { get; set; }
		public bool NewVideoFrameAvailable { get; set; }

		public const int SYSTEM_FREQ = 16000000;

		//private static TraceSwitch GeneralSwitch = new TraceSwitch("General", "General trace switch", "Error");

		public void InsertCartridge(ICartridge cartridge)
		{
			this.Cartridge = cartridge;
		}

		public void Initialize()
		{
			Ram = new Ram64KBMemory();
			Rom = new RomBootMemory();
			Rom.LoadBootImage(BootRomImage);
			
			Mikey = new Mikey(this);
			Suzy = new Suzy(this);
			Suzy.Initialize();

			// Pass all hardware that have memory access to MMU
			Mmu = new MemoryManagementUnit(Rom, Ram, Mikey, Suzy);
			SystemClock = new Clock();
			LcdScreenDma = new int[0x3FC0]; // 160 * 102 pixels

			// Construct processor
			Cpu = new Cmos65SC02(Mmu, SystemClock);

			Mikey.Initialize();

			// Initialization means running reset operation
			Reset();
		}

		public void Reset()
		{
			Mikey.Reset();
			Suzy.Reset();
			Mmu.Reset();
			Cpu.Reset();
		}

		public void Update(ulong cyclesToExecute)
		{
			ulong executedCycles = 0;

			while (cyclesToExecute > executedCycles)
			{
				GenerateInterrupts();
				executedCycles += ExecuteCpu();
				SynchronizeTime();
			}
		}

		private ulong ExecuteCpu()
		{
			ulong executedCycles = Cpu.Execute(1);

			if (Cpu.IsAsleep)
			{
				executedCycles += NextTimerEvent - SystemClock.CompatibleCycleCount;
				// Added this line
				if (NextTimerEvent > Cpu.ScheduledWakeUpTime) NextTimerEvent = Cpu.ScheduledWakeUpTime;
				SystemClock.CompatibleCycleCount = NextTimerEvent;
			}
			return executedCycles;
		}

		private void GenerateInterrupts() 
		{
			// Mikey is only source of interrupts. It contains all timers (regular, audio and UART)
			if (SystemClock.CompatibleCycleCount >= NextTimerEvent) 
				Mikey.Update();
		}

		private void SynchronizeTime()
		{
			if (Suzy.SPRSYS.MathInProcess && SystemClock.CompatibleCycleCount >= this.Suzy.MathReadyTime)
			{
				this.Suzy.EndMathOperation();
			}
			//Debug.WriteLineIf(GeneralSwitch.TraceVerbose, String.Format("LynxHandheld::SynchronizeTime: Current time is {0}", SystemClock.CompatibleCycleCount));
		}
		
		public void UpdateJoystickState(JoystickStates state)
		{
			this.Suzy.JOYSTICK.State = state;
		}

		public void InsertComLynxCable(IComLynxTransport transport)
		{
			this.Mikey.ComLynx.InsertCable(transport);
		}
	}
}
