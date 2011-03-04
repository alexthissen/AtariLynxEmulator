using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;
using KillerApps.Emulation.Processors;
using System.Diagnostics;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class MikeyChipset : IMemoryAccess<ushort, byte>
	{
		public const int AUDIO_DPRAM_READWRITE_MIN = 5;
		public const int AUDIO_DPRAM_READWRITE_MAX = 20;
		public const int COLORPALETTE_DPRAM_READWRITE = 5;
		public const int AVAILABLEHARDWARE_READWRITE = 5;
		private LynxHandheld device;
		private byte timerInterruptStatusRegister;
		private byte timerInterruptMask = 0;

		public SystemControlBits1 SYSCTL1 { get; set; }
		public ParallelData IODAT { get; set; }
		public byte IODIR { get; set; }

		private static TraceSwitch GeneralSwitch = new TraceSwitch("General", "General trace switch", "Error");

		public MikeyChipset(LynxHandheld lynx)
		{
			this.device = lynx;
		}

		public void Reset()
		{
			// SDONEACK = 0 "(not acked)"
			timerInterruptMask = timerInterruptStatusRegister = 0;
			IODIR = 0; // reset = 0,0.0.0,0,0,0,0
			IODAT = new ParallelData(0x00);
			SYSCTL1 = new SystemControlBits1(0x02); // reset x,x,x,x,x,x,1,0
		}

		public void Update() 
		{
			Debug.WriteLineIf(GeneralSwitch.TraceVerbose, "MikeyChipset::Update");
		}

		public void Poke(ushort address, byte value)
		{
			switch (address)
			{
				case MikeyAddresses.INTRST:
					// "Read is a poll, write will reset the int that corresponds to a set bit."
					value ^= 0xff;
					timerInterruptStatusRegister &= value;
					
				// TODO: set timer for interrupts to current cycle count to trigger update
					// Cpu.NextTimerEvent = Cpu.SystemClock.CycleCount;
					bool activeIrqs = (timerInterruptStatusRegister & timerInterruptMask) != 0;
					device.Cpu.SignalInterrupt(activeIrqs);
					break;

				case MikeyAddresses.INTSET:
					// "Read is a poll, write will set the int that corresponds to a set bit."
					timerInterruptStatusRegister |= value;
					activeIrqs = (timerInterruptStatusRegister & timerInterruptMask) != 0;
					device.Cpu.SignalInterrupt(activeIrqs);
					//Cpu.NextTimerEvent = Master.Cpu.SystemCycleCount;
					break;


				// "Also note that only the lines that are set to input are actually valid for reading."
				// "8 bits I/O direction corresponding to the 8 bits at FD8B 0=input, 1= output"
				case MikeyAddresses.IODIR:
					IODIR = value;
					break;

				// "Mikey Parallel Data(sort of a R/W) 8 bits of general purpose I/O data"
				case MikeyAddresses.IODAT:
					IODAT.ByteData = value;

					Debug.WriteLineIf(((IODIR & 0x08) == 0) & GeneralSwitch.TraceInfo, "MikeyChipset::Poke(IODAT): Rest is not set to output.");
					Debug.WriteLineIf(((IODIR & 0x02) == 0) & GeneralSwitch.TraceInfo, "MikeyChipset::Poke(IODAT): CartAddressData is not set to output.");

					// "One is that it is the data pin for the shifter that holds the cartridge address."
					device.Cartridge.CartAddressData(IODAT.CartAddressData);
					// "The other is that it controls power to the cartridge."
					device.CartridgePowerOn = !IODAT.CartPowerOff;
					// "In its current use, it is the write enable line for writeable elements in the cartridge."
					if ((IODIR & 0x10) == 0x10)
						device.Cartridge.WriteEnabled = IODAT.AudioIn;
					break;

				case MikeyAddresses.SYSCTL1:
					SYSCTL1.ByteData = value;
					if (!SYSCTL1.Power)
					{
						device.Reset();
						// TODO: Enter debug mode id configured
					}
					device.Cartridge.CartAddressStrobe(SYSCTL1.CartAddressStrobe);
					break;

				case MikeyAddresses.SDONEACK:
					// "Write a '00' to SDONEACK, allowing Mikey to respond to sleep commands."

					// "The Suzy Done Acknowledge address must be written to prior to running the sprite engine. 
					// This is required even prior to the first time the sprite engine is activated. 
					// It it is not written to in the appropriate sequences, the CPU will not go to sleep 
					// when so requested. In addition, if some software accidentally allows a Suzy operation to 
					// complete without then following that completion with a write to SDONEACK, the CPU 
					// will not sleep. So if sprites stop working, something may have gone wrong with your 
					// SDONEACK software."

					// TODO: Implement state for Suzy Done acknowledgement if necessary
					break;

				case MikeyAddresses.CPUSLEEP:
					// "A write of '0' to this address will reset the CPU bus request flip fIop.
					// The setting of the flip flop is described in the hardware specification."
					
					// BUG: "Sleep does not work if Suzy does not have the bus."
					// We assume that everyone knows about this bug and behaves accordingly, so 
					// writing zero here must be to give Suzy access to the bus and it will start drawing
					// sprites and will signal when it is done. 
					// This is implemented as a new wakeup time by calculating the number of cycles used
					// and skipping forward in time to that moment. 
					ulong suzyCycles = device.Suzy.PaintSprites();
					device.Cpu.TrySleep(suzyCycles);
					break;

				default:
					Debug.WriteLine("Mikey::Poke: Unknown address specified.");
					break;
			}
		}

		public byte Peek(ushort address)
		{
			switch (address)
			{
				case MikeyAddresses.INTSET:
				case MikeyAddresses.INTRST:
					// "The software reads either the INTSET or INTRST registers (they have duplicate information) ..."
					return timerInterruptStatusRegister;

				case MikeyAddresses.IODIR:
					return this.IODIR;

				// "Note that some lines are used for several functions, please read the spec.
				// Also note that only the lines that are set to input are actually valid for reading."
				case MikeyAddresses.IODAT:
					byte value = IODAT.ByteData;
					return (byte)(value & (IODIR ^ 0xff));

				// Write-only addresses
				case MikeyAddresses.CPUSLEEP:
				case MikeyAddresses.SDONEACK:
					Debug.WriteLine("Mikey::Peek: Write-only address used.");
					break;

				default:
					Debug.WriteLine("Mikey::Peek: Unknown address specified.");
					break;
			}
			return 0xff;
		}
	}
}
