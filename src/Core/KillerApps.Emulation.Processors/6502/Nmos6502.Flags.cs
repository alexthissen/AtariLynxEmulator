namespace KillerApps.Emulation.Processors
{
	public partial class Nmos6502
	{
		/// <summary>
		/// Clear Carry
		/// </summary>
		public void CLC()
		{
			C = false;
		}

		/// <summary>
		/// CLear Decimal
		/// </summary>
		public void CLD()
		{
			D = false;
		}

		/// <summary>
		/// CLear Interrupt disable flag
		/// </summary>
		public void CLI()
		{
			I = false;
		}

		/// <summary>
		/// CLear oVerflow
		/// </summary>
		public void CLV()
		{
			V = false;
		}

		/// <summary>
		/// SEt Carry flag
		/// </summary>
		/// <remarks>
		/// Set the carry flag to one.
		/// </remarks>
		public void SEC()
		{
			C = true;
		}

		/// <summary>
		/// SEt Decimal flag
		/// </summary>
		/// <remarks>
		/// Set the decimal mode flag to one.
		/// </remarks>
		public void SED()
		{
			D = true;
		}

		/// <summary>
		/// SEt Interrupt disable
		/// </summary>
		/// <remarks>
		/// Set the interrupt disable flag to one.
		/// </remarks>
		public void SEI()
		{
			I = true;
		}
	}
}
