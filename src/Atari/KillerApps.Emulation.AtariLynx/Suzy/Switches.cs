using System;

namespace KillerApps.Emulation.AtariLynx
{
	public class Switches
	{
		private SwitchesStates state;
		public byte Value 
		{
			get 
			{
				return (byte)state;
			} 
		}

		public SwitchesStates State
		{
			set { state = value; }
		}
	}

	[Flags]
	public enum SwitchesStates
	{
		Pause = 0x01,
		Cart0Inactive = 0x02,
		Cart1Inactive = 0x04,
		None = 0x00
	}
}
