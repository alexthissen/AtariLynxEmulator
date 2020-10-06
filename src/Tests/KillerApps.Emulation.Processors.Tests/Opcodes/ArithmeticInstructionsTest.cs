using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KillerApps.Emulation.Processors.Tests
{
	[TestClass]
	public class ArithmeticInstructionsTest
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
		public void ADCShouldAddWithCarryValue()
		{
			cpu.A = 0x10;
			cpu.C = true;
			byte[] instructions = new byte[]
				{
					0x69, 0x20 // ADC #$20
				};
			InitializeMemory(instructions);

			//Act
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<byte>(0x31, cpu.A, "Accumulator should contain original value + memory + carry flag (if set).");
			Assert.IsFalse(cpu.C, "Carry flag should only be set by ADC if overflow in bit 7.");
		}

		[TestMethod]
		public void ADCShouldAddCorrectlyForDecimal()
		{
			cpu.A = 0x09;
			cpu.C = true;
			cpu.D = true;
			byte[] instructions = new byte[]
				{
					0x69, 0x02 // ADC #$20
				};
			InitializeMemory(instructions);

			//Act
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<byte>(0x12, cpu.A, "Accumulator should contain original value + memory + carry flag (if set).");
			Assert.IsFalse(cpu.C, "Carry flag should only be set by ADC if overflow in bit 7.");			
		}

		[TestMethod]
		public void ADCShouldOverflowCorrectlyForDecimal()
		{
			cpu.A = 0x90;
			cpu.C = false;
			cpu.D = true;
			byte[] instructions = new byte[]
				{
					0x69, 0x20 // ADC #$20
				};
			InitializeMemory(instructions);

			//Act
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<byte>(0x10, cpu.A, "Accumulator should contain original value + memory + carry flag (if set).");
			Assert.IsTrue(cpu.C, "Carry flag should only be set by ADC if overflow in bit 7.");
		}

		[TestMethod]
		public void ADCShouldSetZeroIfAccumulatorZero()
		{
			cpu.A = 0x00;
			cpu.Z = false;
			byte[] instructions = new byte[]
				{
					0x69, 0x00 // ADC #$00
				};
			InitializeMemory(instructions);

			//Act
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<byte>(0x00, cpu.A, "Accumulator should contain original value + memory + carry flag (if set).");
			Assert.IsTrue(cpu.Z, "Zero flag should be set by ADC if accumulator is zero after addition.");
		}

		[TestMethod]
		public void ADCShouldSetOverflowIfSignIsIncorrect()
		{
			cpu.A = 0x7f;
			cpu.Z = false;
			byte[] instructions = new byte[]
				{
					0x69, 0x01 // ADC #$01
				};
			InitializeMemory(instructions);

			//Act
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<byte>(0x80, cpu.A, "Accumulator should contain original value + memory + carry flag (if set).");
			Assert.IsTrue(cpu.V, "Overflow flag should be set by ADC if sign bit is incorrect.");
		}

		[TestMethod]
		public void ADCShouldSetCarryIfOverflow()
		{
			cpu.A = 0xc0;
			cpu.C = false;
			cpu.Z = false;
			byte[] instructions = new byte[]
				{
					0x69, 0x40 // ADC #$40
				};
			InitializeMemory(instructions);

			//Act
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<byte>(0x00, cpu.A, "Accumulator should contain original value + memory + carry flag (if set)");
			Assert.IsTrue(cpu.C, "Carry flag should be set by ADC if overflow occurs after addition.");
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
			Assert.AreEqual<byte>(0x84, cpu.A, "Accumulator should contain logical AND value of accumulator and memory.");
			Assert.IsTrue(cpu.N, "Negative flag should be set if bit 7 is true.");
		}
	}
}
