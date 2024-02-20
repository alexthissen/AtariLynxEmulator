namespace KillerApps.Emulation.Processors
{
	public interface IMemoryAccess<TAddress, TData>
	{
		void Poke(TAddress address, TData value);
		TData Peek(TAddress address);
	}
}
