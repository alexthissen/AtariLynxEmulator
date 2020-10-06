using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.AtariLynx
{
	public abstract class TimerBase : IResetable
	{
		public byte BackupValue { get; set; } // TODO: Revert setter to internal
		public event EventHandler<TimerExpirationEventArgs> Expired;
		internal ITimerLogic TimerLogic;
		public TimerBase PreviousTimer { get; internal set; }
		public ulong ExpirationTime { get; protected set; }
		public DynamicTimerControl DynamicControlBits { get; private set; }
		public abstract TimerControlBase TimerControlBits { get; }
		public byte CurrentValue { get; set; }

		public TimerBase()
		{
			DynamicControlBits = new DynamicTimerControl(0);
		}

		protected void OnExpire(TimerExpirationEventArgs args)
		{
			if (Expired != null)
			{
				Expired(this, args);
			}
		}

		public virtual void Reset()
		{
			BackupValue = 0;
			CurrentValue = 0;
			DynamicControlBits.ByteData = 0;
		}

		public abstract ulong Update(ulong currentCycleCount);
		
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
