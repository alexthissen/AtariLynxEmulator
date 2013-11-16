using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Atari.Lynx;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace KillerApps.Gaming.Atari.MonoGame
{
	public class GamePadHandler : InputHandler
	{
		public GamePadHandler(Game game): base(game) { }

		public override bool ExitGame
		{
			get
			{
				GamePadState gamePad = GamePad.GetState(PlayerIndex.One);
				return gamePad.Buttons.Back == ButtonState.Pressed;
			}
		} 

		protected override JoystickStates BuildJoystickState()
		{
			GamePadState gamePad = GamePad.GetState(PlayerIndex.One);
			JoystickStates joystick = JoystickStates.None;

			if (gamePad.DPad.Down == ButtonState.Pressed) joystick |= JoystickStates.Down;
			if (gamePad.DPad.Up == ButtonState.Pressed) joystick |= JoystickStates.Up;
			if (gamePad.DPad.Left == ButtonState.Pressed) joystick |= JoystickStates.Left;
			if (gamePad.DPad.Right == ButtonState.Pressed) joystick |= JoystickStates.Right;
			if (gamePad.Buttons.A == ButtonState.Pressed) joystick |= JoystickStates.Outside;
			if (gamePad.Buttons.B == ButtonState.Pressed) joystick |= JoystickStates.Inside;
			if (gamePad.Buttons.X == ButtonState.Pressed) joystick |= JoystickStates.Option1;
			if (gamePad.Buttons.Y == ButtonState.Pressed) joystick |= JoystickStates.Option2;

			return joystick;
		}
	}
}
