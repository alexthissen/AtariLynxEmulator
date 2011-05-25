using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class Joystick
	{
		private JoyStickStates state;

		public byte Value 
		{
			get { return (byte)state; } 
		}
		
		public bool LeftHanded { private get; set; }

		public JoyStickStates State
		{
			set
			{
				if (LeftHanded) state = value;
				//state = 0x00;
				//if ((value & JoyStickStates.Up) == JoyStickStates.Up) state |= (byte)(LeftHanded ? 0x80 : 0x40);
				//if ((value & JoyStickStates.Down) == JoyStickStates.Down) state |= (byte)(LeftHanded ? 0x40 : 0x80);
				//if ((value & JoyStickStates.Left) == JoyStickStates.Left) state |= (byte)(LeftHanded ? 0x20 : 0x10);
				//if ((value & JoyStickStates.Right) == JoyStickStates.Right) state |= (byte)(LeftHanded ? 0x10 : 0x20);
				//if ((value & JoyStickStates.Option1) == JoyStickStates.Option1) state |= 0x08;
				//if ((value & JoyStickStates.Option2) == JoyStickStates.Option2) state |= 0x04;
				//if ((value & JoyStickStates.Inside) == JoyStickStates.Inside) state |= 0x02;
				//if ((value & JoyStickStates.Outside) == JoyStickStates.Outside) state |= 0x01;
			}
		}
	}

	[Flags]
	public enum JoyStickStates
	{
		Up = 0x80,
		Down = 0x40,
		Left = 0x20,
		Right = 0x10,
		Option1 = 0x08,
		Option2 = 0x04,
		Inside = 0x02,
		Outside = 0x01,
		None = 0x00
	}
}