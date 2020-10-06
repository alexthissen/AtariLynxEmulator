using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace KillerApps.Emulation.AtariLynx
{
	// "Each audio channel consists of a programmable base frequency, 
	// a programmable polynomial sequence generator, a waveshape selector, 
	// and a programmable volume control."

	public class AudioChannel: TimerBase, IResetable
	{
		// "12 bits of shift register and 9 bits of tap selector. 
		// 9 outputs of the 12 bit shift register are individually selectable as inputs to a large exclusive or gate."
		public ushort ShiftRegister { get; private set; }
		private static int[] switches = { 0, 1, 2, 3, 4, 5, 10, 11, 7 };
		private AudioControlBits audioControlBits;
	
		// "Note: registers only exist on MIKEY rev 2 and later"
		// "Four bit attenuation values range from 0000 = silent to 1111 = 15/16 volume"
		public AttenuationRegister Attenuation { get; private set; }
		public sbyte OutputValue { get; internal set; }
		
		// "7 bits of volume control and 1 bit of sign (2s complement number notation). 
		// Each channel has its own 128 settings of volume, with an invert bit. The range includes ZERO output."
		public sbyte VolumeControl { get; internal set; }

		// "9 outputs of the 12 bit shift register are individually selectable as inputs to a large exclusive or gate."
		public ShiftRegisterFeedbackEnable FeedbackEnable { get; private set; }
		public AudioControlBits AudioControl 
		{
			get { return audioControlBits; }
			set
			{
				audioControlBits = value;
				TimerLogic = TimerLogicFactory.CreateTimerLogic(this, audioControlBits.SourcePeriod, TimerLogic);
			}
		}

		public override TimerControlBase TimerControlBits
		{
			get { return audioControlBits; }
		}

		public AudioChannel()
		{
			AudioControl = new AudioControlBits(0);
			FeedbackEnable = new ShiftRegisterFeedbackEnable(0);
		}

		public override void Reset()
		{
			base.Reset();

			// "Audio are reset to 0, all are read/write"
			VolumeControl = 0x00;
			AudioControl.ByteData = 0x00;
			FeedbackEnable.ByteData = 0x00;
		}

		public byte LowerShiftRegister 
		{
			get { return (byte)(ShiftRegister & 0x000000FF); }
			internal set 
			{
				ShiftRegister &= 0x0F00;
				ShiftRegister |= value;
			}
		}

		public byte OtherControlBits 
		{
			get { return (byte)(((ShiftRegister & 0x0F00) >> 4) + (DynamicControlBits.ByteData & 0x0F)); }
			set
			{
				ShiftRegister &= 0x00FF;
				ShiftRegister |= (ushort)((value & 0xF0) << 4);
				DynamicControlBits.ByteData = (byte)(value & 0x0F);
			}
		}

		// TODO: Provide property for Lower Nybble Bug enabling

		// "The lower nybble of the audio out byte is processed incorrectly in the digital to pulse 
		// width converter. The upper bit of this lower nybble should have been inverted prior to conversion. 
		// This is how we achieve a 50% duty cycle for 0 volume. 
		// (we did it right for the upper nybble) 
		// This error results in a single glitch in the sound when the value transitions from 8 to 9. 
		// The effect is at most, 1/16 of the max volume for one tick of the 0/A clock, 
		// and is generally not noticed."

		protected void Expire()
		{
			UpdateShiftRegister();

			// "1 bit of waveshape selector. The rectangular waveform from the polynomial generator 
			// can be unmodified or integrated. 
			// The purpose of the integration is to create an approximately triangular waveshape. 
			// This sounds better in many cases than a rectangular waveshape."
			if (AudioControl.EnableIntegrateMode)
			{
				// "In integrate mode, instead of sending the volume register directly to the DAC 
				// it instead adds the volume register (or it's 2's complement) to a running total that is 
				// then sent to the DAC."
				int output = this.OutputValue;

				if ((ShiftRegister & 0x0001) == 0x0001)
				{
					// "In integrate mode, shift reg 0 = 1: add volume register to output."
					output += VolumeControl;
				}
				else
				{
					// "In integrate mode, shift reg 0 = 0: subtract volume register from output."
					output -= VolumeControl;
				}

				// "Note that there is hardware clipping at max and min (ff,00)."
				if (output > 127) output = 127;
				if (output < -128) output = -128;
				OutputValue = (sbyte)output;
			}
			else
			{
				// "In normal nonintegrate mode, the bit selects either the value in the volume 
				// register or its 2's complement and sends it to the output DAC."
				if ((ShiftRegister & 0x0001) == 0x0001)
				{
					// "In normal mode, shift reg 0 = 1: contains value of volume register."
					OutputValue = VolumeControl;
				}
				else
				{
					// "In normal mode, shift reg 0 = 0: contains 2's complement of volume register."
					OutputValue = (sbyte)-VolumeControl;
				}
			}

			// "It is set on time out, reset with the reset timer done bit (xxx1, B6)"
			DynamicControlBits.TimerDone = true; // !StaticControlBits.ResetTimerDone;
			DynamicControlBits.BorrowOut = true;

			// "Timers can be set to stop when they reach a count of 0 or to reload from their backup register."
			// Reload if neccessary
			CurrentValue = AudioControl.EnableReload ? BackupValue : (byte)0;
		}

		public override ulong Update(ulong currentCycleCount)
		{
			ExpirationTime = ulong.MaxValue;

			// Only enabled and not-done timers should predict expiration time
			if (AudioControl.EnableCount && (AudioControl.EnableReload || !DynamicControlBits.TimerDone)
				&& VolumeControl != 0 && BackupValue != 0)
			{
				// Assume timer has not expired and is not updated
				DynamicControlBits.BorrowOut = false;
				DynamicControlBits.BorrowIn = false; // TODO: Find out why and when borrow-in is set

				// Calculate new current value and update if necessary
				bool expired = TimerLogic.UpdateCurrentValue(currentCycleCount);

				// When timer value has expired it will attempt to borrow out
				if (expired) Expire();
				ExpirationTime = TimerLogic.PredictExpirationTime(currentCycleCount);
			}

			// Audio channels never trigger interrupts, so never spend CPU time in IRQ handling code
			return 0;
		}

		private void UpdateShiftRegister()
		{
			// "The inversion of the output of the gate is used as the data input to the shift register. 
			// This is known as a polynomial sequence generator and can generate waveforms that 
			// range from square (for musical notes) to pseudo random (for explosions). 
			// This same inverted output is taken from the exclusive or gate and 
			// sent to the waveshape selector."

			// "The repeat period is programmed by selecting the initial value in the shift register 
			// (set shifter) and by picking which feedback taps are connected."
			ushort tapSelector = FeedbackEnable.ByteData;
			if (((AudioControlBits)this.TimerControlBits).FeedbackBit7) tapSelector |= 0x100;
			ushort feedback = CalculateFeedback(ShiftRegister, tapSelector);

			ShiftRegister <<= 1;
			ShiftRegister &= 0xffe;
			ShiftRegister |= (ushort)((feedback > 0) ? 0 : 1);
		}

		private ushort CalculateFeedback(ushort ShiftRegister, ushort tapSelector)
		{
			// 11 10 9 8 7 6 5 4 3 2 1 0
			//  *  *     *   * * * * * *
			ushort feedback = 0x0000;

			for (int index = 0; index < 9; index++)
			{
				if (((tapSelector >> index) & 0x0001) == 0x0001)
					feedback ^= (ushort)((ShiftRegister >> switches[index]) & 0x0001);
			}
			return feedback;
		}
	}
}
