using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.AtariLynx;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace KillerApps.Gaming.MonoGame
{
	public class KeyboardHandler : InputHandler
	{
		public KeyboardHandler(Game game): base(game) { }

		public override bool ExitGame
		{
			get
			{
				KeyboardState keyboard = Keyboard.GetState();
				return keyboard.IsKeyDown(Keys.F4);
			}
		}

		protected override JoystickStates BuildJoystickState()
		{
			KeyboardState keyboard = Keyboard.GetState();
			JoystickStates joystick = JoystickStates.None;

			if (keyboard.IsKeyDown(Keys.Down) == true) joystick |= JoystickStates.Down;
			if (keyboard.IsKeyDown(Keys.Up) == true) joystick |= JoystickStates.Up;
			if (keyboard.IsKeyDown(Keys.Left) == true) joystick |= JoystickStates.Left;
			if (keyboard.IsKeyDown(Keys.Right) == true) joystick |= JoystickStates.Right;
			if (keyboard.IsKeyDown(Keys.X) == true) joystick |= JoystickStates.Outside;
			if (keyboard.IsKeyDown(Keys.Z) == true) joystick |= JoystickStates.Inside;
			if (keyboard.IsKeyDown(Keys.D1) == true) joystick |= JoystickStates.Option1;
			if (keyboard.IsKeyDown(Keys.D2) == true) joystick |= JoystickStates.Option2;

			return joystick;
		}
	}
}
