using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KillerApps.Emulation.Processors.Tests
{
	[TestClass]
	public class FlagsInstructionsTest
	{
		Cmos65SC02 cpu = null;
		byte[] memory = null;

		private const int programStart = 0x0200;
		private const int relativeBranch = 0x05;

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		[TestInitialize()]
		public void MyTestInitialize() 
		{
			Ram64KBMemoryStub ram = new Ram64KBMemoryStub();
			cpu = new Cmos65SC02(ram, new Clock());
			memory = ram.GetDirectAccess();
			cpu.Reset();
		}

		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		private void InitializeMemory(byte[] instructions)
		{
			Array.Copy(instructions, 0, memory, programStart, instructions.Length);
			cpu.PC = programStart;
		}

		[TestMethod]
		public void SECShouldSetCarryFlag()
		{
			cpu.C = false;
			byte[] instructions = new byte[] 
				{
					0x38
				};
			InitializeMemory(instructions);

			cpu.Execute(1);

			Assert.IsTrue(cpu.C, "After SEC the Carry flag should be set.");
		}

		[TestMethod]
		public void SEIShouldSetInterruptDisableFlag()
		{
			cpu.I = false;
			byte[] instructions = new byte[] 
				{
					0x78
				};
			InitializeMemory(instructions);

			cpu.Execute(1);

			Assert.IsTrue(cpu.I, "After SEI the Interrupt Disable flag should be set.");
		}

		[TestMethod]
		public void SEDShouldSetDecimalFlag()
		{
			cpu.D = false;
			byte[] instructions = new byte[] 
				{
					0xF8 // SED
				};
			InitializeMemory(instructions);

			// Act
			cpu.Execute(1);

			Assert.IsTrue(cpu.D, "After SED the decimal flag should be set.");
		}

		[TestMethod]
		public void CLCShouldClearCarryFlag()
		{
			cpu.C = true;
			byte[] instructions = new byte[] 
				{
					0x18 // CLC
				};
			InitializeMemory(instructions);

			// Act
			cpu.Execute(1);

			Assert.IsFalse(cpu.C, "After CLC the carry flag should be cleared.");
		}

		[TestMethod]
		public void CLDShouldClearDecimalModeFlag()
		{
			cpu.D = true;
			byte[] instructions = new byte[] 
				{
					0xD8 // CLD
				};
			InitializeMemory(instructions);

			// Act
			cpu.Execute(1);

			Assert.IsFalse(cpu.C, "After CLD the decimal mode flag should be cleared.");
		}

		[TestMethod]
		public void CLIShouldClearInterruptDisableFlag()
		{
			cpu.I = true;
			byte[] instructions = new byte[] 
				{
					0x58 // CLI
				};
			InitializeMemory(instructions);

			// Act
			cpu.Execute(1);

			Assert.IsFalse(cpu.I, "After CLI the interrupt disable flag should be cleared.");
		}

		[TestMethod]
		public void CLVShouldClearOverflowFlag()
		{
			cpu.V = true;
			byte[] instructions = new byte[] 
				{
					0xB8 // CLV
				};
			InitializeMemory(instructions);

			// Act
			cpu.Execute(1);

			Assert.IsFalse(cpu.V, "After CLV the overflow flag should be cleared.");
		}
	}
}
