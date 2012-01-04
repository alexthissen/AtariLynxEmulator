using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Core
{
	public interface IDirectMemoryAccess<TAddress, TData> : IMemoryAccess<TAddress, TData>
	{
		TData[] GetDirectAccess();
	}
}
