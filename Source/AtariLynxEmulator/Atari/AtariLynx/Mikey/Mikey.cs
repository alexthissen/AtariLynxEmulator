using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;
using KillerApps.Emulation.Processors;
using System.Diagnostics;

namespace KillerApps.Emulation.Atari.Lynx
{
	public partial class Mikey : IMemoryAccess<ushort, byte>, IResetable
	{
		public const int AUDIO_DPRAM_READWRITE_MIN = 5;
		public const int AUDIO_DPRAM_READWRITE_MAX = 20;
		public const int COLORPALETTE_DPRAM_READWRITE = 5;
		public const int AVAILABLEHARDWARE_READWRITE = 5;

		private ILynxDevice device;
		private byte timerInterruptStatusRegister;
		private byte timerInterruptMask = 0;
		
		public byte[] GreenColorMap = new byte[0x10];
		public byte[] BlueRedColorMap = new byte[0x10];
		public uint[] ArgbColorMap = new uint[0x10];

		private Uart comLynx;
		public SystemControlBits1 SYSCTL1 { get; private set; }
		public ParallelData IODAT { get; private set; }
		public DisplayControlBits DISPCTL { get; private set; }
		public ParallelDataDirection IODIR { get; private set; }
		public byte PBKUP { get; set; }
		public MagTapeChannelReadyBit MAGRDY0 { get; private set; }
		public MagTapeChannelReadyBit MAGRDY1 { get; private set; }
		public AudioIn AUDIN { get; private set; }	
		public Word VideoDisplayStartAddress;
		public StereoConnection Stereo { get; private set; }
		public AudioFilter AudioFilter { get; private set; }
		public bool ComLynxCablePresent = false;
		
		// Timers
		public Timer[] Timers = new Timer[8];

		// "There are four identical audio channels."
		// "The audio system is mono with 4 voices and a frequency response from 100Hz to 4kHz."
		public AudioChannel[] AudioChannels = new AudioChannel[4];
		
		//private static TraceSwitch GeneralSwitch = new TraceSwitch("General", "General trace switch", "Error");

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
		private int[] LcdScreenDma;
		private byte[] VideoMemoryDma;
		private byte currentLine;
		private bool RestActive;
		private bool SoundEnabled = true;

		public Mikey(ILynxDevice lynx)
		{
			this.device = lynx;
			this.comLynx = new Uart();
			this.AudioFilter = new AudioFilter();

			// "reset = x"
			MAGRDY0 = new MagTapeChannelReadyBit(0);
			MAGRDY1 = new MagTapeChannelReadyBit(0);
		}

		public void Initialize()
		{
			InitializeTimers();
			if (SoundEnabled) InitializeAudioChannels();

			// TODO: Clean this hack up and use a decent way to get at the LCD screen memory
			LcdScreenDma = ((LynxHandheld)device).LcdScreenDma;
			// TODO: Another hack to avoid rendering when timer 2 has only just started
			currentLcdDmaCounter = -1;
			VideoMemoryDma = device.Ram.GetDirectAccess();
			unchecked
			{
				for (int index = 0; index <= 0x0F; index++) ArgbColorMap[index] = 0xFF000000;
			}
		}

		private void InitializeAudioChannels()
		{
			for (int index = 0; index < AudioChannels.Length; index++)
			{
				AudioChannel channel = new AudioChannel();

				// "FD28 -> FD2F Audio channel 1, links from audio timer 0"
				// "FD30 -> FD37 Audio channel 2, links from audio timer 1"
				// "FD38 -> FD3F Audio channel 3, links trom audio timer 2"
				if (index > 0) channel.PreviousTimer = AudioChannels[index - 1];
				AudioChannels[index] = channel;
			}

			// "FD20 -> FD27 Audio channel 0, links from timer 7"
			AudioChannels[0].PreviousTimer = Timers[7];
		}

		private void InitializeTimers()
		{
			// Create all timers
			for (int index = 0; index < 8; index++)
			{
				byte interruptMask = (byte)(1 << index);
				Timers[index] = new Timer(interruptMask);

				// Hook up linked timer to previous timer in group A and B
				// "Group A:
				// Timer 0 -> Timer 2 -> Timer 4."
				// "Group B:
				// Timer 1 -> Timer 3 -> Timer 5 -> Timer 7 -> Audio 0 -> Audio 1-> Audio 2 -> Audio 3 -> Timer 1."
				if (index > 1) Timers[index].PreviousTimer = Timers[index - 2];

				if (index % 2 != 0) Timers[index].Expired += new EventHandler<TimerExpirationEventArgs>(TimerExpired);
			}

			// "... Audio 3 -> Timer 1."
			if (SoundEnabled) Timers[1].PreviousTimer = AudioChannels[3];

			// Timer 6 is not part of a group and does not have a timer linked to it
			Timers[6].PreviousTimer = null;

			// "Two of the timers will be used for the video frame rate generator."
			// "One (timer 0) is set to the length of a display line and ..."
			Timers[0].Expired += new EventHandler<TimerExpirationEventArgs>(RenderLine);
			// "... the second (timer 2) is set to the number of lines."
			Timers[2].Expired += new EventHandler<TimerExpirationEventArgs>(DisplayEndOfFrame);
			// "One of the timers (timer 4) will be used as the baud rate generator for the serial expansion port (UART)."
			Timers[4].Expired += new EventHandler<TimerExpirationEventArgs>(GenerateBaudRate);
			Timers[6].Expired += new EventHandler<TimerExpirationEventArgs>(TimerExpired);
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
				//Debug.WriteLineIf(GeneralSwitch.TraceInfo, String.Format("Mikie::Update() - Timer IRQ Triggered at {0:X8}", device.SystemClock.CompatibleCycleCount));
				device.Cpu.SignalInterrupt(InterruptType.Irq);
			}
		}

		public void GenerateBaudRate(object sender, TimerExpirationEventArgs e)
		{
			Timer timer = (Timer)sender;

			bool fireInterrupt = comLynx.GenerateBaudRate();
			if (fireInterrupt)
			{
				// Update interrupt status
				timerInterruptStatusRegister |= e.InterruptMask;

				// Trigger a maskable interrupt
				//Debug.WriteLineIf(GeneralSwitch.TraceInfo, String.Format("Mikie::Update() - Timer IRQ Triggered at {0:X8}", device.SystemClock.CompatibleCycleCount));
				device.Cpu.SignalInterrupt(InterruptType.Irq);
			}
		}
		
		private void DisplayEndOfFrame(object sender, TimerExpirationEventArgs e) 
		{
			TimerExpired(sender, e);

			currentLcdDmaCounter = 0;
			currentLine = Timers[2].BackupValue;

			device.NewVideoFrameAvailable = true;
		}
		
		private void RenderLine(object sender, TimerExpirationEventArgs e) 
		{
			TimerExpired(sender, e);

			// TODO: Implementation of Keith does not always trigger IRQ when DMA is not enabled, 
			// display bits (?) or display current have not been set
			if (!DISPCTL.EnableVideoDma) return;

			int backupValue = Timers[2].BackupValue;
			//currentLine = Timers[2].CurrentValue;
			
			// "The current method of driving the LCD requires 3 scan lines of vertical blank."
			// TODO: Determine if rest is between frames 104, 103 and 102 (for 60Hz)
			// Keith Wilkins has Rest period between 102, 101 and 100
			bool rest = (currentLine >= (backupValue - 4) && currentLine <= (backupValue - 2));
			RestActive = rest;
			//IODAT.Rest = rest;

			if (currentLine > (backupValue - 3))
			{
				currentLine--;
				return;
			}
			if (currentLine == (backupValue - 3))
			{
				// Pick up current video start address
				// "The hardware address in Mikey (DISPADDR) is the backup value for the actual address counter. 
				// The backup value is transferred to the address counter at the very start of the third line of 
				// vertical blanking."
				currentLynxDmaAddress = (ushort)(VideoDisplayStartAddress.Value & 0xFFFC);

				// "The value in the register is the start (upper left corner) of the display buffer in normal 
				// mode and the end (lower right corner) of the display buffer in FLIP mode"
				if (DISPCTL.Flip)
				{
					currentLynxDmaAddress += 3;
				}
			}

			if (currentLine > 0) currentLine--;
			if (currentLcdDmaCounter < 0) return;

			// TODO: Define constant for DMA_READWRITE_CYCLE
			e.CyclesInterrupt = 80 * 4;
		
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
			LcdScreenDma[currentLcdDmaCounter++] = (int)ArgbColorMap[source];
		}

		public void Reset()
		{
			comLynx.Reset();

			Initialize();

			// SDONEACK = 0 "(not acked)"
			timerInterruptMask = timerInterruptStatusRegister = 0;
			IODIR = new ParallelDataDirection(0x00); // "reset = 0,0.0.0,0,0,0,0"
			IODAT = new ParallelData(0x00);
			SYSCTL1 = new SystemControlBits1(0x02); // "reset x,x,x,x,x,x,1,0"
			DISPCTL = new DisplayControlBits(0x00);	// "reset = 0"
			AUDIN = new AudioIn(0x80); // "reset = b7,0,0,0,0,0,0,0"

			// "Audio are reset to 0, all are read/write"
			// TODO: Reset audio registers to zero
			Stereo = new StereoConnection();
		}

		private void ForceTimerUpdate()
		{
			//device.NextTimerEvent = device.SystemClock.CycleCount;
			device.NextTimerEvent = device.SystemClock.CompatibleCycleCount;
		}

		public void Update() 
		{
			// "The 4 audio channels are mixed digitally and a pulse width modulated waveform is 
			// output from Mikey to the audio filter. This filter is a 1 pole low pass fitter with a 
			if (SoundEnabled)
			{
				byte sample = MixAudioSample();
				AudioFilter.Output(device.SystemClock.CompatibleCycleCount, sample);
			}

			// Take lowest timer 
			ulong nextTimer = UInt64.MaxValue;
			ulong cycleCountAdvance = 0;
			
			//Debug.WriteLineIf(GeneralSwitch.TraceVerbose, "Mikey::Update");
			foreach (Timer timer in Timers)
			{
				cycleCountAdvance += timer.Update(device.SystemClock.CompatibleCycleCount);
				if (timer.StaticControlBits.EnableCount && (timer.ExpirationTime < nextTimer))
					nextTimer = timer.ExpirationTime;
			}

			if (SoundEnabled)
			{
				foreach (AudioChannel channel in AudioChannels)
				{
					channel.Update(device.SystemClock.CompatibleCycleCount);
					if (channel.AudioControl.EnableCount && (channel.ExpirationTime < nextTimer))
						nextTimer = channel.ExpirationTime;
				}
			}

			// Take lowest timer 
			device.NextTimerEvent = nextTimer;
			//var clocks = Timers.Where(t => t.StaticControlBits.EnableCount);
			//if (clocks.Count() > 0) device.NextTimerEvent = clocks.Min(t => t.ExpirationTime);

			if (device.Cpu.IsAsleep)
			{
				// Make wakeup time next timer event if earlier than first timer
				if (device.NextTimerEvent > device.Cpu.ScheduledWakeUpTime) device.NextTimerEvent = device.Cpu.ScheduledWakeUpTime;
			}

			//Trace.WriteLine(String.Format("Time={0:D12}, NextTimer={1:D12}", device.SystemClock.CompatibleCycleCount, device.NextTimerEvent));
			device.SystemClock.CompatibleCycleCount += cycleCountAdvance;
		}

		private byte MixAudioSample()
		{
			long sample = 0;
			int mixedChannels = 0;

			for (int index = 0; index < 4; index++)
			{
				if (!Stereo.RightEar.AudioChannelDisabled[index])
				{
					sample += AudioChannels[index].OutputValue;
					mixedChannels++;
				}
			}
			if (mixedChannels != 0)
			{
				sample += 128 * mixedChannels;
				sample /= mixedChannels;
				//sample += 128;
			}
			else
				sample = 128;

			return (byte)sample;
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
						StaticControlBits control = new StaticControlBits(value);
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

			if (address >= Mikey.Addresses.AUD0VOL && address <= Mikey.Addresses.AUD3MISC && SoundEnabled)
			{
				int offset = address - Mikey.Addresses.AUD0VOL;
				int index = offset >> 3; // Divide by 8 to get index of audio channel
				AudioChannel channel = AudioChannels[index];

				// "FD20 -> FD27 Audio channel 0, links from timer 7"
				// "FD28 -> FD2F Audio channel 1, links from audio timer 0"
				// "FD30 -> FD37 Audio channel 2, links from audio timer 1"
				// "FD38 -> FD3F Audio channel 3, links trom audio timer 2"
				switch (offset % 8)
				{
					case 0: // "8 bit. 2's Complement Volume Control"
						channel.VolumeControl = (sbyte)value;
						return;
					
					case 1: // "Shift register feedback enable"
						channel.FeedbackEnable.ByteData = value;
						return;

					case 2: // "Audio output value"
						channel.OutputValue = (sbyte)value;
						return;

					case 3: // "Lower 8 Bits of Shift Register"
						channel.LowerShiftRegister = value;
						return;

					case 4: // "Audio Timer Backup Value"
						channel.BackupValue = value;
						return;

					case 5: // "Audio Control Bits"
						//channel.AudioControl.ByteData = value;
						AudioControlBits control = new AudioControlBits(value);
						channel.AudioControl = control;

						// "It is set on time out, reset with the reset timer done bit (xxx1, B6)"
						if (control.ResetTimerDone)
						{
							channel.DynamicControlBits.TimerDone = false;
						}
						if (control.EnableCount || control.ResetTimerDone)
						{
							channel.Start(device.SystemClock.CompatibleCycleCount);
							ForceTimerUpdate();
						}
						return;

					case 6: // "Audio counter"
						channel.CurrentValue = value;
						ForceTimerUpdate();
						return;

					case 7: // "Other control bits"
						channel.OtherControlBits = value; 
						return;
				}
			}

			// Handle other addresses
			switch (address)
			{
				case Mikey.Addresses.MSTEREO:
					// "The Howard boards were not yet finished, so we went ahead and implemented this stereo on them. 
					// This form of stereo was channel switching controlled by FD50."
					// TODO: Implement stereo
					return;

				case Mikey.Addresses.MPAN:
					// TODO: Implement panning
					return;

				case Mikey.Addresses.INTRST:
					// "Read is a poll, write will reset the int that corresponds to a set bit."
					value ^= 0xff;
					timerInterruptStatusRegister &= value;

					// TODO: Keith Wilkins has fix below for Championship Rally here. Is it still necessary?
					if (timerInterruptStatusRegister != 0) device.Cpu.SignalInterrupt(InterruptType.Irq);
					ForceTimerUpdate();
					return;

				case Mikey.Addresses.INTSET:
					// "Read is a poll, write will set the int that corresponds to a set bit."
					timerInterruptStatusRegister |= value;
				
					// TODO: Keith Wilkins has fix for Championship Rally here. Is it still necessary?
					if (timerInterruptStatusRegister != 0) device.Cpu.SignalInterrupt(InterruptType.Irq);
					ForceTimerUpdate();
					return;

				case Mikey.Addresses.MIKEYSREV:
					// "No actual register is implemented"
					return;

				// "Also note that only the lines that are set to input are actually valid for reading."
				// "8 bits I/O direction corresponding to the 8 bits at FD8B 0=input, 1= output"
				case Mikey.Addresses.IODIR:
					IODIR.ByteData = value;
					return;

				// "Mikey Parallel Data(sort of a R/W) 8 bits of general purpose I/O data"
				case Mikey.Addresses.IODAT:
					IODAT.ByteData = value;

					// "One is that it is the data pin for the shifter that holds the cartridge address."
					device.Cartridge.CartAddressData(IODAT.CartAddressData);
				
					// "The other is that it controls power to the cartridge."
					device.CartridgePowerOn = !IODAT.CartPowerOff;
					
					// "In its current use, it is the write enable line for writeable elements in the cartridge."
					if (IODIR.AuxiliaryDigitalInOut == DataDirection.Output)
						device.Cartridge.AuxiliaryDigitalInOut = IODAT.AuxiliaryDigitalInOut;

					// "In its current use, it is the write enable line for writeable elements in the cartridge."
					if (IODIR.AuxiliaryDigitalInOut == DataDirection.Output)
						device.Cartridge.WriteEnabled = IODAT.AuxiliaryDigitalInOut;
					return;

				case Mikey.Addresses.SERCTL:
					comLynx.SetSerialControlRegister(value);
					return;

				case Mikey.Addresses.SERDAT:
					comLynx.TransmitSerialData(value);
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
					// "A write of '0' to this address will reset the CPU bus request flip flop.
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
					//Debug.WriteLineIf(GeneralSwitch.TraceWarning, String.Format("Mikey::Poke - Read-only address {0:X4} used (value {1:X2}).", address, value));
					break;

				default:
					break;
			}

			if (address > Mikey.Addresses.BLUEREDF) return;

			// Blue and red color map
			if (address >= Mikey.Addresses.BLUERED0 && address <= Mikey.Addresses.BLUEREDF)
			{
				int index = address - Mikey.Addresses.BLUERED0;
				BlueRedColorMap[index] = value;
				ArgbColorMap[index] &= 0xFF00FF00;
				ArgbColorMap[index] |= (uint)((value & 0xF0) << 16); // Blue
				ArgbColorMap[index] |= (uint)((value & 0x0F) << 4); // Red
				return;
			}

			// Green color map
			if (address >= Mikey.Addresses.GREEN0)
			{
				int index = address - Mikey.Addresses.GREEN0;
				GreenColorMap[index] = value;
				ArgbColorMap[index] &= 0xFFFF00FF;
				ArgbColorMap[index] |= (uint)((value & 0x0F) << 12);
				return;
			}

			if (address >= 0xFD20 && address <= 0xFD3F)
				return;

			//Trace.WriteLineIf(GeneralSwitch.TraceWarning, String.Format("Mikey::Poke: Unknown address ${0:X4} specified (value={1:X2}).", address,value));
		}

		public byte Peek(ushort address)
		{
			// Timer addresses can be handled separately
			if (address >= Mikey.Addresses.HTIMBKUP && address <= Mikey.Addresses.TIM7CTLB)
			{
				int offset = address - Mikey.Addresses.HTIMBKUP;
				int index = offset >> 2; // Divide by 4 to get index of timer
				Timer timer = Timers[index];
				
				// TODO: This line introduces a bug in Awesome Golf, but fixes the random blocks in Blockout
				// Find out what breaks Awesome Golf
				timer.Update(device.SystemClock.CompatibleCycleCount);

				switch (offset % 4)
				{
					case 0: // Backup value
						return timer.BackupValue;

					case 1: // Static control
						return timer.StaticControlBits.ByteData;

					case 2: // Current value
						return timer.CurrentValue;

					case 3: // Dynamic control bits
						return timer.DynamicControlBits.ByteData;
				}
			}

			if (address >= Mikey.Addresses.AUD0VOL && address <= Mikey.Addresses.AUD3MISC && SoundEnabled)
			{
				int offset = address - Mikey.Addresses.AUD0VOL;
				int index = offset >> 3; // Divide by 8 to get index of audio channel
				AudioChannel channel = AudioChannels[index];

				switch (offset % 8)
				{
					case 0: // "8 bit. 2's Complement Volume Control"
						return (byte)channel.VolumeControl;

					case 1: // "Shift register feedback enable"
						return channel.FeedbackEnable.ByteData;

					case 2: // "Audio output value"
						return (byte)channel.OutputValue;

					case 3: // "Lower 8 Bits of Shift Register"
						return channel.LowerShiftRegister;

					case 4: // "Audio Timer Backup Value"
						return channel.BackupValue;

					case 5: // "Audio Control Bits"
						return channel.AudioControl.ByteData;

					case 6: // "Audio counter"
						return channel.CurrentValue;

					case 7: // "Other control bits"
						return channel.OtherControlBits;
				}
			}

			switch (address)
			{
				case Mikey.Addresses.MSTEREO:
					// "The Howard boards were not yet finished, so we went ahead and implemented this stereo on them. 
					// This form of stereo was channel switching controlled by FD50."
					// TODO: Implement stereo
					return 0x00;

				case Mikey.Addresses.MPAN:
					// TODO: Implement panning
					return 0x00;

				case Mikey.Addresses.INTSET:
				case Mikey.Addresses.INTRST:
					// "The software reads either the INTSET or INTRST registers (they have duplicate information) ..."
					return timerInterruptStatusRegister;

				case Mikey.Addresses.MAGRDY0:
					byte magReady0 = MAGRDY0.ByteData;
					// "B7=edge (1) Reset upon read."
					MAGRDY0.Edge = false;
					return magReady0;
					
				case Mikey.Addresses.MAGRDY1:
					byte magReady1 = MAGRDY0.ByteData;
					// "B7=edge (1) Reset upon read."
					MAGRDY0.Edge = false;
					return magReady1;

				case Mikey.Addresses.IODIR:
					return this.IODIR.ByteData;

				// "Note that some lines are used for several functions, please read the spec.
				// Also note that only the lines that are set to input are actually valid for reading."
				case Mikey.Addresses.IODAT:
					byte value = 0x00;
					if (IODIR.AuxiliaryDigitalInOut == DataDirection.Input && device.Cartridge.AuxiliaryDigitalInOut) 
						value |= ParallelData.AuxiliaryDigitalInOutMask;
					if (IODIR.AuxiliaryDigitalInOut == DataDirection.Output && IODAT.AuxiliaryDigitalInOut) 
						value |= ParallelData.AuxiliaryDigitalInOutMask;
					if (IODIR.Rest == DataDirection.Output && (!IODAT.Rest || !RestActive)) value |= ParallelData.RestMask;
					if ((IODIR.NoExpansion == DataDirection.Input && ComLynxCablePresent) || 
							(IODIR.NoExpansion == DataDirection.Output && IODAT.NoExpansion))
					  value |= ParallelData.NoExpansionMask;
					if (IODIR.CartAddressData == DataDirection.Output && IODAT.CartAddressData) value |= ParallelData.CartAddressDataMask;
					if (IODIR.ExternalPower == DataDirection.Input || IODAT.ExternalPower) value |= ParallelData.ExternalPowerMask;
					return value;

				case Mikey.Addresses.SERCTL:
					return comLynx.SERCTL.ByteData;

				case Mikey.Addresses.SERDAT:
					// TODO: Build real buffer
					comLynx.SERCTL.ReceiveReady = false;
					return comLynx.SERDAT;

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
					//Debug.WriteLineIf(GeneralSwitch.TraceWarning, String.Format("Mikey::Peek - Write-only address ${0:X4} used.", address));
					return 0xFF;

				default:
					break;
			}

			if (address >= Mikey.Addresses.BLUERED0 && address <= Mikey.Addresses.BLUEREDF)
			{
				int index = address - Mikey.Addresses.BLUERED0;
				return BlueRedColorMap[index];
			}

			if (address >= Mikey.Addresses.GREEN0 && address <= Mikey.Addresses.GREENF)
			{
				int index = address - Mikey.Addresses.GREEN0;
				return GreenColorMap[index];
			}

			//Trace.WriteLineIf(GeneralSwitch.TraceWarning, String.Format("Mikey::Peek -  Unknown address ${0:X4} specified.", address));
			return 0x00;
		}
	}
}
