using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class LinkingTimerLogic: ITimerLogic
	{
		// TODO: Find out what last link carrying is meant for
		public bool LastLinkCarry { get; private set; }
		private Timer Owner;
		
		public LinkingTimerLogic(Timer owner)
		{
			this.Owner = owner;
		}
	
		public void UpdateCurrentValue(ulong currentCycleCount)
		{
			if (Owner.PreviousTimer.DynamicControlBits.BorrowOut)
			{
				Owner.CurrentValue--;
				Owner.DynamicControlBits.BorrowIn = true;
			}
			
			// TODO: Find out what last link carrying is meant for
			LastLinkCarry = Owner.PreviousTimer.DynamicControlBits.BorrowOut;
		}

		public void Start(ulong cycleCount)
		{
			// Nothing to do for linked timers
		}

		public ulong PredictTimerEvent(ulong currentCycleCount)
		{
			// Predicted timer event is always dependent upon linked timing
			return ulong.MaxValue;
		}
	}
}
