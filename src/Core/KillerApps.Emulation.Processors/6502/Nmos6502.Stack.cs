namespace KillerApps.Emulation.Processors
{
	public partial class Nmos6502
	{
		/// <summary>
		/// PusH Accumulator
		/// </summary>
		/// <remarks>
		/// Pushes a copy of the accumulator on to the stack.
		/// </remarks>
		public void PHA()
		{
			PushOnStack(A);
		}

		/// <summary>
		/// PusH Processor status
		/// </summary>
		/// <remarks>
		/// Pushes a copy of the status flags on to the stack.
		/// </remarks>
		public void PHP()
		{
			PushOnStack(ProcessorStatus);
		}

		/// <summary>
		/// PusH Y register
		/// </summary>
		/// <remarks>
		/// Pushes a copy of the Y register on to the stack.
		/// </remarks>
		public void PHY()
		{
			PushOnStack(Y);
		}

		/// <summary>
		/// PuLl Accumulator
		/// </summary>
		/// <remarks>
		/// Pulls an 8 bit value from the stack and into the accumulator. The zero and negative 
		/// flags are set as appropriate.
		/// </remarks>
		public void PLA()
		{
			A = PullFromStack();
			UpdateNegativeZeroFlags(A);
		}

		/// <summary>
		/// PuLl Processor status
		/// </summary>
		/// <remarks>
		/// Pulls an 8 bit value from the stack and into the processor flags. The flags will 
		/// take on new states as determined by the value pulled.
		/// </remarks>
		public void PLP()
		{
			ProcessorStatus = PullFromStack();
		}

		/// <summary>
		/// PuLl Y register
		/// </summary>
		/// <remarks>
		/// Pulls an 8 bit value from the stack and into the Y register. The zero and negative 
		/// flags are set as appropriate.
		/// </remarks>
		public void PLY()
		{
			Y = PullFromStack();
			UpdateNegativeZeroFlags(Y);
		}
	}
}
