using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.AtariLynx
{
	public partial class Suzy
	{
		public static class Addresses
		{
			public const ushort MATHD = 0xFC52;
			public const ushort MATHC = 0xFC53;
			public const ushort MATHB = 0xFC54;
			public const ushort MATHA = 0xFC55;
			public const ushort MATHP = 0xFC56;
			public const ushort MATHN = 0xFC57;
			public const ushort MATHH = 0xFC60;
			public const ushort MATHG = 0xFC61;
			public const ushort MATHF = 0xFC62;
			public const ushort MATHE = 0xFC63;
			public const ushort MATHM = 0xFC6C;
			public const ushort MATHL = 0xFC6D;
			public const ushort MATHK = 0xFC6E;
			public const ushort MATHJ = 0xFC6F;

			public const ushort TMPADRL = 0xFC00; // "Temporary address" Low byte
			public const ushort TMPADRH = 0xFC01; // "Temporary address" High byte
			public const ushort TILTACUML = 0xFC02; // "Accumulator for tilt value" Low byte
			public const ushort TILTACUMH = 0xFC03; // "Accumulator for tilt value" High byte
			public const ushort HOFFL = 0xFC04; // "Offset to H edge of screen" Low byte
			public const ushort HOFFH = 0xFC05; // "Offset to H edge of screen" High byte
			public const ushort VOFFL = 0xFC06; // "Offset to V edge of screen" Low byte
			public const ushort VOFFH = 0xFC07; // "Offset to V edge of screen" High byte
			public const ushort VIDBASL = 0xFC08; // "Base Address of Video Build Buffer" Low byte
			public const ushort VIDBASH = 0xFC09; // "Base Address of Video Build Buffer" High byte
			public const ushort COLLBASL = 0xFC0A; // "Base Address of Coll Build Buffer" Low byte
			public const ushort COLLBASH = 0xFC0B; // "Base Address of Coll Build Buffer" High byte
			public const ushort VIDADRL = 0xFC0C; // "Current Video Build Address" Low byte
			public const ushort VIDADRH = 0xFC0D; // "Current Video Build Address" High byte
			public const ushort COLLADRL = 0xFC0E; // "Current Collision Build Address" Low byte
			public const ushort COLLADRH = 0xFC0F; // "Current Collision Build Address" High byte
			public const ushort SCBNEXTL = 0xFC10; // "Address of Next SCB" Low byte
			public const ushort SCBNEXTH = 0xFC11; // "Address of Next SCB" High byte
			public const ushort SPRDLINEL = 0xFC12; // "Start of Sprite Data Line Address" Low byte
			public const ushort SPRDLINEH = 0xFC13; // "Start of Sprite Data Line Address" High byte
			public const ushort HPOSSTRTL = 0xFC14; // "Starting Hpos" Low byte
			public const ushort HPOSSTRTH = 0xFC15; // "Starting Hpos" High byte
			public const ushort VPOSSTRTL = 0xFC16; // "Starting Vpos" Low byte
			public const ushort VPOSSTRTH = 0xFC17; // "Starting Vpos" High byte
			public const ushort SPRHSIZL = 0xFC18; // "H Size" Low byte
			public const ushort SPRHSIZH = 0xFC19; // "H Size" High byte
			public const ushort SPRVSIZL = 0xFC1A; // "V Size" Low byte
			public const ushort SPRVSIZH = 0xFC1B; // "V Size" High byte
			public const ushort STRETCHL = 0xFC1C; // "H Size Adder" Low byte
			public const ushort STRETCHH = 0xFC1D; // "H Size Adder" High byte
			public const ushort TILTL = 0xFC1E; // "H Position Adder" Low byte
			public const ushort TILTH = 0xFC1F; // "H Position Adder" High byte
			public const ushort SPRDOFFL = 0xFC20; // "Offset to Next Sprite Data Line" Low byte
			public const ushort SPRDOFFH = 0xFC21; // "Offset to Next Sprite Data Line" High byte
			public const ushort SPRVPOSL = 0xFC22; // "Current Vpos" Low byte
			public const ushort SPRVPOSH = 0xFC23; // "Current Vpos" High byte
			public const ushort COLLOFFL = 0xFC24; // "Offset to Collision Depository" Low byte
			public const ushort COLLOFFH = 0xFC25; // "Offset to Collision Depository" High byte
			public const ushort VSIZACUML = 0xFC26; // "Vertical Size Accumulator" Low byte
			public const ushort VSIZACUMH = 0xFC27; // "Vertical Size Accumulator" High byte
			public const ushort HSIZOFFL = 0xFC28; // "Horizontal Size Offset" Low byte
			public const ushort HSIZOFFH = 0xFC29; // "Horizontal Size Offset" High byte
			public const ushort VSIZOFFL = 0xFC2A; // "Vertical Size Offset" Low byte
			public const ushort VSIZOFFH = 0xFC2B; // "Vertical Size Offset" High byte
			public const ushort SCBADRL = 0xFC2C; // "Address of Current SCB" Low byte
			public const ushort SCBADRH = 0xFC2D; // "Address of Current SCB" High byte
			public const ushort PROCADRL = 0xFC2E; // "Current Spr Data Proc Address" Low byte
			public const ushort PROCADRH = 0xFC2F; // "Current Spr Data Proc Address" High byte

			public const ushort SPRCTL0 = 0xFC80; // "FC80 = SPRCTL0 Sprite Control Bits 0 (W)"
			public const ushort SPRCTL1 = 0xFC81; // "FC81 = SPRCTL1 Sprite Control Bits 1 (W)(U)"
			public const ushort SPRCOL = 0xFC82; // "FC82 = SPRCOLL. Sprite Collision Number (W)"
			public const ushort SPRINIT = 0xFC83; // "Sprite Initialization Bits (W)(U)"
			public const ushort SUZYBUSEN = 0xFC90; // "FC90 = SUZYBUSEN. Suzy Bus Enable (W)"
			public const ushort SPRGO = 0xFC91; // "FC91 = SPRG0. Sprite Process Start Bit (W)"
			public const ushort SPRSYS = 0xFC92; // "FC92 = SPRSYS. System Control Bits (R/W)"

			public const ushort SUZYHREV = 0xFC88; // Suzy Hardware Revision (R)
			public const ushort JOYSTICK = 0xFCB0; // "Read Joystick and Switches (R)"
			public const ushort SWITCHES = 0xFCB1; // "Read Other Switches (R)"
			public const ushort RCART0 = 0xFCB2; // RCART(R/W)
			public const ushort RCART1 = 0xFCB3; // RCART(R/W)
		}
	}
}
