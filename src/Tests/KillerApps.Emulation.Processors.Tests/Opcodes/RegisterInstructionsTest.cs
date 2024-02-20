using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace KillerApps.Emulation.Processors.Tests;

[TestClass]
public class RegisterInstructionsTest
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
	public void LDAShouldLoadValueInAccumulator()
	{
		int value = 0x00;
		cpu.A = 0x42;
		byte[] instructions = new byte[]
			{
				0xA9, (byte)value // LDA #$00
			};
		InitializeMemory(instructions);

		// Act 
		cpu.Execute(1);

		// Assert
		Assert.AreEqual<int>(value, cpu.A, "Accumulator should contain immediate value after LDA.");
		Assert.IsTrue(cpu.Z, "Zero flag should be set when loading zero value after LDA.");
	}

	[TestMethod]
	public void LDAShouldSetNegativeFlagWhenLoadingNegativeValue()
	{
		int value = 0x80;
		cpu.A = 0x42;
		byte[] instructions = new byte[]
			{
				0xA9, (byte)value // LDA #$80
			};
		InitializeMemory(instructions);

		// Act 
		cpu.Execute(1);

		// Assert
		Assert.AreEqual<int>(value, cpu.A, "Accumulator should contain immediate value after LDA.");
		Assert.IsTrue(cpu.N, "When loading negative value the Negative flag should be set after LDA.");
	}

	[TestMethod]
	public void LDXShouldLoadValueInXRegister()
	{
		int value = 0x00;
		cpu.X = 0x42;
		byte[] instructions = new byte[]
			{
				0xA2, (byte)value // LDX #$00
			};
		InitializeMemory(instructions);

		// Act 
		cpu.Execute(1);

		// Assert
		Assert.AreEqual<int>(value, cpu.X, "X register should contain immediate value after LDX.");
		Assert.IsTrue(cpu.Z, "Zero flag should be set when loading zero value after LDX.");
	}

	[TestMethod]
	public void LDXShouldSetNegativeFlagWhenLoadingNegativeValue()
	{
		int value = 0x80;
		cpu.X = 0x42;
		byte[] instructions = new byte[]
			{
				0xA2, (byte)value // LDX #$80
			};
		InitializeMemory(instructions);

		// Act 
		cpu.Execute(1);

		// Assert
		Assert.AreEqual<int>(value, cpu.X, "X register should contain immediate value after LDX.");
		Assert.IsTrue(cpu.N, "When loading negative value the Negative flag should be set after LDX.");
	}

	[TestMethod]
	public void LDYShouldLoadValueInYRegister()
	{
		int value = 0x00;
		cpu.Y = 0x42;
		byte[] instructions = new byte[]
			{
				0xA0, (byte)value // LDY #$00
			};
		InitializeMemory(instructions);

		// Act 
		cpu.Execute(1);

		// Assert
		Assert.AreEqual<int>(value, cpu.Y, "Y register should contain immediate value after LDY.");
		Assert.IsTrue(cpu.Z, "Zero flag should be set when loading zero value after LDY.");
	}

	[TestMethod]
	public void LDYShouldSetNegativeFlagWhenLoadingNegativeValue()
	{
		int value = 0x80;
		cpu.Y = 0x42;
		byte[] instructions = new byte[]
			{
				0xA0, (byte)value // LDY #$80
			};
		InitializeMemory(instructions);

		// Act 
		cpu.Execute(1);

		// Assert
		Assert.AreEqual<int>(value, cpu.Y, "Y register should contain immediate value after LDY.");
		Assert.IsTrue(cpu.N, "When loading negative value the Negative flag should be set after LDY.");
	}

	public void TAXShouldTransferAccumulatorToXRegister()
	{
		byte value = 0xff;
		cpu.A = value;
		byte[] instructions = new byte[]
			{
				0xAA // TAX
			};
		InitializeMemory(instructions);

		// Act
		cpu.Execute(1);

		// Assert
		Assert.AreEqual<int>(value, cpu.X, "X register should contain value of accumulator after TAX.");
		Assert.IsTrue(cpu.N, "Negative flag should be set for negative value in X register after TAX.");
	}

	public void TAXShouldSetZeroFlagForZeroValue()
	{
		byte value = 0x00;
		cpu.A = value;
		byte[] instructions = new byte[]
			{
				0xAA // TAX
			};
		InitializeMemory(instructions);

		// Act
		cpu.Execute(1);

		// Assert
		Assert.IsTrue(cpu.Z, "Zero flag should be set for zero value in X register after TAX.");
	}

	public void TXAShouldTransferXRegisterToAccumulator()
	{
		byte value = 0xff;
		cpu.X = value;
		byte[] instructions = new byte[]
			{
				0x8A // TXA
			};
		InitializeMemory(instructions);

		// Act
		cpu.Execute(1);

		// Assert
		Assert.AreEqual<int>(value, cpu.X, "Accumulator should contain value of X register after TXA.");
		Assert.IsTrue(cpu.N, "Negative flag should be set for negative value in accumulator after TXA.");
	}

	public void TXAShouldSetZeroFlagForZeroValue()
	{
		byte value = 0x00;
		cpu.X = value;
		byte[] instructions = new byte[]
			{
				0x8A // TAX
			};
		InitializeMemory(instructions);

		// Act
		cpu.Execute(1);

		// Assert
		Assert.IsTrue(cpu.Z, "Zero flag should be set for zero value in accumulator after TXA.");
	}

	public void TAYShouldTransferAccumulatorToYRegister()
	{
		byte value = 0xff;
		cpu.A = value;
		byte[] instructions = new byte[]
			{
				0xA8 // TAY
			};
		InitializeMemory(instructions);

		// Act
		cpu.Execute(1);

		// Assert
		Assert.AreEqual<int>(value, cpu.Y, "Y register should contain value of accumulator after TAY.");
		Assert.IsTrue(cpu.N, "Negative flag should be set for negative value in Y register after TAY.");
	}

	public void TAYShouldSetZeroFlagForZeroValue()
	{
		byte value = 0x00;
		cpu.A = value;
		byte[] instructions = new byte[]
			{
				0xA8 // TAY
			};
		InitializeMemory(instructions);

		// Act
		cpu.Execute(1);

		// Assert
		Assert.IsTrue(cpu.Z, "Zero flag should be set for zero value in Y register after TAY.");
	}

	public void TYAShouldTransferYRegisterToAccumulator()
	{
		byte value = 0xff;
		cpu.Y = value;
		byte[] instructions = new byte[]
			{
				0x98 // TYA
			};
		InitializeMemory(instructions);

		// Act
		cpu.Execute(1);

		// Assert
		Assert.AreEqual<int>(value, cpu.Y, "Accumulator should contain value of Y register after TYA.");
		Assert.IsTrue(cpu.N, "Negative flag should be set for negative value in accumulator after TYA.");
	}

	public void TYAShouldSetZeroFlagForZeroValue()
	{
		byte value = 0x00;
		cpu.Y = value;
		byte[] instructions = new byte[]
			{
				0x98 // TYA
			};
		InitializeMemory(instructions);

		// Act
		cpu.Execute(1);

		// Assert
		Assert.IsTrue(cpu.Z, "Zero flag should be set for zero value in accumulator after TYA.");
	}
}
