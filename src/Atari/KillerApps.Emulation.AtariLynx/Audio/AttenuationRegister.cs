using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.AtariLynx
{
	public class AttenuationRegister
	{
		private const byte LeftEarMask = 0xF0;
		private const byte RightEarMask = 0x0F;

		public byte ByteData { get; set; }

		// "B7-B4=Four bit attenuation for left ear"
		public byte LeftEar
		{
			get { return (byte)((ByteData & LeftEarMask) >> 4); }
		}

		// "B3-B0=Four bit attenuation for right ear"
		public byte RightEar
		{
			get { return (byte)(ByteData & RightEarMask); }
		}
	}
}
