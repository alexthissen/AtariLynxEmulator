using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Atari.Lynx;
using KillerApps.Emulation.Processors;

namespace AtariLynx.Tests
{
	public class LynxDeviceStub : ILynxDevice
	{
		public bool NewVideoFrameAvailable { get; set; }

		public ICartridge Cartridge
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}
		public Mikey Mikey
		{
			get { throw new NotImplementedException(); }
		}
		public Suzy Suzy
		{
			get { throw new NotImplementedException(); }
		}
		public Cmos65SC02 Cpu
		{
			get { throw new NotImplementedException(); }
		}
		public Clock SystemClock
		{
			get { throw new NotImplementedException(); }
		}
		public bool CartridgePowerOn
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}
		public ulong NextTimerEvent
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}
		public RomBootMemory Rom
		{
			get { throw new NotImplementedException(); }
		}
		public void Reset()
		{
			throw new NotImplementedException();
		}

		private Ram64KBMemory ram;

		public LynxDeviceStub(byte[] memory)
		{
			this.ram = new Ram64KBMemory(memory);
		}

		public Ram64KBMemory Ram
		{
			get { return ram; }
		}
	}
}
