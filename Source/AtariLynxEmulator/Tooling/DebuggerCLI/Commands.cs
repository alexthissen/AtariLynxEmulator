using ConsoleCommandApi;
using KillerApps.Emulation.Atari.Lynx.Debugger;
using KillerApps.Emulation.Processors;
using KillerApps.Emulation.Tooling.Disassembler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebuggerCLI
{
	public class Commands
	{
		[ConsoleCommand("go", Description = "Continue")]
		public static void Go()
		{
			Logger.Current.Info("Continue");
			Program.Engine.Continue();
		}

		[ConsoleCommand("reg")]
		public static void SetRegister(
			[ParameterDescription("register", "Name of register", Alias = "r")]
			string register,
			[ParameterDescription("value", "Value of register", Alias = "v")]
			byte value
		)
		{
		}

		[ConsoleCommand("mem")]
		public static void GetMemory(
			[ParameterDescription("address", "Start address of memory", Alias = "a")]
			ushort address,
			[ParameterDescription("size", "Number of bytes to read", Alias = "s")]
			ushort size)
		{
			Logger.Current.Info("Read memory");
			byte[] memory = Program.Engine.InspectMemory(address, size);

			for (int index = 0; index < memory.Length; index++)
			{
				Console.Write("{0:X2} ", memory[index]);
				if ((index % 16) == 15) Console.WriteLine();
			}
			Console.WriteLine();

			byte[] bytes = new byte[UInt16.MaxValue + 1];
			Array.Copy(memory, 0, bytes, address, memory.Length);
			RamMemory ram = new RamMemory(bytes);
			foreach (Expression6502 expression in ram.Disassemble(address).Take(10))
			{
				Console.WriteLine(expression);
			}
		}
	}
}
