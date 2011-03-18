using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class SpriteSystemControl
	{
		// Because of different read and write behavior of SPRSYS, the logic is implemented 
		// in get/set accessors
		public byte ByteData
		{
			get
			{
				byte data = 0x00;
				if (MathInProcess) data |= MATHWORKINGMask;
				if (MathWarning) data |= MATHWARNINGMask;
				if (LastCarry) data |= MATHCARRYMask;
				if (VStretch) data |= VSTRETCHINGMask;
				if (LeftHanded) data |= LEFTHANDEDMask;
				if (UnsafeAccess) data |= UNSAFE_ACCESSMask;
				if (StopAtEndOfCurrentSprite) data |= SPRITESTOPMask;
				if (SpriteProcessStarted) data |= SPRITEWORKINGMask;
				return data;
			}
			set
			{
				SignedMath = (value & SIGNMATHMask) == SIGNMATHMask;
				Accumulate = (value & ACCUMULATEMask) == ACCUMULATEMask;
				DontCollide = (value & NO_COLLIDEMask) == NO_COLLIDEMask;
				VStretch = (value & VSTRETCHMask) == VSTRETCHMask;
				LeftHanded = (value & LEFTHANDEDMask) == LEFTHANDEDMask;
				if ((value & CLR_UNSAFEMask) == CLR_UNSAFEMask) UnsafeAccess = false;
				StopAtEndOfCurrentSprite = (value & SPRITESTOPMask) == SPRITESTOPMask;
			}
		}

		// Write only
		public bool SignedMath { internal get; set; } // "B7 = Signmath. 0 =unsigned math, 1 =signed math."
		public bool Accumulate { internal get; set; } // "B6 = OK to accumulate. 0 =do not accumulate, 1 =yes, accumulate."
		public bool DontCollide { internal get; set; } // "B5 = dont collide. 1=dont collide with any sprites."

		// Read only
		public bool MathInProcess { get; internal set; } // "B7 = Math in process"
		public bool MathWarning { get; internal set; } // "B6 = Mathbit. If mult, 1=accumulator overflow. If div, 1=div by zero attempted."
		public bool LastCarry { get; internal set; } // "B5 = Last carry bit."
		public bool UnsafeAccess { get; internal set; } // "B2 = UnsafeAccess. 1=Unsafe Access was performed."
		public bool SpriteProcessStarted { get; internal set; } // "B0 = Sprite process was started and has neither completed nor been stopped. "

		// Read and write
		public bool VStretch { get; set; } // "B4 = Vstretch. 1=stretch the v. 0 =Don't play with it, it will grow by itself."
		public bool LeftHanded { get; set; } // "83 = Lefthand, 0=normal handed"

		// "Continue sprite processing by setting the Sprite Process Start Bit. Either setting or 
		// clearing the SPSB will clear this stop request."
		// LX: SPSB stands for SpriteProcessingStartBit
		public bool StopAtEndOfCurrentSprite { get; set; } // "B1 = Stop at end of current sprite, 1 = request to stop."

		private const byte SIGNMATHMask = 0x80;
		private const byte ACCUMULATEMask = 0x40;
		private const byte NO_COLLIDEMask = 0x20;
		private const byte VSTRETCHMask = 0x10;
		private const byte LEFTHANDMask = 0x08;
		private const byte CLR_UNSAFEMask = 0x04;
		private const byte SPRITETOSTOPMask = 0x02;

		private const byte MATHWORKINGMask = 0x80;
		private const byte MATHWARNINGMask = 0x40;
		private const byte MATHCARRYMask = 0x20;
		private const byte VSTRETCHINGMask = 0x10;
		private const byte LEFTHANDEDMask = 0x08;
		private const byte UNSAFE_ACCESSMask = 0x04;
		private const byte SPRITESTOPMask = 0x02;
		private const byte SPRITEWORKINGMask = 0x01;
	}
}
