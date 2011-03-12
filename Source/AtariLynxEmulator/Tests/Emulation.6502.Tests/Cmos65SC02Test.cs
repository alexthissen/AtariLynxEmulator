using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Processors;
using KillerApps.Emulation.Core;

namespace KillerApps.Emulation.Processors.Tests
{
	/// <summary>
	/// Summary description for Nmos6502Test
	/// </summary>
	[TestClass]
	public class Nmos6502Test
	{
		Nmos6502 cpu = null;
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
		public void DumpWithToStringShouldProvideCorrectState()
		{
			// Arrange
			cpu.A = 0x11;
			cpu.X = 0x22;
			cpu.Y = 0x33;
			cpu.ProcessorStatus = 0xff;
			cpu.PC = 0x1337;
			cpu.SP = 0x44;

			// Act
			string dump = cpu.ToString();

			// Assert
			Assert.AreEqual<string>("A:11 X:22 Y:33 S:44 PC:1337 Flags:[NVRBDIZC]", dump, "State dump of processor does not match.");
		}

		[TestMethod]
		public void MaskableInterruptShouldNotFireWhenInterruptDisableFlagSet()
		{
			// Arrange
			byte[] instructions = new byte[]
				{
					0xea // NOP
				};
			InitializeMemory(instructions);
			cpu.I = true;

			// Act
			cpu.SignalInterrupt(InterruptType.Irq);
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<ushort>(programStart + 1, cpu.PC, "Program counter should not have changed to interrupt vector.");
		}

		[TestMethod]
		public void MaskableInterruptShouldFireWhenInterruptDisableFlagClear()
		{
			// Arrange
			byte[] instructions = new byte[]
				{
					0xea // NOP
				};
			InitializeMemory(instructions);
			memory[VectorAddresses.IRQ_VECTOR] = 0x37;
			memory[VectorAddresses.IRQ_VECTOR + 1] = 0x13;
			cpu.I = false;

			// Act
			cpu.SignalInterrupt(InterruptType.Irq);
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<ushort>(0x1337, cpu.PC, "Program counter should have changed to interrupt vector.");
		}

		[TestMethod]
		public void ActiveInterruptShouldAlwaysWakeProcessor()
		{
			// Arrange
			byte[] instructions = new byte[]
				{
					0xea // NOP
				};
			InitializeMemory(instructions);
			cpu.TrySleep(1000); // Put processor to sleep
			cpu.I = true; // Disable interrupts
			cpu.SignalInterrupt(InterruptType.Irq);

			// Act
			cpu.Execute(1);

			// Assert
			Assert.IsFalse(cpu.IsAsleep, "After interrupt processor should always be awake.");
		}

		[TestMethod]
		public void SleepingProcessorShouldNotExecuteInstructions()
		{
			// Arrange
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

			cpu.TrySleep(1000); // Put processor to sleep
			Assert.IsFalse(cpu.IsSystemIrqActive, "No interrupts should be active before test.");

			// Act
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<ushort>(programStart, cpu.PC, "When sleeping program counter should not advance.");
			Assert.AreEqual<byte>(value, cpu.A, "Accumulator value should not have been changed when sleeping.");
			Assert.AreEqual<byte>(value, cpu.X, "X register value should not have been changed when sleeping.");
			Assert.AreEqual<byte>(value, cpu.Y, "Y register value should not have been changed when sleeping.");
			Assert.AreEqual<byte>(stackPointer, cpu.SP, "Stack pointer value should not have been changed when sleeping.");
		}

		[TestMethod]
		public void TrySleepShouldPutProcessorToSleep()
		{
			Assert.IsFalse(cpu.IsAsleep, "Processor should not be asleep before test.");

			// Act
			cpu.TrySleep(1000);
			
			// Assert 
			Assert.IsTrue(cpu.IsAsleep, "Processor should have been asleep.");
		}

		[TestMethod]
		public void NonMaskableInterruptShouldAlwaysFire()
		{
			// Arrange
			byte[] instructions = new byte[]
				{
					0xea // NOP
				};
			InitializeMemory(instructions);
			ushort nmiHandlerAddress = 0x1337;
			memory[VectorAddresses.NMI_VECTOR] = 0x37;
			memory[VectorAddresses.NMI_VECTOR + 1] = 0x13;
			memory[nmiHandlerAddress] = 0xea;
			cpu.I = true;

			// Act
			cpu.SignalInterrupt(InterruptType.Nmi);
			cpu.Execute(1);

			// Assert
			Assert.AreEqual<ushort>(++nmiHandlerAddress, cpu.PC, "Program counter should have changed to instruction after interrupt vector.");
		}

		[TestMethod]
		public void SignalInterruptShouldSetCorrectActiveInterrupt()
		{
			// Arrange
			InterruptType irq = InterruptType.Irq;

			// Act
			cpu.SignalInterrupt(irq);

			// Assert
			Assert.IsTrue(cpu.IsSystemIrqActive, "An interrupt should be active.");
			Assert.AreEqual<InterruptType>(irq, cpu.ActiveInterrupt, "Active interrupt is not set correctly by SignalInterrupt.");
		}
	}
}
