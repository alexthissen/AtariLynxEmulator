using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KillerApps.Emulation.Eeproms.Tests
{
	/// <summary>
	/// Summary description for Eeprom93C46BTest
	/// </summary>
	[TestClass]
	public class Eeprom93C46BTest
	{
		public Eeprom93C46BTest() { }

		private TestContext testContextInstance;
		private Eeprom93C46B eeprom;

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
		public void TestInitialize() 
		{
			eeprom = new Eeprom93C46B();
		}
		
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void OnStartupEepromShouldBeInCorrectState()
		{
			Assert.IsFalse(eeprom.CS, "At startup CS signal should be low.");
			Assert.IsFalse(eeprom.CLK.Value, "At startup CLK signal should be low.");
			Assert.IsFalse(eeprom.DI, "At startup DI signal should be low.");
			Assert.IsFalse(eeprom.DO, "At startup DO signal should be low.");
			Assert.AreEqual<OperationState>(OperationState.Standby, eeprom.State,
				"State of 93C46 should be standby on startup.");
			Assert.IsFalse(eeprom.EraseWriteEnabled, "Eeprom should have erase and write disabled at startup.");
		}

		[TestMethod]
		public void ProgramShouldChangeRegisterToCorrectValues()
		{
			eeprom.DI = true;
			eeprom.Update(true, true); //, true);

			Assert.IsTrue(eeprom.CS, "ChipSelect should be high after program.");
			Assert.IsTrue(eeprom.CLK.Value, "Clock value should be high after program.");
			Assert.IsTrue(eeprom.DI, "Data In should be high after program.");
		}

		[TestMethod]
		public void StateShouldChangeToStartWhenStartConditionIsMet()
		{
			eeprom.DI = true;

			// Act 
			eeprom.Update(true, true); //, true);

			// Assert
			Assert.AreEqual<OperationState>(OperationState.Clocking, eeprom.State,
				"State should change to ClockInstruction when start condition is met.");
			Assert.AreEqual<int>(0, eeprom.Cycles, "At change to Start the number of cycles should be zero.");
			Assert.AreEqual<ushort>(0, eeprom.OpcodeAndAddress, "Opcode should be zero at Start.");
		}

		[TestMethod]
		public void StateShouldOnlyChangeToStartForRisingClockSignal()
		{
			// Arrange
			eeprom.CLK.Value = true; // Force clock signal to high
			eeprom.CurrentEdge = VoltageEdge.None; // Reset voltage edge
			eeprom.DI = true;

			// Act
			eeprom.Update(true, true); //, true);

			// Assert
			Assert.AreEqual<OperationState>(OperationState.Standby, eeprom.State,
				"State should only change to Start when clock signal is rising.");
		}

		[TestMethod]
		public void RisingClockShouldGivePositiveEdge()
		{
			// Arrange
			VoltageEdge edge = VoltageEdge.Negative;
			eeprom.CLK.Value = false;
			eeprom.CLK.EdgeChange += (sender, args) => { edge = args.Edge; };

			// Act
			eeprom.CLK.Value = true;

			// Assert
			Assert.AreEqual<VoltageEdge>(VoltageEdge.Positive, edge, "Rising clock value should give positive edge.");
		}

		[TestMethod]
		public void ClockingOneBitShouldProgressInstructionBuilding()
		{
			// Arrange
			eeprom.DI = true;
			eeprom.Update(true, true); //, true);

			// Act
			eeprom.Update(true, false); //, true);
			eeprom.Update(true, true); //, true);

			// Assert
			Assert.AreEqual<int>(1, eeprom.Cycles, "Exactly one cycle should have been executed.");
			Assert.AreEqual<ushort>(0x01, eeprom.OpcodeAndAddress, "For one bit with value 1 Opcode should change to 1.");
		}

		[TestMethod]
		public void OpcodeEWENShouldEnableEraseWrite()
		{
			// Arrange
			eeprom.DI = true;
			eeprom.Update(true, true); //, true);

			// Act
			eeprom.EnableEraseWrite();

			// Assert
			Assert.IsTrue(eeprom.EraseWriteEnabled, "EWEN opcode should enable erase and write.");
			Assert.AreEqual<OperationState>(OperationState.Standby, eeprom.State, "After EWEN eeprom should be on standby.");
		}

		[TestMethod]
		public void OpcodeEWDSShouldDisableEraseWrite()
		{
			// Arrange
			eeprom.DI = true;
			eeprom.Update(true, true);//, true);

			// Act
			eeprom.DisableEraseWrite();

			// Assert
			Assert.IsFalse(eeprom.EraseWriteEnabled, "EWDS opcode should disable erase and write.");
			Assert.AreEqual<OperationState>(OperationState.Standby, eeprom.State, "After EWDS eeprom should be on standby.");
		}

		[TestMethod]
		public void Clocking8BitsShouldExecuteInstruction()
		{
			// Arrange
			eeprom.Start();
			eeprom.EnableEraseWrite();
			eeprom.Start();
			Instruction instruction = null;
			eeprom.InstructionDone += (sender, args) => { instruction = args.ExecutedInstruction; };
			
			// Act
			for (int i = 0; i < 8; i++)
			{
				eeprom.Pulse(true);
			}

			// Assert
			Assert.AreEqual<OperationState>(OperationState.Standby, eeprom.State,
				"After 8 cycles state should change to Standby for ERASE.");
			Assert.IsNotNull(instruction, "An instruction should have executed.");
			Assert.AreEqual<Opcode>(Opcode.ERASE, instruction.Opcode, "ERASE should have executed.");
			Assert.AreEqual<ushort>(0x3F, instruction.Address, "Address for instruction should be lower 6 bits at one.");
		}

		[TestMethod]
		public void ReadShouldReturnCorrectDataFromMemory()
		{
			// Arrange
			ushort address = 0x000F;
			ushort value = 0x1234;
			Instruction instruction = null;

			eeprom.Start();
			eeprom.Memory[address] = value;
			eeprom.InstructionDone += (sender, args) => { instruction = args.ExecutedInstruction; };
			
			// Act
			ushort actual = eeprom.Read(address);

			// Assert
			Assert.IsNotNull(instruction, "Instruction should have executed.");
			Assert.AreEqual<Opcode>(Opcode.READ, instruction.Opcode, "READ opcode should have executed.");
			Assert.AreEqual<ushort>(address, instruction.Address, "Address should be as expected.");
			Assert.AreEqual<ushort>(value, actual, "Read operation should return correct value from memory cell.");
		}

		[TestMethod]
		public void WriteShouldSetCorrectDataInMemory()
		{
			// Arrange
			ushort address = 0x000F;
			ushort value = 0x1234;
			Instruction instruction = null;

			eeprom.Start();
			eeprom.EnableEraseWrite();
			eeprom.Memory[address] = 0x00;
			eeprom.InstructionDone += (sender, args) => { instruction = args.ExecutedInstruction; };
			eeprom.Start();

			// Act
			eeprom.Write(address, value);

			// Assert
			Assert.IsNotNull(instruction, "Instruction should have executed.");
			Assert.AreEqual<Opcode>(Opcode.WRITE, instruction.Opcode, "WRITE opcode should have executed.");
			Assert.AreEqual<ushort>(address, instruction.Address, "Address should be as expected.");
			Assert.AreEqual<ushort>(value, eeprom.Memory[address], "Write operation should have set value in memory cell.");
		}

		[TestMethod]
		public void WriteAllShouldSetCorrectDataInMemory()
		{
			// Arrange
			ushort value = 0x1234;
			Instruction instruction = null;

			eeprom.Start();
			eeprom.EnableEraseWrite();
			eeprom.InstructionDone += (sender, args) => { instruction = args.ExecutedInstruction; };
			eeprom.Start();

			// Act
			eeprom.WriteAll(value);

			// Assert
			Assert.IsNotNull(instruction, "Instruction should have executed.");
			Assert.AreEqual<Opcode>(Opcode.WRAL, instruction.Opcode, "WRAL opcode should have executed.");
			for (int i = 0; i < eeprom.Memory.Length; i++)
			{
				Assert.AreEqual<ushort>(value, eeprom.Memory[i], "WriteAll operation should have set value in memory cell {0}.", i);				
			} 
		}

		[TestMethod]
		public void ConvertingFrom4BitOpcodeShouldReturnInstruction()
		{
			// Arrange
			ushort opcodeAndAddress = 0x003F;
			
			// Act
			Instruction instruction = eeprom.PrepareInstruction(opcodeAndAddress);

			// Assert
			Assert.AreEqual<Opcode>(Opcode.EWEN, instruction.Opcode,
				"4 Bit opcode should give corresponding instruction.");
		}

		[TestMethod]
		public void ConvertingFrom2BitOpcodeShouldReturnInstructionAndAddress()
		{
			// Arrange
			byte opcodeAndAddress = 0xFF;

			// Act
			Instruction instruction = eeprom.PrepareInstruction(opcodeAndAddress);

			// Assert
			Assert.AreEqual<Opcode>(Opcode.ERASE, instruction.Opcode,
				"2 bit opcode should give corresponding instruction.");
			Assert.AreEqual<ushort>(0x3F, instruction.Address, "2 bit opcode should have correct address.");
		}
	}
}
