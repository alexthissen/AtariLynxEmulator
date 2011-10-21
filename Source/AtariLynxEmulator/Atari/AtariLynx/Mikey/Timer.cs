using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class Timer: TimerBase
	{
		public byte InterruptMask { get; private set; }
		private StaticControlBits staticControlBits;

		public Timer(byte interruptMask)
			: base()
		{
			InterruptMask = interruptMask;
			StaticControlBits = new StaticControlBits(0);
		}

		public StaticControlBits StaticControlBits
		{
			get { return staticControlBits; }
			set
			{
				staticControlBits = value;

				// Timer 4 (UART for ComLynx) has different clocking logic
				if (InterruptMask == 0x10)
				{
					// TODO: Check if UART timer logic needs to be initialized from current timer
					TimerLogic = new UartTimerLogic(this);
				}
				else
					TimerLogic = TimerLogicFactory.CreateTimerLogic(this, staticControlBits.SourcePeriod, TimerLogic);
			}
		}

		public override TimerControlBase TimerControlBits
		{
			get { return staticControlBits; }
		}

		// "Timers are reset to 0"
		public override void Reset()
		{
			base.Reset();
			StaticControlBits.ByteData = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>Number of system cycles used by work in timer interrupt</returns>
		public override ulong Update(ulong currentCycleCount)
		{
			ulong cyclesInterrupt = 0;

			// Only enabled and not-done timers should predict expiration time
			ExpirationTime = ulong.MaxValue;
			if (StaticControlBits.EnableCount && (StaticControlBits.EnableReload || !DynamicControlBits.TimerDone))
			{
				// Assume timer has not expired and is not updated
				DynamicControlBits.BorrowOut = false;
				DynamicControlBits.BorrowIn = false; // TODO: Find out why and when borrow-in is set

				// Calculate new current value and update if necessary
				bool expired = TimerLogic.UpdateCurrentValue(currentCycleCount);

				// When timer value has expired it will borrow out
				if (expired)
				{
					cyclesInterrupt = Expire();
				}

				ExpirationTime = TimerLogic.PredictExpirationTime(currentCycleCount);
			}
			return cyclesInterrupt;
		}

		protected ulong Expire()
		{
			ulong cyclesInterrupt = 0;

			// "It is set on time out, reset with the reset timer done bit (xxx1, B6)"
			DynamicControlBits.TimerDone = true; // !StaticControlBits.ResetTimerDone;
			DynamicControlBits.BorrowOut = true;

			// "Timers can be set to stop when they reach a count of 0 or to reload from their backup register."
			// Reload if neccessary
			CurrentValue = StaticControlBits.EnableReload ? BackupValue : (byte)0;

			TimerExpirationEventArgs args = new TimerExpirationEventArgs(this.InterruptMask);
			OnExpire(args);
			cyclesInterrupt = args.CyclesInterrupt;
			
			return cyclesInterrupt;
		}
	}
}
