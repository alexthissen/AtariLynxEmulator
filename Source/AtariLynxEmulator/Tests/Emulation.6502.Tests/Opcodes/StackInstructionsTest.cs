using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KillerApps.Emulation.Processors.Tests
{
	[TestClass]
	public class StackInstructionsTest
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
		public void TSXShouldTransferStackPointerToXRegister()
		{
			byte stackPointer = 0xfd;
			cpu.X = 0;
			cpu.SP = stackPointer;
			byte[] instructions = new byte[]
				{
					0xba // TSX
				};
			InitializeMemory(instructions);

			// Act
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<byte>(stackPointer, cpu.X, "Stack pointer should be transfered to X register with TSX.");
			Assert.IsTrue(cpu.N, "Negative flag should be set if bit 7 of stack pointer is true with TSX.");
		}

		[TestMethod]
		public void TSXShouldSetZeroFlagWhenStackPointerIsZero()
		{
			byte stackPointer = 0x00;
			cpu.X = 0xff;
			cpu.SP = stackPointer;
			byte[] instructions = new byte[]
				{
					0xba // TSX
				};
			InitializeMemory(instructions);

			// Act
			cpu.Execute(1);

			// Assert
			Assert.IsTrue(cpu.Z, "Zero flag should be set if stack pointer is zero with TSX.");
		}

		[TestMethod]
		public void TXSShouldTransferXRegisterToStackPointer()
		{
			byte registerX = 0xff;
			cpu.X = 0xff;
			cpu.SP = 0x00;
			byte[] instructions = new byte[]
				{
					0x9a // TXS
				};
			InitializeMemory(instructions);

			// Act
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<byte>(registerX, cpu.SP, "Stack pointer should be set to value of X register with TXS.");			
		}

		[TestMethod]
		public void PHAShouldPushAccumulatorOnStack()
		{
			byte accumulator = 0x42;
			cpu.A = accumulator;
			byte[] instructions = new byte[]
				{
					0x48 // PHA
				};
			InitializeMemory(instructions);

			// Act
			cpu.Execute(1);
			byte valueOnStack = cpu.PullFromStack();

			// Assert
			Assert.AreEqual<byte>(accumulator, valueOnStack, "Value on stack should be same as accumulator after PHA.");
		}

		[TestMethod]
		public void PHPShouldPushProcessorStatusOnStack()
		{
			byte status = 0xe4;
			cpu.ProcessorStatus = status;
			byte[] instructions = new byte[]
				{
					0x08 // PHP
				};
			InitializeMemory(instructions);

			// Act
			cpu.Execute(1);
			byte valueOnStack = cpu.PullFromStack();

			// Assert
			Assert.AreEqual<byte>(status, valueOnStack, "Value on stack should be same as processor status after PHP.");
		}

		[TestMethod]
		public void PHXShouldPushXRegisterOnStack()
		{
			byte registerX = 0x42;
			cpu.X = registerX;
			byte[] instructions = new byte[]
				{
					0xDA // PHX
				};
			InitializeMemory(instructions);

			// Act
			cpu.Execute(1);
			byte valueOnStack = cpu.PullFromStack();

			// Assert
			Assert.AreEqual<byte>(registerX, valueOnStack, "Value on stack should be same as register X after PHX.");
		}

		[TestMethod]
		public void PHYShouldPushYRegisterOnStack()
		{
			byte registerY = 0x42;
			cpu.Y = registerY;
			byte[] instructions = new byte[]
				{
					0x5A // PHY
				};
			InitializeMemory(instructions);

			// Act
			cpu.Execute(1);
			byte valueOnStack = cpu.PullFromStack();

			// Assert
			Assert.AreEqual<byte>(registerY, valueOnStack, "Value on stack should be same as register Y after PHY.");
		}

		[TestMethod]
		public void PLAShouldPullStackValueInAccumulator()
		{
			byte value = 0x42;
			cpu.A = 0x00;
			cpu.PushOnStack(value);
			byte[] instructions = new byte[]
				{
					0x68 // PLA
				};
			InitializeMemory(instructions);

			// Act
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<byte>(value, cpu.A, "Accumulator should get value from stack after PLA.");
		}

		[TestMethod]
		public void PLPShouldSetProcessorStatusFromStack()
		{
			byte value = 0x62;
			cpu.ProcessorStatus = 0x20;
			cpu.PushOnStack(value);
			byte[] instructions = new byte[]
				{
					0x28 // PLP
				};
			InitializeMemory(instructions);

			// Act
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<byte>(value, cpu.ProcessorStatus, "Processor status should get value from stack after PLP.");
		}

		[TestMethod]
		public void PLXShouldSetXRegisterFromStack()
		{
			byte value = 0x42;
			cpu.X = 0x00;
			cpu.PushOnStack(value);
			byte[] instructions = new byte[]
				{
					0xFA // PLX
				};
			InitializeMemory(instructions);

			// Act
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<byte>(value, cpu.X, "X register should get value from stack after PLX.");
		}

		[TestMethod]
		public void PLYShouldSetYRegisterFromStack()
		{
			byte value = 0x42;
			cpu.Y = 0x00;
			cpu.PushOnStack(value);
			byte[] instructions = new byte[]
				{
					0x7A // PLY
				};
			InitializeMemory(instructions);

			// Act
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<byte>(value, cpu.Y, "Y register should get value from stack after PLY.");
		}

	}
}
