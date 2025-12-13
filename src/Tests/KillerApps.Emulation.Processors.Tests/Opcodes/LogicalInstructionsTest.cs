using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace KillerApps.Emulation.Processors.Tests;

[TestClass]
public class LogicalInstructionsTest
{
	Cmos65SC02 cpu = null;
	byte[] memory = null;

	private const int programStart = 0x0200;
	private const int relativeBranch = 0x05;

	[TestInitialize()]
	public void MyTestInitialize() 
	{
		Ram64KBMemoryStub ram = new Ram64KBMemoryStub();
		cpu = new Cmos65SC02(ram, new Clock());
		memory = ram.GetDirectAccess();
		cpu.Reset();
	}

	private void InitializeMemory(byte[] instructions)
	{
		Array.Copy(instructions, 0, memory, programStart, instructions.Length);
		cpu.PC = programStart;
	}

	[TestMethod]
	public void BITShouldSetZeroFlagWhenANDIsZero()
	{
		// Arrange
		cpu.A = 0x00;
		memory[0x13] = 0xff; // Set zero page memory
		byte[] instructions = new byte[]
			{
				0x24, 0x13 // BIT $13
			};
		InitializeMemory(instructions);

		// Act 
		cpu.Execute(1);

		// Assert
		Assert.IsTrue(cpu.Z, "Zero flag should be set with BIT if AND is zero.");
	}

	[TestMethod]
	public void BITShouldSetVFlagWhenBit6IsSet()
	{
		// Arrange
		cpu.A = 0xff;
		memory[0x13] = 0x40; // Set zero page memory
		byte[] instructions = new byte[]
			{
				0x24, 0x13 // BIT $13
			};
		InitializeMemory(instructions);

		// Act 
		cpu.Execute(1);

		// Assert
		Assert.IsTrue(cpu.V, "Overflow flag should be set with bit 6 is set.");
	}

	[TestMethod]
	public void BITShouldSetNFlagWhenBit7IsSet()
	{
		// 
		cpu.A = 0xff;
		memory[0x13] = 0x80; // Set zero page memory
		byte[] instructions = new byte[]
			{
				0x24, 0x13 // BIT $13
			};
		InitializeMemory(instructions);

		// Act 
		cpu.Execute(1);

		// Assert
		Assert.IsTrue(cpu.N, "Negative flag should be set with bit 7 is set.");
	}

	[TestMethod]
	public void ANDShouldPerformLogicalAndWithAccumulator()
	{
		cpu.A = 0xac;
		byte[] instructions = new byte[]
			{
				0x29, 0xc6 // AND #$0f
			};
		InitializeMemory(instructions);

		//Act
		cpu.Execute(1);

		// Assert
		Assert.AreEqual<int>(0x84, cpu.A, "Accumulator should contain logical AND value of accumulator and memory.");
		Assert.IsTrue(cpu.N, "Negative flag should be set if bit 7 is true.");
	}
}
