using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace KillerApps.Emulation.Processors.Tests;

[TestClass]
public class LogicalInstructionsTest
{
	private TestContext testContextInstance;
	Cmos65SC02 cpu = null;
	byte[] memory = null;

	private const int programStart = 0x0200;
	private const int relativeBranch = 0x05;

	/// <summary>
	///Gets or sets the test context which provides
	///information about and functionality for the current test run.
	///</summary>
	public TestContext TestContext
	{
		get
		{
			return testContextInstance;
		}
		set
		{
			testContextInstance = value;
		}
	}

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
