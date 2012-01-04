using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Atari.Lynx;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;

namespace KillerApps.Gaming.Atari
{
	public class InputHandler : DrawableGameComponent
	{
		protected JoystickStates joystick;

		public InputHandler(Game game): base (game) { }

		public JoystickStates Joystick
		{
			get
			{
				JoystickStates result = JoystickStates.None;
				result = BuildJoystickState();
				return result;
			}
		}

		protected virtual JoystickStates BuildJoystickState()
		{
			return JoystickStates.None;
		}
	}
}
