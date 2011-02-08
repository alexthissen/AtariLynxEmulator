using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;
using KillerApps.Emulation.Processors;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class LynxHandheld
	{
		public IMemoryAccess<ushort, byte> Cartridge { get; private set; }
		public IMemoryAccess<ushort, byte> Ram { get; private set; }
		public IMemoryAccess<ushort, byte> Rom { get; private set; }
		public IMemoryAccess<ushort, byte> MMU { get; private set; }
		public MikeyChipset Mikey { get; set; }
		public SuzyChipset Suzy { get; set; }
		public Nmos6502 Cpu { get; private set; }
		public Clock SystemClock { get; private set; }

		public void Initialize()
		{
			Ram = new Ram64KBMemory();

			// Pass all hardware that have memory access to MMU
			MMU = new MemoryManagementUnit(Rom, Ram, Mikey, Suzy);
		}

		public void Update()
		{
			ExecuteCpu(1);
			GenerateInterrupts();
			SynchronizeTime();
		}

		private void SynchronizeTime() { }

		private void ExecuteCpu(int cyclesToExecute)
		{
			SystemClock.CycleCount += Cpu.Execute(cyclesToExecute);
		}

		private void GenerateInterrupts() { }
	}
}
