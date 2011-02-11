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
	public class LynxHandheld
	{
		public RomCart Cartridge { get; private set; }
		public IMemoryAccess<ushort, byte> Ram { get; private set; }
		public IMemoryAccess<ushort, byte> Rom { get; private set; }
		public IMemoryAccess<ushort, byte> MMU { get; private set; }
		public MikeyChipset Mikey { get; set; }
		public SuzyChipset Suzy { get; set; }
		public Nmos6502 Cpu { get; private set; }
		public Clock SystemClock { get; private set; }

		public bool CartridgePowerOn { get; set; }

		public void Initialize()
		{
			Ram = new Ram64KBMemory();
			// TODO: Load rom boot image into stream and pass to RomBootMemory ctor
			Rom = new RomBootMemory(null);

			// Pass all hardware that have memory access to MMU
			MMU = new MemoryManagementUnit(Rom, Ram, Mikey, Suzy);
		}

		public void Update()
		{
			ExecuteCpu(1);
			GenerateInterrupts();
			SynchronizeTime();
		}

		private void SynchronizeTime() 
		{
			Debug.WriteLineIf(true, String.Format("LynxHandheld::SynchronizeTime: Current time is {0}", SystemClock.CycleCount));
		}

		private void ExecuteCpu(int cyclesToExecute)
		{
			SystemClock.CycleCount += Cpu.Execute(cyclesToExecute);
		}

		private void GenerateInterrupts() 
		{
			// Mikey is only source of interrupts. It contains all timers (regular, audio and UART)
			// TODO: After implementing timing, only update Mikey when there is a predicted timer event
			Mikey.Update();
		}
	}
}
