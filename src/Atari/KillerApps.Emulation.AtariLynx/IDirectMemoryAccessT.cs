using KillerApps.Emulation.Processors;
using System;
using System.Collections.Generic;
using System.Text;

namespace KillerApps.Emulation.AtariLynx
{
	public interface IDirectMemoryAccess<TAddress, TData> : IMemoryAccess<TAddress, TData>
	{
		TData[] GetDirectAccess();
	}
}
