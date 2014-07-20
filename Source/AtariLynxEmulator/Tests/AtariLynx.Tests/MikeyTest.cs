using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx;
using Moq;

namespace AtariLynx.Tests
{
	[TestClass]
	public class MikeyTest
	{
		[TestMethod]
		public void PeekColorMapShouldReturnPokeValues()
		{
			// Arrange
			Mock<ILynxDevice> device = new Mock<ILynxDevice>();
			Mikey mikey = new Mikey(device.Object);

			// Act
			for (byte index = 0; index <= 0x0F; index++)
			{
				mikey.Poke((ushort)(Mikey.Addresses.GREEN0 + index), index);
				mikey.Poke((ushort)(Mikey.Addresses.BLUERED0 + index), (byte)((index << 4) + index));
			}
			
			// Assert
			for (byte index = 0; index <= 0x0F; index++)
			{
				Assert.AreEqual<byte>((byte)(16 * index), mikey.Peek((ushort)(Mikey.Addresses.GREEN0 + index)));
				Assert.AreEqual<byte>((byte)(16 * index), (byte)(mikey.Peek((ushort)(Mikey.Addresses.BLUERED0 + index)) >> 4));
				Assert.AreEqual<byte>((byte)(16 * index), (byte)(mikey.Peek((ushort)(Mikey.Addresses.BLUERED0 + index)) & 0x0F));
			}
		}

		[TestMethod]
		public void PokeColorMapShouldSetColorMapValues()
		{
			// Arrange
			Mock<ILynxDevice> device = new Mock<ILynxDevice>();
			Mikey mikey = new Mikey(device.Object);

			// Act
			for (byte index = 0; index <= 0x0F; index++)
			{
				mikey.Poke((ushort)(Mikey.Addresses.GREEN0 + index), index);
				mikey.Poke((ushort)(Mikey.Addresses.BLUERED0 + index), (byte)((index << 4) + index));
			}

			// Assert
			for (byte index = 0; index <= 0x0F; index++)
			{
				Assert.AreEqual<byte>(index, mikey.GreenColorMap[index], "Green map not set correctly.");
				Assert.AreEqual<byte>((byte)((index << 4) + index), mikey.BlueRedColorMap[index], "BlueRed map not set correctly.");
			}
		}
	}
}
