using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public interface IComLynxTransport
	{
		void Connect(Transmitter transmitter, Receiver receiver);
		void Initialize();
		void ChangeSettings(SerialControlRegister register, int baudrate);
		void Send(byte data);
		void Receive(byte data);
	}
}
