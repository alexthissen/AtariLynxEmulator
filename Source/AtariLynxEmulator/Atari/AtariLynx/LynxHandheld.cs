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
		public IMemoryAccess16BitBus Cartridge { get; private set; }
		public IMemoryAccess16BitBus Ram { get; private set; }
		public IMemoryAccess16BitBus Rom { get; private set; }
		public IMemoryAccess16BitBus MMU { get; private set; }
		public Nmos6502 Cpu { get; private set; }

		public void Update()
		{
			ExecuteCpu(1);
			GenerateInterrupts();
			SynchronizeTime();
		}

		private void SynchronizeTime() { }

		private void ExecuteCpu(int cyclesToExecute)
		{
			Cpu.Execute(cyclesToExecute);
		}

		private void GenerateInterrupts() { }
	}
}
