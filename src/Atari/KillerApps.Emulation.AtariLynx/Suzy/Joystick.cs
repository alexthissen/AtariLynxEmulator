using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.AtariLynx
{
	public class Joystick
	{
		private JoystickStates state;
		
		public bool LeftHanded { private get; set; }
		public byte Value 
		{
			get 
			{
				return (byte)(LeftHanded ? SwitchHandedness(state) : state);
			} 
		}
		
		private JoystickStates SwitchHandedness(JoystickStates value)
		{
			JoystickStates state = (JoystickStates)(((byte)value) & 0x0F);

			if ((value & JoystickStates.Up) == JoystickStates.Up) state |= JoystickStates.Down;
			if ((value & JoystickStates.Down) == JoystickStates.Down) state |= JoystickStates.Up;
			if ((value & JoystickStates.Left) == JoystickStates.Left) state |= JoystickStates.Right;
			if ((value & JoystickStates.Right) == JoystickStates.Right) state |= JoystickStates.Left;

			return state;
		}

		public JoystickStates State
		{
			set { state = value; }
		}
	}

	[Flags]
	public enum JoystickStates
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