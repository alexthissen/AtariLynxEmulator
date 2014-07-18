using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public interface IComLynxTransport
	{
		bool DisableSoftwareTransmitLoopback { get; }
		void Send(byte data);
		byte Receive();
	}
}
