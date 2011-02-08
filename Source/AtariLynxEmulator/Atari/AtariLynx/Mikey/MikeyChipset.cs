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

		public MikeyChipset(LynxHandheld lynx)
		{
			this.device = lynx;
		}

		public void Reset()
		{
			// SDONEACK = 0 "(not acked)"
			timerInterruptMask = timerInterruptStatusRegister = 0;
		}

		public void Update() { }

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

				case MikeyAddresses.SDONEACK:
					// "Write a '00' to SDONEACK, allowing Mikey to respond to sleep commands."

					// "The Suzy Done Acknowledge address must be written to prior to running the sprite engine. 
					// This is required even prior to the first time the sprite engine is activated. 
					// It it is not written to in the appropriate sequences, the CPU will not go to sleep 
					// when so requested. In addition, if some software accidentally allows a Suzy operation to 
					// complete without then following that completion with a write to SDONEACK, the CPU 
					// will not sleep. So if sprites stop working, something may have gone wrong with your 
					// SDONEACK software."

					// TODO: Implement state for Suzy Done acknowledgement
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
					Debug.WriteLine("Mikey::PokeByte: Unknown address specified.");
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

				// Write-only addresses
				case MikeyAddresses.CPUSLEEP:
				case MikeyAddresses.SDONEACK:
					Debug.WriteLine("Mikey::PeekByte: Write-only address used.");
					break;

				default:
					Debug.WriteLine("Mikey::PeekByte: Unknown address specified.");
					break;
			}
			return 0xff;
		}
	}
}
