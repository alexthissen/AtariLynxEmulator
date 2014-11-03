using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Atari.Lynx.Debugger
{
	internal class BreakpointManager
	{
		private DebugEngine engine;
		private List<DebugBreakpoint> breakpoints = new List<DebugBreakpoint>();

		public BreakpointManager(DebugEngine engine)
		{
			this.engine = engine;
		}

		public IEnumerable<DebugBreakpoint> Breakpoints { get { return breakpoints.AsEnumerable(); } }

		public void SetBreakpoint(ushort address, string name = null)
		{
			// Check whether breakpoint already exists
			DebugBreakpoint breakpoint = breakpoints.Where(bp => bp.Address == address).FirstOrDefault();
			if (breakpoint == null) return;

			// First read instruction at breakpoint location
			ReadMemoryResponse response = engine.SendAndReceive<ReadMemoryResponse>(new ReadMemoryRequest(address, 1));
			byte instruction = response.Memory[0];

			// Overwrite memory with BRK (0x00) command
			engine.SendAndWait(new WriteMemoryRequest(address, 0, new byte[] { 0x00 }));

			// Store breakpoint for later
			breakpoint = new DebugBreakpoint(address, instruction);
			breakpoints.Add(breakpoint);
		}

		public void ClearBreakpoint(ushort address)
		{
			// Search for breakpoint at specified address
			DebugBreakpoint breakpoint = FindBreakpoint(address);
			if (breakpoint == null) return;

			// Restore instruction at memory location
			WriteMemoryRequest request = 
				new WriteMemoryRequest(breakpoint.Address, 1, new byte[] { breakpoint.Code });
			engine.SendAndWait(request);
		}

		public DebugBreakpoint FindBreakpoint(ushort address)
		{
			return breakpoints.Where(bp => bp.Address == address).FirstOrDefault();
		}
	}
}
