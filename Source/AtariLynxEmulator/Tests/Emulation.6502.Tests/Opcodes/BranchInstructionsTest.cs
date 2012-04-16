using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KillerApps.Emulation.Processors.Tests
{
	[TestClass]
	public class BranchInstructionsTest
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
		public void BNEWithZeroFlagClearShouldBranchRelative()
		{
			// Clear zero flag to force branch
			cpu.Z = false;
			byte[] instructions = new byte[]
				{
					0xD0, relativeBranch // BNE #$05
				};
			InitializeMemory(instructions);

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(programStart + relativeBranch + 2, cpu.PC, "Program counter should be increased with relative jump for BNE with zero flag clear.");
		}

		[TestMethod]
		public void BNEWithZeroFlagSetShouldNotBranch()
		{
			// Set zero flag to avoid branch
			cpu.Z = true;
			byte[] instructions = new byte[]
				{
					0xD0, relativeBranch // BNE #$05
				};
			InitializeMemory(instructions);

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(programStart + 2, cpu.PC, "Program counter should not be increased with relative jump for BNE with zero flag set.");
		}

		[TestMethod]
		public void BEQWithZeroFlagClearShouldBranchRelative()
		{
			// Set zero flag to force branch
			cpu.Z = true;
			byte[] instructions = new byte[]
				{
					0xF0, relativeBranch // BEQ #$05
				};
			InitializeMemory(instructions);

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(programStart + relativeBranch + 2, cpu.PC, "Program counter should be increased with relative jump after BEQ with zero flag set.");
		}

		[TestMethod]
		public void BEQWithZeroFlagClearShouldNotBranch()
		{
			// Clear zero flag to avoid branch
			cpu.Z = false;
			byte[] instructions = new byte[]
				{
					0xF0, relativeBranch // BEQ #$05
				};
			InitializeMemory(instructions);

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(programStart + 2, cpu.PC, "Program counter should not be increased with relative jump for BEQ with zero flag clear.");
		}

		[TestMethod]
		public void BCCWithCarryFlagClearShouldBranchRelative()
		{
			// Clear carry flag to force branch
			cpu.C = false;
			byte[] instructions = new byte[]
				{
					0x90, relativeBranch // BCC #$05
				};
			InitializeMemory(instructions);

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(programStart + relativeBranch + 2, cpu.PC, 
				"Program counter should be increased with relative jump after BCC with carry flag clear.");
		}

		[TestMethod]
		public void BCCWithCarryFlagSetShouldNotBranch()
		{
			// Set carry flag to avoid branch
			cpu.C = true;
			byte[] instructions = new byte[]
				{
					0x90, relativeBranch // BCC #$05
				};
			InitializeMemory(instructions);

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(programStart + 2, cpu.PC, "Program counter should not be increased with relative jump for BCC with carry flag set.");
		}

		[TestMethod]
		public void BCSWithCarryFlagSetShouldBranchRelative()
		{
			// Set carry flag to force branch
			cpu.C = true;
			byte[] instructions = new byte[]
				{
					0xB0, relativeBranch // BCS #$05
				};
			InitializeMemory(instructions);

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(programStart + relativeBranch + 2, cpu.PC, "Program counter should be increased with relative jump after BCS with carry flag set.");
		}

		[TestMethod]
		public void BCSWithCarryFlagClearShouldNotBranch()
		{
			// Clear carry flag to avoid branch
			cpu.C = false;
			byte[] instructions = new byte[]
				{
					0xB0, relativeBranch // BCS #$05
				};
			InitializeMemory(instructions);

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(programStart + 2, cpu.PC, "Program counter should not be increased with relative jump for BCS with carry flag clear.");
		}

		[TestMethod]
		public void BMIWithNegativeFlagSetShouldBranchRelative()
		{
			// Set negative flag to force branch
			cpu.N = true;
			byte[] instructions = new byte[]
				{
					0x30, relativeBranch // BMI #$05
				};
			InitializeMemory(instructions);

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(programStart + relativeBranch + 2, cpu.PC, "Program counter should be increased with relative jump after BMI with negative flag set.");
		}

		[TestMethod]
		public void BMIWithNegativeFlagClearShouldNotBranch()
		{
			// Clear negative flag to avoid branch
			cpu.N = false;
			byte[] instructions = new byte[]
				{
					0x30, relativeBranch // BMI #$05
				};
			InitializeMemory(instructions);

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(programStart + 2, cpu.PC, "Program counter should not be increased with relative jump for BMI with negative flag clear.");
		}

		[TestMethod]
		public void BPLWithNegativeFlagSetShouldNotBranch()
		{
			// Set negative flag to avoid branch
			cpu.N = true;
			byte[] instructions = new byte[]
				{
					0x10, relativeBranch // BPL #$05
				};
			InitializeMemory(instructions);

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(programStart + 2, cpu.PC, "Program counter should not be increased with relative jump for BPL with negative flag set.");
		}

		[TestMethod]
		public void BPLWithNegativeFlagClearShouldBranchRelative()
		{
			// Set negative flag to force branch
			cpu.N = false;
			byte[] instructions = new byte[]
				{
					0x10, relativeBranch // BPL #$05
				};
			InitializeMemory(instructions);

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(programStart + relativeBranch + 2, cpu.PC, "Program counter should be increased with relative jump after BPL with negative flag clear.");
		}

		[TestMethod]
		public void BVCWithOverflowFlagSetShouldNotBranch()
		{
			// Set overflow flag to avoid branch
			cpu.V = true;
			byte[] instructions = new byte[]
				{
					0x50, relativeBranch // BVC #$05
				};
			InitializeMemory(instructions);

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(programStart + 2, cpu.PC, "Program counter should not be increased with relative jump for BVC with overflow flag set.");
		}

		[TestMethod]
		public void BVCWithOverflowFlagClearShouldBranchRelative()
		{
			// Clear overflow flag to force branch
			cpu.V = false;
			byte[] instructions = new byte[]
				{
					0x50, relativeBranch // BVC #$05
				};
			InitializeMemory(instructions);

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(programStart + relativeBranch + 2, cpu.PC, "Program counter should be increased with relative jump after BVC with overflow flag clear.");
		}

		[TestMethod]
		public void BVSWithOverflowFlagClearShouldNotBranch()
		{
			// Clear overflow flag to avoid branch
			cpu.V = false;
			byte[] instructions = new byte[]
				{
					0x70, relativeBranch // BVS #$05
				};
			InitializeMemory(instructions);

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(programStart + 2, cpu.PC, "Program counter should not be increased with relative jump for BVS with overflow flag clear.");
		}

		[TestMethod]
		public void BVSWithOverflowFlagSetShouldBranchRelative()
		{
			// Set overflow flag to force branch
			cpu.V = true;
			byte[] instructions = new byte[]
				{
					0x70, relativeBranch // BVS #$05
				};
			InitializeMemory(instructions);

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(programStart + relativeBranch + 2, cpu.PC, "Program counter should be increased with relative jump after BVS with overflow flag set.");
		}

		[TestMethod]
		public void BRAShouldBranchRelative()
		{
			byte[] instructions = new byte[]
				{
					0x80, relativeBranch // BRA #$05
				};
			InitializeMemory(instructions);

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(programStart + relativeBranch + 2, cpu.PC, "Program counter should be increased with relative jump after BRA.");
		}
	}
}
