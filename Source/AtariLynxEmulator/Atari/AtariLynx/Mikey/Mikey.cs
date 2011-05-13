using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;
using KillerApps.Emulation.Processors;
using System.Diagnostics;

namespace KillerApps.Emulation.Atari.Lynx
{
	public partial class Mikey : IMemoryAccess<ushort, byte>
	{
		public const int AUDIO_DPRAM_READWRITE_MIN = 5;
		public const int AUDIO_DPRAM_READWRITE_MAX = 20;
		public const int COLORPALETTE_DPRAM_READWRITE = 5;
		public const int AVAILABLEHARDWARE_READWRITE = 5;
		
		private ILynxDevice device;
		private byte timerInterruptStatusRegister;
		private byte timerInterruptMask = 0;
		private int lineAddress { get; set; }

		public byte[] RedColorMap = new byte[0x10];
		public byte[] GreenColorMap = new byte[0x10];
		public byte[] BlueColorMap = new byte[0x10];
		public int[] ArgbColorMap = new int[0x10];

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
		
		private ushort currentLynxDmaAddress;
		private int currentLcdDmaCounter;
		private byte[] LcdScreenDma;
		private byte[] VideoMemoryDma;
		private byte currentLine;
		
		public Mikey(ILynxDevice lynx)
		{
			this.device = lynx;
		}

		public void Initialize()
		{
			InitializeTimers();
			// TODO: Clean this hack up and use a decent way to get at the LCD screen memory
			LcdScreenDma = ((LynxHandheld)device).LcdScreenDma;
			// TODO: Another hack to avoid rendering when timer 2 has only just started
			currentLcdDmaCounter = -1;
			VideoMemoryDma = device.Ram.GetDirectAccess();
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
			Timers[0].Expired += new EventHandler<TimerExpirationEventArgs>(RenderLine);
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
				Debug.WriteLineIf(GeneralSwitch.TraceInfo, String.Format("Mikie::Update() - Timer IRQ Triggered at {0:X8}", device.SystemClock.CompatibleCycleCount));
				device.Cpu.SignalInterrupt(InterruptType.Irq);
			}
		}

		private void GenerateBaudRate(object sender, TimerExpirationEventArgs e) { }
		
		private void DisplayEndOfFrame(object sender, TimerExpirationEventArgs e) 
		{
			// Pick up current video start address
			currentLynxDmaAddress = (ushort)(VideoDisplayStartAddress.Value & 0xFFFC);
			if (DISPCTL.Flip)
			{
				currentLynxDmaAddress += 3;
			}

			currentLcdDmaCounter = 0;
			currentLine = Timers[2].BackupValue;
		}
		
		private void RenderLine(object sender, TimerExpirationEventArgs e) 
		{
			// TODO: Implementation of Keith does not always trigger IRQ when DMA is not enabled, display bits (?) or display current have not been set
			if (!DISPCTL.EnableVideoDma) return;

			int backupValue = Timers[2].BackupValue;
			//currentLine = Timers[2].CurrentValue;
			
			// "The current method of driving the LCD requires 3 scan lines of vertical blank."
			// TODO: Determine if rest is between frames 104, 103 and 102 (for 60Hz)
			// Keith Wilkins has Rest period between 102, 101 and 100
			bool rest = (currentLine >= (backupValue - 4) && currentLine <= (backupValue - 2));
			IODAT.Rest = rest;

			if (currentLine > (backupValue - 3))
			{
				currentLine--;
				return;
			}

			if (currentLine > 0) currentLine--;
			if (currentLcdDmaCounter < 0) return;

			// TODO: Define constant for DMA_READWRITE_CYCLE
			device.SystemClock.CompatibleCycleCount += 80 * 4;
		
			// Every byte has two nibbles for two pixels
			for (int loop = 0; loop < Suzy.SCREEN_WIDTH / 2; loop++)
			{
				byte source = VideoMemoryDma[currentLynxDmaAddress];
				if (DISPCTL.Flip)
				{
					currentLynxDmaAddress--;
					SetPixel((byte)(source & 0x0F));
					SetPixel((byte)(source >> 4));
				}
				else
				{
					currentLynxDmaAddress++;
					SetPixel((byte)(source >> 4));
					SetPixel((byte)(source & 0x0F));
				}
			}
		}

		private void SetPixel(byte source)
		{
			LcdScreenDma[currentLcdDmaCounter++] = RedColorMap[source];
			LcdScreenDma[currentLcdDmaCounter++] = GreenColorMap[source];
			LcdScreenDma[currentLcdDmaCounter++] = BlueColorMap[source];
			LcdScreenDma[currentLcdDmaCounter++] = 0xFF;
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
			Debug.WriteLineIf(GeneralSwitch.TraceVerbose, "Mikey::Update");
			foreach (Timer timer in Timers)
			{
				timer.Update(device.SystemClock.CompatibleCycleCount);
			}

			// Take lowest timer 
			device.NextTimerEvent = ulong.MaxValue;
			var clocks = Timers.Where(t => t.StaticControlBits.EnableCount);
			if (clocks.Count() > 0) device.NextTimerEvent = clocks.Min(t => t.ExpirationTime);

			if (device.Cpu.IsAsleep)
			{
				// Make wakeup time next timer event if earlier than first timer
				if (device.NextTimerEvent > device.Cpu.ScheduledWakeUpTime) device.NextTimerEvent = device.Cpu.ScheduledWakeUpTime;
			}
		}

		public void Poke(ushort address, byte value)
		{
			// Timer addresses can be handled separately
			if (address >= Mikey.Addresses.HTIMBKUP && address <= Mikey.Addresses.TIM7CTLB)
			{
				int offset = address - Mikey.Addresses.HTIMBKUP;
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
				case Mikey.Addresses.INTRST:
					// "Read is a poll, write will reset the int that corresponds to a set bit."
					value ^= 0xff;
					timerInterruptStatusRegister &= value;
					
					// TODO: Keith Wilkins has fix below for Championship Rally here. Is it still necessary?
					//device.Cpu.SignalInterrupt(...)
					//ForceTimerUpdate();
					return;

				case Mikey.Addresses.INTSET:
					// "Read is a poll, write will set the int that corresponds to a set bit."
					timerInterruptStatusRegister |= value;
				
					// TODO: Keith Wilkins has fix for Championship Rally here. Is it still necessary?
					//device.Cpu.SignalInterrupt(...)
					//ForceTimerUpdate();
					return;

				case Mikey.Addresses.MIKEYSREV:
					// "No actual register is implemented"
					return;

				// "Also note that only the lines that are set to input are actually valid for reading."
				// "8 bits I/O direction corresponding to the 8 bits at FD8B 0=input, 1= output"
				case Mikey.Addresses.IODIR:
					IODIR = value;
					return;

				// "Mikey Parallel Data(sort of a R/W) 8 bits of general purpose I/O data"
				case Mikey.Addresses.IODAT:
					IODAT.ByteData = value;

					Debug.WriteLineIf(((IODIR & 0x08) == 0) & GeneralSwitch.TraceInfo, "MikeyChipsetPoke(IODAT): Rest is not set to output.");
					Debug.WriteLineIf(((IODIR & 0x02) == 0) & GeneralSwitch.TraceInfo, "MikeyChipsetPoke(IODAT): CartAddressData is not set to output.");

					// "One is that it is the data pin for the shifter that holds the cartridge address."
					device.Cartridge.CartAddressData(IODAT.CartAddressData);
					// "The other is that it controls power to the cartridge."
					device.CartridgePowerOn = !IODAT.CartPowerOff;
					// "In its current use, it is the write enable line for writeable elements in the cartridge."
					if ((IODIR & 0x10) == 0x10)
						device.Cartridge.WriteEnabled = IODAT.AudioIn;
					return;

				case Mikey.Addresses.SYSCTL1:
					SYSCTL1.ByteData = value;
					if (!SYSCTL1.Power)
					{
						device.Reset();
						// TODO: Enter debug mode id configured
					}
					device.Cartridge.CartAddressStrobe(SYSCTL1.CartAddressStrobe);
					return;

				case Mikey.Addresses.SDONEACK:
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

				case Mikey.Addresses.CPUSLEEP:
					// "A write of '0' to this address will reset the CPU bus request flip fIop.
					// The setting of the flip flop is described in the hardware specification."
					
					// BUG: "Sleep does not work if Suzy does not have the bus."
					// We assume that everyone knows about this bug and behaves accordingly, so 
					// writing zero here must be to give Suzy access to the bus and it will start drawing
					// sprites and will signal when it is done. 
					// This is implemented as a new wakeup time by calculating the number of cycles used
					// and skipping forward in time to that moment. 
					ulong suzyCycles = device.Suzy.RenderSprites();
					device.Cpu.TrySleep(suzyCycles);
					return;
					
				case Mikey.Addresses.DISPCTL:
					DISPCTL.ByteData = value;
					return;

				case Mikey.Addresses.PBKUP:
					// "Additionally, the magic 'P' counter has to be set to match the LCD scan rate. The formula is:
					// INT((((line time - .5us) / 15) * 4) -1)"
					PBKUP = value;
					return;

				case Mikey.Addresses.DISPADRL:
					// "DISPADRL (FD94) is lower 8 bits of display address with the bottom 2 bit ignored by the hardware.
					// The address of the upper left corner of a display buffer must always have '00' in the bottom 2 bits."
					VideoDisplayStartAddress.LowByte = (byte)(value & 0xFD);
					return;

				case Mikey.Addresses.DISPADRH:
					// "DISPADRH (FD95) is upper 8 bits of display address."
					VideoDisplayStartAddress.HighByte = value;
					return;

				case Mikey.Addresses.MAGRDY0:
				case Mikey.Addresses.MAGRDY1:
				case Mikey.Addresses.AUDIN:
				case Mikey.Addresses.MIKEYHREV:
					Debug.WriteLineIf(GeneralSwitch.TraceWarning, String.Format("Mikey::Poke - Read-only address {0:X4} used (value {1:X2}).", address, value));
					break;

				default:
					break;
			}

			// Blue and red color map
			if (address >= Mikey.Addresses.BLUERED0)
			{
				int index = address - Mikey.Addresses.BLUERED0;
				BlueColorMap[index] = (byte)(value & 0xF0);
				RedColorMap[index] = (byte)((value & 0x0F) << 4);
				//ArgbColorMap[index] = 0; //&= 0xFF0000FF;
				//ArgbColorMap[index] |= (value & 0xF0) << 8;
				//ArgbColorMap[index] |= (value & 0x0F) << 28;
				return;
			}

			// Green color map
			if (address >= Mikey.Addresses.GREEN0)
			{
				int index = address - Mikey.Addresses.GREEN0;
				GreenColorMap[index] = (byte)((value & 0x0F) << 4);
				//ArgbColorMap[index] &=  
				//ArgbColorMap[index] |= (value & 0x0F) << 20;
				return;
			}

			Debug.WriteLineIf(GeneralSwitch.TraceWarning, String.Format("Mikey::Poke: Unknown address {0:X$} specified (value {1:X2}).", address,value));
		}

		public byte Peek(ushort address)
		{
			switch (address)
			{
				case Mikey.Addresses.INTSET:
				case Mikey.Addresses.INTRST:
					// "The software reads either the INTSET or INTRST registers (they have duplicate information) ..."
					return timerInterruptStatusRegister;

				case Mikey.Addresses.IODIR:
					return this.IODIR;

				// "Note that some lines are used for several functions, please read the spec.
				// Also note that only the lines that are set to input are actually valid for reading."
				case Mikey.Addresses.IODAT:
					byte value = IODAT.ByteData;
					return (byte)(value & (IODIR ^ 0xff));

				case Mikey.Addresses.PBKUP:
					return PBKUP;

				case Mikey.Addresses.MIKEYHREV:
					// "No actual register is implemented"
					return 0x01;

				case Mikey.Addresses.MIKEYSREV:
					// "No actual register is implemented"
					break;

				// Write-only addresses
				case Mikey.Addresses.CPUSLEEP:
				case Mikey.Addresses.SDONEACK:
				case Mikey.Addresses.DISPADRL:
				case Mikey.Addresses.DISPADRH:
				case Mikey.Addresses.SYSCTL1:
				case Mikey.Addresses.DISPCTL:
					Debug.WriteLineIf(GeneralSwitch.TraceWarning, String.Format("Mikey::Peek - Write-only address {0:X4} used.", address));
					return 0xFF;

				default:
					break;
			}

			if (address >= Mikey.Addresses.BLUERED0)
			{
				int index = address - Mikey.Addresses.BLUERED0;
				return (byte)((BlueColorMap[index] << 4) + RedColorMap[index]);
			}

			if (address >= Mikey.Addresses.GREEN0)
			{
				int index = address - Mikey.Addresses.GREEN0;
				return GreenColorMap[index];
			}

			Debug.WriteLineIf(GeneralSwitch.TraceWarning, String.Format("Mikey::Peek -  Unknown address {0:X4} specified.", address));
			return 0xff;
		}
	}
}
