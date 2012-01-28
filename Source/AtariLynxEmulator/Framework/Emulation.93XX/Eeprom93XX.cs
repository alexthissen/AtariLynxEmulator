using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace KillerApps.Emulation.Eeproms
{
	public class Eeprom93XX
	{
		private Instruction currentInstruction = null;
		internal ushort[] Memory;
		internal ushort Data;
		internal ushort OpcodeAndAddress;
		internal VoltageEdge CurrentEdge;
		protected int AddressBitCount;
		protected int DataBitCount { get { return Organization == Organization.Byte ? 8 : 16; } }
		protected int InstructionBitCount { get { return AddressBitCount + 2; } }
		protected const ushort OpcodeMaskBase = 0xF;

		// Events
		public event EventHandler<InstructionDoneEventArgs> InstructionDone;

		// Properties
		public ushort OpcodeMask { get; private set; }
		public ushort AddressMask { get; private set; }
		public bool EraseWriteEnabled { get; private set; }
		public OperationState State { get; private set; }
		public int Cycles { get; private set; }
		public Organization Organization { get; private set; }

		public Instruction CurrentInstruction
		{
			get
			{
				if (State == OperationState.Standby)
					throw new InvalidOperationException("Eeprom is not performing an instruction.");

				return currentInstruction;
			}
		}

		// "A high level selects the device; a low level deselects the device and 
		// forces it into standby mode."
		// "If CS is low, the internal control logic is held in a RESET status."
		public bool CS { get; private set; }

		// "The Serial Clock is used to synchronize the communication between a 
		// master device and the 93LC46AX/BX."
		public Clock CLK { get; internal set; }

		// "Data In (DI) is used to clock in a START bit, opcode,
		// address, and data synchronously with the CLK input."
		public bool DI { get; set; }

		// "Data Out (DO) is used in the READ mode to output data synchronously 
		// with the CLK input (TPD after the positive edge of CLK)."
		public bool DO { get; set; }

		public Eeprom93XX(byte addressBitCount, Organization organization)
		{
			this.AddressBitCount = addressBitCount;
			this.Organization = organization;
			CLK = new Clock();
			CLK.EdgeChange += new EventHandler<EdgeChangedEventArgs>(OnEdgeChange);
			State = OperationState.Standby;

			// "The 93LC46A/B powers up in the ERASE/WRITE Disable (EWDS) state"
			EraseWriteEnabled = false;
			Memory = new ushort[1 << (AddressBitCount + 1)];
			ComputeMasks();
		}

		private void ComputeMasks()
		{
			// TODO: Opcode mask calculation will fail for 76A version, because it takes a dummy bit after 2 bit opcode
			OpcodeMask = (ushort)(OpcodeMaskBase << (AddressBitCount - 2));
			AddressMask = (ushort)((1 << AddressBitCount) - 1);
		}

		internal Instruction PrepareInstruction(ushort opcodeAndAddress)
		{
			byte rawOpcode = (byte)((opcodeAndAddress & OpcodeMask) >> (AddressBitCount - 2));
			if (rawOpcode > 0x03) rawOpcode &= 0x0C;
			return new Instruction((Opcode)rawOpcode, (ushort)(opcodeAndAddress & AddressMask));
		}

		protected ushort Address
		{
			get
			{
				if (State != OperationState.Executing)
					throw new InvalidOperationException("Eeprom is not executing an instruction.");
				return (ushort)(OpcodeAndAddress & AddressMask);
			}
		}

		protected bool IsStartConditionMet
		{
			// "The START bit is detected by the device if CS and DI are both high 
			// with respect to the positive edge of CLK for the first time."
			get { return CurrentEdge == VoltageEdge.Positive && CS && DI; }
		}

		private void OnStandby()
		{
			State = OperationState.Standby;
			CS = false;
			currentInstruction = null;
		}

		private void OnStart()
		{
			State = OperationState.Clocking;
			OpcodeAndAddress = 0;
			Cycles = 0;
		}

		// Event handlers
		private void OnEdgeChange(object sender, EdgeChangedEventArgs e)
		{
			CurrentEdge = e.Edge;
		}

		private void OnAddressComplete()
		{
			currentInstruction = PrepareInstruction(OpcodeAndAddress);

			switch (CurrentInstruction.Opcode)
			{
				case Opcode.ERASE:
					Erase(CurrentInstruction.Address);
					break;

				case Opcode.ERAL:
					EraseAll();
					break;

				case Opcode.EWDS:
					DisableEraseWrite();
					break;

				case Opcode.EWEN:
					EnableEraseWrite();
					break;

				case Opcode.READ:
					Data = Memory[CurrentInstruction.Address];
					Cycles = 0;
					State = OperationState.Executing;
					break;

				case Opcode.WRITE:
				case Opcode.WRAL:
					Data = 0;
					Cycles = 0;
					State = OperationState.Executing;
					break;

				default:
					break;
			}
		}

		protected virtual void OnInstructionDone(InstructionDoneEventArgs args)
		{
			if (InstructionDone != null)
				InstructionDone(this, args);
		}

		public void Update(bool chipSelect, bool clock) //, bool dataIn)
		{
			CS = chipSelect;
			CLK.Value = clock;
			//DI = dataIn;

			if (!CS) State = OperationState.Standby;

			// Debug.WriteLine("{0},{1},{2},{3},{4},{5}", CS, CLK.Value, DI, DO, CurrentEdge, State);

			switch (State)
			{
				case OperationState.Standby:
					if (IsStartConditionMet)
					{
						OnStart();
					}
					break;

				case OperationState.Clocking:
					ClockInstruction(DI);
					break;

				case OperationState.Executing:
					ExecuteInstruction();
					break;

				default:
					break;
			}

			CurrentEdge = VoltageEdge.None;
		}

		private void ClockInstruction(bool dataIn)
		{
			// Instructions, addresses, and write data are clocked into
			// the DI pin on the rising edge of the clock (CLK).
			if (CurrentEdge != VoltageEdge.Positive) return;

			OpcodeAndAddress <<= 1;
			OpcodeAndAddress |= (ushort)(dataIn ? 1 : 0);
			Cycles++;

			if (Cycles == InstructionBitCount)
			{
				OnAddressComplete();
			}
		}

		private void ExecuteInstruction()
		{
			// "The output data bits will toggle on the rising edge of the CLK and ..."
			// "Instructions, addresses, and write data are clocked into
			// the DI pin on the rising edge of the clock (CLK)."
			if (CurrentEdge != VoltageEdge.Positive) return;

			switch (CurrentInstruction.Opcode)
			{
				case Opcode.READ:
					ReadNextBit();
					break;

				case Opcode.WRITE:
				case Opcode.WRAL:
					WriteNextBit();
					break;

				default:
					throw new InvalidOperationException(String.Format("Opcode {0} cannot be in Executing state.", CurrentInstruction.Opcode));
			}

			Cycles++;
			if (Cycles == DataBitCount)
			{
				OnInstructionDone(new InstructionDoneEventArgs(CurrentInstruction));

				if (CurrentInstruction.Opcode == Opcode.READ && (Address < ((1 << AddressBitCount) - 1)))
				{
					// "Sequential read is possible when CS is held high. The memory data will automatically 
					// cycle to the next register and output sequentially."
					OpcodeAndAddress++;
					Cycles = 0;
				}
				else
				{
					// "DO at logical ‘0’ indicates that programming is still in progress. DO at logical ‘1’ 
					// indicates that the register at the specified address has been written with the data 
					// specified and the device is ready for another instruction."
					DO = true; // For now pretend that the storage time is neglectable
					if (CurrentInstruction.Opcode == Opcode.WRITE) Write(currentInstruction.Address, Data);
					if (CurrentInstruction.Opcode == Opcode.WRAL) WriteAll(currentInstruction.Address);
					OnStandby();
				}
			}
		}

		private void WriteNextBit()
		{
			Data <<= 1;
			Data |= (ushort)(DI ? 1 : 0);
		}

		private void EnableEraseWrite()
		{
			EraseWriteEnabled = true;
			OnInstructionDone(new InstructionDoneEventArgs(CurrentInstruction));
			OnStandby();
		}

		private void DisableEraseWrite()
		{
			EraseWriteEnabled = false;
			OnInstructionDone(new InstructionDoneEventArgs(CurrentInstruction));
			OnStandby();
		}

		private void ReadNextBit()
		{
			DO = ((Data & (1 << (DataBitCount - Cycles - 1))) != 0);
		}

		private void Erase(ushort address)
		{
			if (!EraseWriteEnabled)
				throw new InvalidOperationException("Eeprom does not have EraseWrite enabled.");

			// "The ERASE instruction forces all data bits of the specified address to the 
			// logical “1” state."
			Memory[address] = UInt16.MaxValue;

			// "This falling edge of the CS pin initiates the self-timed programming cycle."
			OnInstructionDone(new InstructionDoneEventArgs(CurrentInstruction));
			OnStandby();
		}

		private void EraseAll()
		{
			if (!EraseWriteEnabled)
				throw new InvalidOperationException("Eeprom does not have erase enabled.");

			// "The ERAL cycle is completely self-timed and	commences at the falling edge of the CS."
			CS = false;

			// "The Erase All (ERAL) instruction will erase the entire memory array to 
			// the logical ‘1’ state. The ERAL cycle is identical to the erase cycle, except 
			// for the different opcode."
			for (int address = 0; address < Memory.Length; address++)
			{
				Memory[address] = UInt16.MaxValue;
			}

			OnInstructionDone(new InstructionDoneEventArgs(CurrentInstruction));
			OnStandby();
		}

		private void Write(ushort address, ushort data)
		{
			if (!EraseWriteEnabled)
				throw new InvalidOperationException("Eeprom does not have erase enabled.");

			// "After the last data bit is put on the DI pin, the falling edge of CS initiates 
			// the self-timed autoerase and programming cycle."
			CS = false;

			Memory[address] = data;
		}

		private void WriteAll(ushort data)
		{
			if (!EraseWriteEnabled)
				throw new InvalidOperationException("Eeprom does not have erase enabled.");

			// "The WRAL command does include an automatic ERAL cycle for the device."
			EraseAll();

			// "The Write All (WRAL) instruction will write the entire memory array with 
			// the data specified in the command."
			for (int address = 0; address < Memory.Length; address++)
			{
				Memory[address] = data;
			}
		}
	}
}
