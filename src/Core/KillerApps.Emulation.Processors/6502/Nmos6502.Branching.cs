﻿namespace KillerApps.Emulation.Processors
{
	public partial class Nmos6502
	{
		/// <summary>
		/// Branch if Carry Clear
		/// </summary>
		/// <remarks>
		/// If the carry flag is clear then add the relative displacement to the program counter to cause a branch to a new location.
		/// </remarks>
		public void BCC()
		{
			if (!C)
			{
				sbyte offset = (sbyte)Memory.Peek(PC);
				//SystemClock.CycleCount += MemoryReadCycle;
				PC++;
				PC = (ushort)(PC + offset);
				//Debug.WriteLineIf(GeneralSwitch.TraceInfo, String.Format("BCC taking branch to ${0:X4}", PC));
			}
			else
			{
				PC++;
			}
			SystemClock.CycleCount++;
		}

		/// <summary>
		/// Branch if Carry Set
		/// </summary>
		/// <remarks>
		/// If the carry flag is set then add the relative displacement to the program counter to 
		/// cause a branch to a new location.
		/// </remarks>
		public void BCS()
		{
			if (C)
			{
				sbyte offset = (sbyte)Memory.Peek(PC);
				//SystemClock.CycleCount += MemoryReadCycle;
				PC++;
				PC = (ushort)(PC + offset);
				//Debug.WriteLineIf(GeneralSwitch.TraceInfo, String.Format("BCS taking branch to ${0:X4}", PC));
			}
			else
			{
				PC++;
			}
			SystemClock.CycleCount++;
		}

		/// <summary>
		/// Branch if Equal
		/// </summary>
		/// <remarks>
		/// If the zero flag is set then add the relative displacement to the program counter to cause a branch to a new location.
		/// </remarks>
		public void BEQ()
		{
			if (Z)
			{
				sbyte offset = (sbyte)Memory.Peek(PC);
				//SystemClock.CycleCount += MemoryReadCycle;
				PC++;
				PC = (ushort)(PC + offset);
				//Debug.WriteLineIf(GeneralSwitch.TraceInfo, String.Format("BEQ taking branch to ${0:X4}", PC));
			}
			else
			{
				PC++;
			}
			SystemClock.CycleCount++;
		}

		/// <summary>
		/// Branch on MInus
		/// </summary>
		public void BMI()
		{
			if (N)
			{
				sbyte offset = (sbyte)Memory.Peek(PC);
				//SystemClock.CycleCount += MemoryReadCycle;
				PC++;
				PC = (ushort)(PC + offset);
				//Debug.WriteLineIf(GeneralSwitch.TraceInfo, String.Format("BMI taking branch to ${0:X4}", PC));
			}
			else
			{
				PC++;
			}
			SystemClock.CycleCount++;
		}

		/// <summary>
		/// Branch if Not Equal
		/// </summary>
		/// <remarks>
		/// If the zero flag is clear then add the relative displacement to the program counter to cause a branch to a new location.
		/// </remarks>
		public void BNE()
		{
			if (!Z)
			{
				//SystemClock.CycleCount += MemoryReadCycle;
				sbyte offset = (sbyte)Memory.Peek(PC);
				PC++;
				PC = (ushort)(PC + offset);
				//Debug.WriteLineIf(GeneralSwitch.TraceInfo, String.Format("BNE taking branch to ${0:X4}", PC));
			}
			else
			{
				PC++;
			}
			SystemClock.CycleCount++;
		}

		/// <summary>
		/// Branch on PLus
		/// </summary>
		/// <remarks>
		/// If the negative flag is clear then add the relative displacement to the program 
		/// counter to cause a branch to a new location.
		/// </remarks>
		public void BPL()
		{
			if (!N)
			{
				sbyte offset = (sbyte)Memory.Peek(PC);
				PC++;
				PC = (ushort)(PC + offset);
			}
			else
			{
				PC++;
			}
			SystemClock.CycleCount++;
		}

		/// <summary>
		/// Branch on oVerflow Clear
		/// </summary>
		/// <remarks>
		/// If the overflow flag is clear then add the relative displacement to the program 
		/// counter to cause a branch to a new location.
		/// </remarks>
		public void BVC()
		{
			if (!V)
			{
				sbyte offset = (sbyte)Memory.Peek(PC);
				//SystemClock.CycleCount += MemoryReadCycle;
				PC++;
				PC = (ushort)(PC + offset);
				//Debug.WriteLineIf(GeneralSwitch.TraceInfo, String.Format("BVC taking branch to ${0:X4}", PC));
			}
			else
			{
				PC++;
			}
			SystemClock.CycleCount++;
		}

		/// <summary>
		/// Branch on oVerflow Set
		/// </summary>
		/// <remarks>
		/// If the overflow flag is set then add the relative displacement to the program 
		/// counter to cause a branch to a new location.
		/// </remarks>
		public void BVS()
		{
			if (V)
			{
				sbyte offset = (sbyte)Memory.Peek(PC);
				//SystemClock.CycleCount += MemoryReadCycle;
				PC++;
				PC = (ushort)(PC + offset);
				//Debug.WriteLineIf(GeneralSwitch.TraceInfo, String.Format("BVS taking branch to ${0:X4}", PC));
			}
			else
			{
				PC++;
			}
			SystemClock.CycleCount++;
		}
	}
}
