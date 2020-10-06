using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.AtariLynx
{
	public partial class Mikey
	{
		public static class Addresses
		{
			// "FD00 -> FD03 Timer channel 0 and Hcount
			// FD04 -> FD07 Timer channel 1 and mag0a (read current state of TAPE0 in b7)
			// FD08 -> FD0B Timer channel 2 and Vcount
			// FD0C -> FD0F Timer channel 3 and mag0b
			// FD10 -> FD13 Timer channel 4 and serial rate
			// FD14 -> FD17 Timer channel 5 and mag1a (read current state of TAPE1 in b7)
			// FD18 -> FD1B Timer channel 6
			// FD1C -> FD1F Timer channel 7 and mag1b"
			public const ushort HTIMBKUP = 0xFD00; // "Timer 0 backup value"
			public const ushort TIM7CTLB = 0xFD1F; // "Timer 7 dynamic control"

			public const ushort AUD0VOL = 0xFD20; // "Audio 0 volume value"
			public const ushort AUD3MISC = 0xFD3F; // "Audio 3 other bits"

			public const ushort ATTENREG0 = 0xFD40;
			public const ushort ATTENREG1 = 0xFD41;
			public const ushort ATTENREG2 = 0xFD42;
			public const ushort ATTENREG3 = 0xFD43;
			public const ushort MPAN = 0xFD44;
			public const ushort MSTEREO = 0xFD50;

			// "FD51 -> FD7F = not yet allocated"

			public const ushort INTRST = 0xFD80; // "INTRST.Interrupt Poll 0, (R/W)"
			public const ushort INTSET = 0xFD81; // "INTSET. Interrupt Poll 1, (R/W)"
			
			// "FD82 -> FD83 = not yet allocated"

			public const ushort MAGRDY0 = 0xFD84; // "Mag Tape Channel 0 ready bit.(R)"
			public const ushort MAGRDY1 = 0xFD85; // "Mag Tape Channel 1 ready bit.(R)"
			public const ushort AUDIN = 0xFD86; // "Audio In (R)"
			public const ushort SYSCTL1 = 0xFD87; // "SYSCTL1.Control Bits.(W)"
			public const ushort IODIR = 0xFD8A; // "Mikey Parallel I/O Data Direction (W)"
			public const ushort IODAT = 0xFD8B; // "Mikey Parallel Data (sort of a R/W)"
			public const ushort MIKEYHREV = 0xFD88; // "Mikey Hardware Revision, (W)"
			public const ushort MIKEYSREV = 0xFD89; // "Mikey Software Revision, (W)"
			public const ushort SERCTL = 0xFD8C; // "Serial Control Register.(R/W)"
			public const ushort SERDAT = 0xFD8D; // "Serial Data (R/W)"
			public const ushort SDONEACK = 0xFD90; // "Suzy Done Acknowledge, (W)"
			public const ushort CPUSLEEP = 0xFD91; // "CPU Bus Request Disable(W)"
			public const ushort DISPCTL = 0xFD92; // "Video Bus Request Enable, (W)"
			public const ushort PBKUP = 0xFD93; // "Magic 'P' count, (W)"
			public const ushort DISPADRL = 0xFD94; // "Start Address of Video Display, (W)"
			public const ushort DISPADRH = 0xFD95; // "Start Address of Video Display, (W)"

			// "FD96 -> FD9B = not yet allocated"

			public const ushort GREEN0 = 0xFDA0; // "Green color map 0, (R/W)"
			public const ushort GREEN1 = 0xFDA1; // "Green color map 1, (R/W)"
			public const ushort GREEN2 = 0xFDA2; // "Green color map 2, (R/W)"
			public const ushort GREEN3 = 0xFDA3; // "Green color map 3, (R/W)"
			public const ushort GREEN4 = 0xFDA4; // "Green color map 4, (R/W)"
			public const ushort GREEN5 = 0xFDA5; // "Green color map 5, (R/W)"
			public const ushort GREEN6 = 0xFDA6; // "Green color map 6, (R/W)"
			public const ushort GREEN7 = 0xFDA7; // "Green color map 7, (R/W)"
			public const ushort GREEN8 = 0xFDA8; // "Green color map 8, (R/W)"
			public const ushort GREEN9 = 0xFDA9; // "Green color map 9, (R/W)"
			public const ushort GREENA = 0xFDAA; // "Green color map 10, (R/W)"
			public const ushort GREENB = 0xFDAB; // "Green color map 11, (R/W)"
			public const ushort GREENC = 0xFDAC; // "Green color map 12, (R/W)"
			public const ushort GREEND = 0xFDAD; // "Green color map 13, (R/W)"
			public const ushort GREENE = 0xFDAE; // "Green color map 14, (R/W)"
			public const ushort GREENF = 0xFDAF; // "Green color map 15, (R/W)"
			public const ushort BLUERED0 = 0xFDB0; // "Blue and Red color map 0, (R/W)"
			public const ushort BLUERED1 = 0xFDB1; // "Blue and Red color map 1, (R/W)"
			public const ushort BLUERED2 = 0xFDB2; // "Blue and Red color map 2, (R/W)"
			public const ushort BLUERED3 = 0xFDB3; // "Blue and Red color map 3, (R/W)"
			public const ushort BLUERED4 = 0xFDB4; // "Blue and Red color map 4, (R/W)"
			public const ushort BLUERED5 = 0xFDB5; // "Blue and Red color map 5, (R/W)"
			public const ushort BLUERED6 = 0xFDB6; // "Blue and Red color map 6, (R/W)"
			public const ushort BLUERED7 = 0xFDB7; // "Blue and Red color map 7, (R/W)"
			public const ushort BLUERED8 = 0xFDB8; // "Blue and Red color map 8, (R/W)"
			public const ushort BLUERED9 = 0xFDB9; // "Blue and Red color map 9, (R/W)"
			public const ushort BLUEREDA = 0xFDBA; // "Blue and Red color map 10, (R/W)"
			public const ushort BLUEREDB = 0xFDBB; // "Blue and Red color map 11, (R/W)"
			public const ushort BLUEREDC = 0xFDBC; // "Blue and Red color map 12, (R/W)"
			public const ushort BLUEREDD = 0xFDBD; // "Blue and Red color map 13, (R/W)"
			public const ushort BLUEREDE = 0xFDBE; // "Blue and Red color map 14, (R/W)"
			public const ushort BLUEREDF = 0xFDBF; // "Blue and Red color map 15, (R/W)"
		}
	}
}