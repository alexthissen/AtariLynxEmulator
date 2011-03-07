using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace KillerApps.Emulation.Atari.Lynx
{
	public abstract class TimerBase
	{
		public byte InterruptMask { get; set; }

		public ClockSelect SourcePeriod { get; set; }
		public byte CurrentValue { get; set; }
		public byte BackupValue { get; set; }

		public StaticTimerControl StaticControlBits { get; protected set; }
		public DynamicTimerControl DynamicControlBits { get; protected set; }

		public TimerBase(byte interruptMask)
		{
			InterruptMask = interruptMask;
		}

		// "Timers are reset to 0"
		public void Reset()
		{
			BackupValue = 0;
			StaticControlBits.ByteData = 0;
			CurrentValue = 0;
			DynamicControlBits.ByteData = 0;
		}

		public event EventHandler<InterruptEventArgs> IrqFired;

		/// <summary>
		/// 
		/// </summary>
		/// <returns>Number of system cycles used by work in timer interrupt</returns>
		public void Update(ulong currentCycleCount)
		{
			if (!StaticControlBits.EnableCount) return;

			// Assume timer has not expired and is not updated
			DynamicControlBits.BorrowOut = false;
			DynamicControlBits.BorrowIn = false; // TODO: Find out why and when borrow-in is set

			// Calculate new current value and update is necessary
			UpdateCurrentValue(currentCycleCount);
			
			// When value has gone negative it will borrow out and trigger IRQ if enabled
			if ((CurrentValue & 0x80000000) == 0x80000000)
			{
				HandleUnderflow();
			}
		}

		protected virtual void HandleUnderflow()
		{
			// "It is set on time out, reset with the reset timer done bit (xxx1, B6)"
			DynamicControlBits.BorrowOut = true;
			DynamicControlBits.TimerDone = true;

			// Reload if neccessary
			// "Timers can be set to stop when they reach a count of 0 or to reload from their backup register."
			if (StaticControlBits.EnableReload)
			{
				CurrentValue += BackupValue;
				// "As with the audio channels, a count of 0 is valid for 1 full cycle of the selected clock."
				CurrentValue++;
			}
			else
			{
				CurrentValue = 0;
			}

			// Handle maskable IRQ when enabled
			if (StaticControlBits.EnableInterrupt)
			{
				Debug.WriteLine("Mikie::Update() - Timer IRQ Triggered");
				IrqFired(this, new InterruptEventArgs() { Mask = InterruptMask });
			}
		}

		protected abstract void UpdateCurrentValue(ulong currentCycleCount);
		public abstract void Start(ulong cycleCount);
	}
}
