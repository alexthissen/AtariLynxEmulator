using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class Timer
	{
		public byte InterruptMask { get; private set; }
		public byte BackupValue { get; set; } // TODO: Revert setter to internal
		public event EventHandler<TimerExpirationEventArgs> Expired;
		internal ITimerLogic TimerLogic;
		private StaticTimerControl staticControlBits;
		public Timer PreviousTimer { get; internal set; }
		public ulong ExpirationTime { get; private set; }
		public DynamicTimerControl DynamicControlBits { get; protected set; }
		public byte CurrentValue { get; set; }

		public StaticTimerControl StaticControlBits
		{
			get { return staticControlBits; }
			set
			{
				staticControlBits = value;
				TimerLogic = TimerLogicFactory.CreateTimerLogic(this, staticControlBits, TimerLogic);
			}
		}

		public Timer(byte interruptMask)
		{
			InterruptMask = interruptMask;
			StaticControlBits = new StaticTimerControl(0);
			DynamicControlBits = new DynamicTimerControl(0);
		}

		// "Timers are reset to 0"
		public void Reset()
		{
			BackupValue = 0;
			StaticControlBits.ByteData = 0;
			CurrentValue = 0;
			DynamicControlBits.ByteData = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>Number of system cycles used by work in timer interrupt</returns>
		public ulong Update(ulong currentCycleCount)
		{
			ulong cyclesInterrupt = 0;

			// Only enabled and not-done timers should predict expiration time
			ExpirationTime = ulong.MaxValue;
			if (StaticControlBits.EnableCount && (StaticControlBits.EnableReload || !DynamicControlBits.TimerDone))
			{
				// Assume timer has not expired and is not updated
				DynamicControlBits.BorrowOut = false;
				DynamicControlBits.BorrowIn = false; // TODO: Find out why and when borrow-in is set

				// Calculate new current value and update is necessary
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

		protected virtual ulong Expire()
		{
			ulong cyclesInterrupt = 0;

			// "It is set on time out, reset with the reset timer done bit (xxx1, B6)"
			DynamicControlBits.TimerDone = true; // !StaticControlBits.ResetTimerDone;
			DynamicControlBits.BorrowOut = true;

			// "Timers can be set to stop when they reach a count of 0 or to reload from their backup register."
			// Reload if neccessary
			CurrentValue = StaticControlBits.EnableReload ? BackupValue : (byte)0;

			if (Expired != null)
			{
				TimerExpirationEventArgs args = new TimerExpirationEventArgs(this.InterruptMask);
				Expired(this, args);
				cyclesInterrupt = args.CyclesInterrupt;
			}
			
			return cyclesInterrupt;
		}

		public void Start(ulong cycleCount)
		{
			// Starting a timer will force an update
			//Update(cycleCount);
			//TimerLogic.UpdateCurrentValue(cycleCount);

			// Delegate down to internal logic
			TimerLogic.Start(cycleCount);
		}
	}
}
