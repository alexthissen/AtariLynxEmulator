namespace KillerApps.Emulation.Processors
{
	public abstract class ProcessorBase
	{
		public abstract void Initialize();
		public abstract void Reset();
		public abstract ulong Execute(int instructionsToExecute);
		public abstract object SignalInterrupt(InterruptType interrupt, params object[] details);
	}
}
