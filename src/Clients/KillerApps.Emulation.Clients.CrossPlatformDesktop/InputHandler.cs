using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.AtariLynx;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace KillerApps.Gaming.MonoGame
{
	public class InputHandler : DrawableGameComponent
	{
		public InputHandler(Game game): base (game) { }

		public JoystickStates Joystick
		{
			get
			{
				return BuildJoystickState();
			}
		}

		public SwitchesStates Switches
		{
			get
			{
				return BuildSwitchesState();
			}
		}

		public virtual bool ExitGame { get { return false; } }

		protected virtual JoystickStates BuildJoystickState()
		{
			return JoystickStates.None;
		}

		protected virtual SwitchesStates BuildSwitchesState()
		{	
			return SwitchesStates.None;
		}
	}
}
