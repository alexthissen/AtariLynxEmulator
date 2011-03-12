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
		
		private ILynxDevice device;
		private byte timerInterruptStatusRegister;
		private byte timerInterruptMask = 0;

		public byte[] RedColorMap = new byte[0x10];
		public byte[] GreenColorMap = new byte[0x10];
		public byte[] BlueColorMap = new byte[0x10];
		public SystemControlBits1 SYSCTL1 { get; set; }
		public ParallelData IODAT { get; set; }
		public DisplayControlBits DISPCTL { get; set; }
		public byte IODIR { get; set; }
		public byte PBKUP { get; set; }
		public Word VideoDisplayStartAddress;
					
		// Timers
		public Timer[] Timers = new Timer[8];
		
		private static TraceSwitch GeneralSwitch = new TraceSwitch("General", "General trace switch", "Error");

		private const byte Timer0Mask = 0x01; // "B0 = timer 0 (horizontal line timer)"
		private const byte Timer1Mask = 0x02; // "B1 = timer 1"
		private const byte Timer2Mask = 0x04; // "B2 = timer 2 (vertical line counter)"
		private const byte Timer3Mask = 0x08; // "B3 = timer 3"
		private const byte Timer4Mask = 0x10; // "B4 = serial interrupt"
		private const byte Timer5Mask = 0x20; // "B5 = timer 5"
		private const byte Timer6Mask = 0x40; // "B6 = timer 6"
		private const byte Timer7Mask = 0x80; // "B7 = timer 7"

		public MikeyChipset(ILynxDevice lynx)
		{
			this.device = lynx;
		}

		public void Initialize()
		{
			InitializeTimers();
		}

		private void InitializeTimers()
		{
			// Create all timers
			for (int index = 0; index < 8; index++)
			{
				byte interruptMask = (byte)(1 << index);
				Timers[index] = new Timer(interruptMask);

				// Hook up linked timer to previous timer in group B
				if (index > 1) Timers[index].PreviousTimer = Timers[index - 2];
				// TODO: Hook up timer 1 to audio 3 when linked

				Timers[index].Expired += new EventHandler<TimerExpirationEventArgs>(TimerExpired);
			}

			// Timer 6 is not part of a group and does not have a timer linked to it
			Timers[5].PreviousTimer = null;

			// "Two of the timers will be used for the video frame rate generator."
			Timers[0].Expired += new EventHandler<TimerExpirationEventArgs>(DisplayRenderLine);
			// "... the second (timer 2) is set to the number of lines."
			Timers[2].Expired += new EventHandler<TimerExpirationEventArgs>(DisplayEndOfFrame);
			// "One of the timers (timer 4) will be used as the baud rate generator for the serial expansion port (UART)."
			Timers[4].Expired += new EventHandler<TimerExpirationEventArgs>(GenerateBaudRate);
		}

		private void TimerExpired(object sender, TimerExpirationEventArgs e)
		{
			Timer timer = (Timer)sender;
			
			// Only timers with enabled interrupt will trigger
			if (timer.StaticControlBits.EnableInterrupt)
			{
				// Update interrupt status
				timerInterruptStatusRegister |= e.InterruptMask;

				// Trigger a maskable interrupt
				device.Cpu.SignalInterrupt(InterruptType.Irq);
			}
		}

		private void GenerateBaudRate(object sender, TimerExpirationEventArgs e) { }
		private void DisplayEndOfFrame(object sender, TimerExpirationEventArgs e) { }
		private void DisplayRenderLine(object sender, TimerExpirationEventArgs e) 
		{
 			// TODO: Implementation of Keith does not always trigger IRQ when DMA is not enabled, display bits (?) or display current have not been set
		}

		public void Reset()
		{
			Initialize();

			// SDONEACK = 0 "(not acked)"
			timerInterruptMask = timerInterruptStatusRegister = 0;
			IODIR = 0; // reset = 0,0.0.0,0,0,0,0
			IODAT = new ParallelData(0x00);
			SYSCTL1 = new SystemControlBits1(0x02); // reset x,x,x,x,x,x,1,0
			DISPCTL = new DisplayControlBits(0x00);	// reset = 0
		}

		private void ForceTimerUpdate()
		{
			//device.NextTimerEvent = device.SystemClock.CycleCount;
			device.NextTimerEvent = device.SystemClock.CompatibleCycleCount;
		}

		public void Update() 
		{
			Debug.WriteLineIf(GeneralSwitch.TraceVerbose, "MikeyChipset::Update");
			foreach (Timer timer in Timers)
			{
				timer.Update(device.SystemClock.CompatibleCycleCount);
			}

			// Take lowest 
			device.NextTimerEvent = ulong.MaxValue;
			var clocks = Timers.Where(t => t.StaticControlBits.EnableCount);
			if (clocks.Count() > 0) device.NextTimerEvent = clocks.Min(t => t.ExpirationTime);
		}

		public void Poke(ushort address, byte value)
		{
			// Timer addresses can be handled separately
			if (address >= MikeyAddresses.HTIMBKUP && address <= MikeyAddresses.TIM7CTLB)
			{
				int offset = address - MikeyAddresses.HTIMBKUP;
				int index = offset >> 2; // Divide by 4 to get index of timer
				Timer timer = Timers[index];

				switch (offset % 4)
				{
					case 0: // Backup value
						timer.BackupValue = value;
						return;

					case 1: // Static control
						StaticTimerControl control = new StaticTimerControl(value);
						timer.StaticControlBits = control;

						// "It is set on time out, reset with the reset timer done bit (xxx1, B6)"
						if (control.ResetTimerDone)
						{
							timer.DynamicControlBits.TimerDone = false;
						}
						if (control.EnableCount || control.ResetTimerDone)
						{
							timer.Start(device.SystemClock.CompatibleCycleCount);
							ForceTimerUpdate();
						}
						return;

					case 2: // Current value
						timer.CurrentValue = value;
						ForceTimerUpdate();
						return;

					case 3: // Dynamic control bits
						timer.DynamicControlBits.ByteData = value;
						return;
				}
			}

			// Handle other addresses
			switch (address)
			{
				case MikeyAddresses.INTRST:
					// "Read is a poll, write will reset the int that corresponds to a set bit."
					value ^= 0xff;
					timerInterruptStatusRegister &= value;
					
					// TODO: Keith Wilkins has fix below for Championship Rally here. Is it still necessary?
					//device.Cpu.SignalInterrupt(...)
					//ForceTimerUpdate();
					return;

				case MikeyAddresses.INTSET:
					// "Read is a poll, write will set the int that corresponds to a set bit."
					timerInterruptStatusRegister |= value;
				
					// TODO: Keith Wilkins has fix for Championship Rally here. Is it still necessary?
					//device.Cpu.SignalInterrupt(...)
					//ForceTimerUpdate();
					return;

				case MikeyAddresses.MIKEYSREV:
					// "No actual register is implemented"
					return;

				// "Also note that only the lines that are set to input are actually valid for reading."
				// "8 bits I/O direction corresponding to the 8 bits at FD8B 0=input, 1= output"
				case MikeyAddresses.IODIR:
					IODIR = value;
					return;

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
					return;

				case MikeyAddresses.SYSCTL1:
					SYSCTL1.ByteData = value;
					if (!SYSCTL1.Power)
					{
						device.Reset();
						// TODO: Enter debug mode id configured
					}
					device.Cartridge.CartAddressStrobe(SYSCTL1.CartAddressStrobe);
					return;

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
					return;

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
					return;
					
				case MikeyAddresses.DISPCTL:
					DISPCTL.ByteData = value;
					return;

				case MikeyAddresses.PBKUP:
					// "Additionally, the magic 'P' counter has to be set to match the LCD scan rate. The formula is:
					// INT((((line time - .5us) / 15) * 4) -1)"
					PBKUP = value;
					return;

				case MikeyAddresses.DISPADRL:
					// "DISPADRL (FD94) is lower 8 bits of display address with the bottom 2 bit ignored by the hardware.
					// The address of the upper left corner of a display buffer must always have '00' in the bottom 2 bits."
					VideoDisplayStartAddress.LowByte = (byte)(value & 0xFD);
					return;

				case MikeyAddresses.DISPADRH:
					// "DISPADRH (FD95) is upper 8 bits of display address."
					VideoDisplayStartAddress.HighByte = value;
					return;

				case MikeyAddresses.MAGRDY0:
				case MikeyAddresses.MAGRDY1:
				case MikeyAddresses.AUDIN:
				case MikeyAddresses.MIKEYHREV:
					Debug.WriteLineIf(GeneralSwitch.TraceWarning, String.Format("Mikey::Poke - Read-only address {0:X4} used (value {1:X2}).", address, value));
					break;

				default:
					break;
			}

			// Blue and red color map
			if (address >= MikeyAddresses.BLUERED0)
			{
				int index = address - MikeyAddresses.BLUERED0;
				BlueColorMap[index] = (byte)((value & 0xF0) >> 4);
				RedColorMap[index] = (byte)(value & 0x0F);
				return;
			}

			// Green color map
			if (address >= MikeyAddresses.GREEN0)
			{
				int index = address - MikeyAddresses.GREEN0;
				GreenColorMap[index] = (byte)(value & 0x0F);
				return;
			}

			Debug.WriteLineIf(GeneralSwitch.TraceWarning, String.Format("Mikey::Poke: Unknown address {0:X$} specified (value {1:X2}).", address,value));
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

				case MikeyAddresses.PBKUP:
					return PBKUP;

				case MikeyAddresses.MIKEYHREV:
					// "No actual register is implemented"
					return 0x01;

				case MikeyAddresses.MIKEYSREV:
					// "No actual register is implemented"
					break;

				// Write-only addresses
				case MikeyAddresses.CPUSLEEP:
				case MikeyAddresses.SDONEACK:
				case MikeyAddresses.DISPADRL:
				case MikeyAddresses.DISPADRH:
				case MikeyAddresses.SYSCTL1:
				case MikeyAddresses.DISPCTL:
					Debug.WriteLineIf(GeneralSwitch.TraceWarning, String.Format("Mikey::Peek - Write-only address {0:X4} used.", address));
					return 0xFF;

				default:
					break;
			}

			if (address >= MikeyAddresses.BLUERED0)
			{
				int index = address - MikeyAddresses.BLUERED0;
				return (byte)((BlueColorMap[index] << 4) + RedColorMap[index]);
			}

			if (address >= MikeyAddresses.GREEN0)
			{
				int index = address - MikeyAddresses.GREEN0;
				return GreenColorMap[index];
			}

			Debug.WriteLineIf(GeneralSwitch.TraceWarning, String.Format("Mikey::Peek -  Unknown address {0:X4} specified.", address));
			return 0xff;
		}
	}
}
