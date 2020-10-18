using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KillerApps.Emulation.Processors.Tests
{
	[TestClass]
	public class ShiftInstructionsTest
	{
		Cmos65SC02 cpu = null;
		byte[] memory = null;

		private const int programStart = 0x0200;

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
		public void ASLShouldShiftLeftAllBitsAndSetLastBitToZero()
		{
			cpu.A = 0x85;
			byte[] instructions = new byte[]
				{
					0x0a // ASLA
				};
			InitializeMemory(instructions);

			// Act
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>((0x85 << 1) & 0xff, cpu.A, "Accumulator should contain left shifted value.");
			Assert.IsTrue((cpu.A % 2) == 0, "Bit 0 should always contain zero.");
		}

		[TestMethod]
		public void ASLShouldSetCarryFlagIfBit7IsShiftedLeft()
		{
			cpu.A = 0x80; // Binary 1000 0000
			byte[] instructions = new byte[]
				{
					0x0a // ASLA
				};
			InitializeMemory(instructions);

			// Act
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(0x00 << 1, cpu.A, "Accumulator should contain zero if bit 7 is shifted left.");
			Assert.IsTrue(cpu.C, "Carry flag should be set when bit 7 is shifted left.");
			Assert.IsTrue(cpu.Z, "Zero flag should be set if result is zero.");
		}
	}
}
