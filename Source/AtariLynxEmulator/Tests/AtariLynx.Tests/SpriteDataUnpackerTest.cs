using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx;

namespace AtariLynx.Tests
{
	/// <summary>
	/// Summary description for SpriteDataUnpackerTest
	/// </summary>
	[TestClass]
	public class SpriteDataUnpackerTest
	{
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
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		byte[] data = new byte[] 
			{
				0x04, 0x30, 0xc0, 0x00, 0x04, 0x30, 0xc0, 0x00, 0x04, 0x30, 0xc0, 0x00, 0x05, 0x10, 0x04, 0x44,
				0x00, 0x05, 0x08, 0x0c, 0x42, 0x00, 0x05, 0x08, 0x0c, 0x42, 0x00, 0x05, 0x80, 0x14, 0x60, 0x00,
				0x03, 0x38, 0x00, 0x01, 0x04, 0x30, 0xc0, 0x00, 0x04, 0x28, 0x84, 0x00, 0x04, 0x20, 0x88, 0x00,
				0x04, 0x18, 0x8c, 0x00, 0x04, 0x10, 0x90, 0x00, 0x04, 0x08, 0x94, 0x00, 0x04, 0x80, 0x98, 0x00,
				0x03, 0x38, 0x00, 0x01, 0x04, 0x30, 0xc0, 0x00, 0x04, 0x28, 0x84, 0x00, 0x04, 0x20, 0x88, 0x00,
				0x04, 0x18, 0x8c, 0x00, 0x04, 0x10, 0x90, 0x00, 0x04, 0x08, 0x94, 0x00, 0x04, 0x80, 0x98, 0x00,
				0x03, 0x38, 0x00, 0x01, 0x04, 0x30, 0xc0, 0x00, 0x04, 0x30, 0xc0, 0x00, 0x04, 0x30, 0xc0, 0x00,
				0x05, 0x10, 0x04, 0x44, 0x00, 0x05, 0x08, 0x0c, 0x42, 0x00, 0x05, 0x08, 0x0c, 0x42, 0x00, 0x05,
				0x80, 0x14, 0x60, 0x00, 0x03, 0x38, 0x00, 0x00
			};

		[TestMethod]
		public void DeterminePacketTypeShouldReturnCorrectType()
		{
			// Arrange
			byte[] packet = new byte[] { 0x30, 0xc0, 0x00 };
			ShiftRegister register = new ShiftRegister(12);
			register.Initialize(new ArraySegment<byte>(packet));
			SpriteDataUnpacker unpacker = new SpriteDataUnpacker(register, packet);
			unpacker.Initialize(0, 4, false);
			
			// Act
			OrdinaryDataPacketType packetType = unpacker.ReadPacketType();

			// Assert
			Assert.AreEqual<OrdinaryDataPacketType>(OrdinaryDataPacketType.Packed, packetType, "Wrong packet type read from array.");
		}

		[TestMethod]
		public void ReadingEntireSpriteDataShouldSucceed()
		{
			ShiftRegister register = new ShiftRegister(12);
			SpriteDataUnpacker unpacker = new SpriteDataUnpacker(register, data);
			unpacker.Initialize(0, 4, false);

			// Act
			int quadrant = 1;
			int totalPixelCount = 0;

			while (true)
			{
				byte offset = unpacker.ReadOffsetToNextLine();
				if (offset == 1)
				{
					quadrant++;
					continue;
				}
				if (offset == 0) break;

				totalPixelCount += unpacker.PixelsInLine((byte)(offset - 1)).Count();
			}

			// Assert
			Assert.AreEqual<int>(0x7a, totalPixelCount, "Total number of pixels is not correct.");
			Assert.AreEqual<int>(4, quadrant, "Sprite data contains 4 quadrants.");
		}
	}
}
