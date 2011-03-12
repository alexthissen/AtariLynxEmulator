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
	
		public bool UpdateCurrentValue(ulong currentCycleCount)
		{
			// TODO: Find out what last link carrying is meant for
			LastLinkCarry = Owner.PreviousTimer.DynamicControlBits.BorrowOut;
			
			if (Owner.PreviousTimer.DynamicControlBits.BorrowOut)
			{
				Owner.DynamicControlBits.BorrowIn = true;

				if (Owner.CurrentValue == 0)
				{
					return true;
				}
				else
				{
					Owner.CurrentValue--;
				}
			}
			return false;
		}

		public void Start(ulong cycleCount)
		{
			// Nothing to do for linked timers
		}

		public ulong PredictExpirationTime(ulong currentCycleCount)
		{
			// Predicted timer event is always dependent upon linked timing
			return ulong.MaxValue;
		}
	}
}
