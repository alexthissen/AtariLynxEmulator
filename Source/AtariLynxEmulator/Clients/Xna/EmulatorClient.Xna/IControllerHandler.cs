using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Atari.Lynx;

namespace KillerApps.Gaming.Atari.Xna
{
	public interface IControllerHandler
	{
		JoystickStates Joystick { get; }

		//bool ExitGame { get; }
	}
}
