using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KillerApps.Emulation.Processors.Tests
{
	[TestClass]
	public class SystemFunctionsTest
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
		public void BRKShouldGenerateInterrupt()
		{
			cpu.PC = 0x0201; // Use different address than 0x0200, because high and low byte of address would be same 0x0202
			cpu.D = true;
			byte programStatus = cpu.ProcessorStatus;
			memory[0x0201] = 0x00; // BRK instruction
			memory[VectorAddresses.IRQ_VECTOR] = 0x12; // Low bit of IRQ vector
			memory[VectorAddresses.IRQ_VECTOR + 1] = 0x34; // High bit of IRQ vector

			// Act
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<ushort>(0x3412, cpu.PC, "After interrupt the Program Counter should be equal to address stored in IRQ vector.");
			Assert.AreEqual<byte>(0x02, memory[0x01ff], "High byte of program counter should be on stack first.");
			Assert.AreEqual<byte>(0x03, memory[0x01fe], "Low byte of program counter should be on stack next.");
			Assert.AreEqual<byte>((byte)(programStatus | 0x10), memory[0x01fd], "Value on stack should be original program status with break flag set.");
			Assert.IsTrue(cpu.I, "Interrupt disable flag should be set.");
			Assert.IsFalse(cpu.D, "Decimal flag should be cleared.");
		}

		[TestMethod]
		public void RTIShouldRestorePreInterruptState()
		{
			cpu.PC = 0x0201; // Use different address than 0x0200, because high and low byte of address would be same 0x0202
			memory[0x0201] = 0x40; // RTI instruction
			byte processorStatus = 0x72;
			cpu.PushOnStack(0x13);
			cpu.PushOnStack(0x37);
			cpu.PushOnStack(processorStatus);

			// Act
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<ushort>(0x1337, cpu.PC, "Program counter is not restored to correct address.");
			Assert.AreEqual<byte>(processorStatus, cpu.ProcessorStatus, "Processor status is not restored.");
		}

		[TestMethod]
		public void NOPShouldPerformNoActions()
		{
			byte[] instructions = new byte[]
				{
					0xea // NOP
				};
			InitializeMemory(instructions);
			byte value = 0x42;
			cpu.A = value;
			cpu.X = value;
			cpu.Y = value;
			byte stackPointer = cpu.SP;

			// Act
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(programStart + 1, cpu.PC, "For NOP the program counter should have been incremented by 1.");
			Assert.AreEqual<byte>(value, cpu.A, "Accumulator value should not have been changed by NOP.");
			Assert.AreEqual<byte>(value, cpu.X, "X register value should not have been changed by NOP.");
			Assert.AreEqual<byte>(value, cpu.Y, "Y register value should not have been changed by NOP.");
			Assert.AreEqual<byte>(stackPointer, cpu.SP, "Stack pointer value should not have been changed by NOP.");
		}

		[TestMethod]
		public void JMPAbsoluteShouldSetPCToAbsoluteAddress()
		{
			byte[] instructions = new byte[]
				{
					0x4c, 0x37, 0x13 // JMP $1337
				};
			InitializeMemory(instructions);

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(0x1337, cpu.PC, "Absolute jump should change program counter to address.");
		}

		[TestMethod]
		public void JMPIndirectShouldSetPCToIndirectAddress()
		{
			byte[] instructions = new byte[]
				{
					0x6c, 0x00, 0x10 // JMP ($1000)
				};
			InitializeMemory(instructions);
			memory[0x1000] = 0x37;
			memory[0x1001] = 0x13;

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(0x1337, cpu.PC, "Indirect jump should change program counter to address stored at indirect jump location.");
		}

		[TestMethod]
		public void JSRShouldPushReturnAddressOnStack()
		{
			byte[] instructions = new byte[]
				{
					0x20, 0x37, 0x13 // JSR $1337
				};
			InitializeMemory(instructions);

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<byte>(programStart >> 8, memory[0x1ff], "Stack should start with high byte of return address.");
			Assert.AreEqual<byte>((byte)(programStart & 0xff), memory[0x1e], "Stack should start with high byte of return address.");
			Assert.AreEqual<int>(0x1337, cpu.PC, "Absolute jump subroutine should change program counter to address stored at absolute jump location.");
		}

		[TestMethod]
		public void RTSShouldUpdatePCFromStack()
		{
			cpu.PushOnStack(0x13);
			cpu.PushOnStack(0x37);

			byte[] instructions = new byte[]
				{
					0x60 // RTS
				};
			InitializeMemory(instructions);

			// Act 
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(0x1338, cpu.PC, "RTS jump subroutine should change program counter to address stored at absolute jump location plus 1.");
		}

		[TestMethod]
		public void RTIShouldPullPSandPSFromStack()
		{
			byte originalStatus = 0xe3;
			byte[] instructions = new byte[]
				{
					0x40 // RTI
				};
			InitializeMemory(instructions);
			cpu.PushOnStack(0x13);
			cpu.PushOnStack(0x37);
			cpu.PushOnStack(originalStatus);

			// Act
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<int>(0x1337, cpu.PC, "RTI should have retrieved program counter from stack after RTI.");
			Assert.AreEqual<int>(originalStatus, cpu.ProcessorStatus, "RTI should restore program status after RTI.");
		}
	}
}
