using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;
using KillerApps.Emulation.Processors;
using System.Diagnostics;
using System.IO;

namespace KillerApps.Emulation.Atari.Lynx
{
	public interface ILynxDevice: IResetable
	{
		RomCart Cartridge { get; set; }
		MikeyChipset Mikey { get; }
		SuzyChipset Suzy { get; }
		Cmos65SC02 Cpu { get; }
		Clock SystemClock { get; }
		bool CartridgePowerOn { get; set; }
		ulong NextTimerEvent { get; set; }
	}

	public class LynxHandheld: ILynxDevice
	{
		public RomCart Cartridge { get; set; }
		public Ram64KBMemory Ram { get; private set; }
		public RomBootMemory Rom { get; private set; }
		internal MemoryManagementUnit Mmu { get; private set; }
		public MikeyChipset Mikey { get; private set; }
		public SuzyChipset Suzy { get; private set; }
		public Cmos65SC02 Cpu { get; private set; }
		public Clock SystemClock { get; private set; }

		public Stream BootRomImage { get; set; }
		public Stream CartRomImage { get; set; }

		public bool CartridgePowerOn { get; set; }
		public ulong NextTimerEvent { get; set; }

		private static TraceSwitch GeneralSwitch = new TraceSwitch("General", "General trace switch", "Error");

		public void Initialize()
		{
			Ram = new Ram64KBMemory();
			Rom = new RomBootMemory();
			Rom.LoadBootImage(BootRomImage);
			
			Mikey = new MikeyChipset(this);
			Suzy = new SuzyChipset(this);

			// Pass all hardware that have memory access to MMU
			Mmu = new MemoryManagementUnit(Rom, Ram, Mikey, Suzy);
			SystemClock = new Clock();
			
			// Finally construct processor
			Cpu = new Cmos65SC02(Mmu, SystemClock);

			Reset();
		}

		public void Reset()
		{
			Mikey.Reset();
			Suzy.Reset();
			Mmu.Reset();
			Cpu.Reset();
		}

		public void Update()
		{
			GenerateInterrupts();
			ExecuteCpu(1);
			SynchronizeTime();
		}

		private void ExecuteCpu(int cyclesToExecute)
		{
			Cpu.Execute(cyclesToExecute);
		}

		private void GenerateInterrupts() 
		{
			// Mikey is only source of interrupts. It contains all timers (regular, audio and UART)
			if (SystemClock.CompatibleCycleCount >= NextTimerEvent) 
				Mikey.Update();
			Debug.WriteLineIf(GeneralSwitch.TraceVerbose, "LynxHandheld::GenerateInterrupts");
		}

		private void SynchronizeTime()
		{
			Debug.WriteLineIf(GeneralSwitch.TraceVerbose, String.Format("LynxHandheld::SynchronizeTime: Current time is {0}", SystemClock.CompatibleCycleCount));
		}
	}
}
