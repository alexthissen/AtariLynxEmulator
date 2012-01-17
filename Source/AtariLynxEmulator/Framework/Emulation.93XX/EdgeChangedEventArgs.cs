using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Eeproms
{
	public class EdgeChangedEventArgs : EventArgs
	{
		public EdgeChangedEventArgs(VoltageEdge edge)
		{
			this.Edge = edge;
		}

		public VoltageEdge Edge { get; private set; }
	}
}
