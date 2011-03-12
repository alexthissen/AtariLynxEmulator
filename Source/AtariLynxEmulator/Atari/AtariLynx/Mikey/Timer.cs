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
		public byte BackupValue { get; internal set; }
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
				TimerLogic = TimerLogicFactory.CreateTimerLogic(this, staticControlBits);
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
		public void Update(ulong currentCycleCount)
		{
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
					Expire();
				}

				ExpirationTime = TimerLogic.PredictExpirationTime(currentCycleCount);
			}
		}

		protected virtual void Expire()
		{
			// "It is set on time out, reset with the reset timer done bit (xxx1, B6)"
			DynamicControlBits.TimerDone = true; // !StaticControlBits.ResetTimerDone;
			DynamicControlBits.BorrowOut = true;

			// "Timers can be set to stop when they reach a count of 0 or to reload from their backup register."
			// Reload if neccessary
			CurrentValue = StaticControlBits.EnableReload ? BackupValue : (byte)0;

			Debug.WriteLine("Mikie::Update() - Timer IRQ Triggered");
			if (Expired != null) 
				Expired(this, new TimerExpirationEventArgs() { InterruptMask = this.InterruptMask });
		}

		public void Start(ulong cycleCount)
		{ 
			// Delegate down to internal logic
			TimerLogic.Start(cycleCount);
			
			// Starting a timer will force an update
			//Update(cycleCount);
		}
	}
}
