using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.AtariLynx;

namespace KillerApps.Gaming.MonoGame
{
	public interface IControllerHandler
	{
		JoystickStates Joystick { get; }

		//bool ExitGame { get; }
	}
}
