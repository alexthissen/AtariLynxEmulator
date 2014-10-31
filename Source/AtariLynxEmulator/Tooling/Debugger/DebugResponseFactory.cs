using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Atari.Lynx.Debugger
{
	public static class DebugResponseFactory
	{
		public static IDebugResponse CreateResponse(byte command)
		{
			if (!Enum.IsDefined(typeof(DebugCommand), command)) return null;
			return CreateResponse((DebugCommand)command);
		}

		public static IDebugResponse CreateResponse(DebugCommand command)
		{
			IDebugResponse response = null;
			switch (command)
			{
				case DebugCommand.Continue:
					response = new DebugResponse<ContinueRequest>();
					break;
				case DebugCommand.ReceiveRegisters:
					response = new DebugResponse<ReceiveRegistersRequest>();
					break;
				case DebugCommand.WriteMemory:
					return null;
				//response = new DebugResponse<WriteMemoryRequest>();
				case DebugCommand.ReadMemory:
					response = new ReadMemoryResponse();
					break;
				case DebugCommand.SendRegisters:
					response = new SendRegistersResponse();
					break;
				case DebugCommand.DownloadScreen:
					response = new DownloadScreenResponse();
					break;
				default:
					return null;
			}
			response.AddBytes(new byte[1] { (byte)command });
			return response;
		}
	}
}
