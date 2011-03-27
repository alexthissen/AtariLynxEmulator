using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx;

namespace AtariLynx.Tests
{
	[TestClass]
	public class ShiftRegisterTest
	{
		[TestMethod]
		public void ConstructorShouldSetCorrectSize()
		{
			// Arrange
			int size = 12;

			// Act
			ShiftRegister register = new ShiftRegister(size);

			// Assert
			Assert.AreEqual<int>(size, register.Size, "Size has not been set correctly by constructor.");
		}

		[TestMethod]
		public void InitializeShouldPrepareRegister()
		{
			// Arrange
			ArraySegment<byte> data = new ArraySegment<byte>(new byte[512], 100, 200);
			int size = 12;
			ShiftRegister shifter = new ShiftRegister(size);

			// Act
			shifter.Initialize(data);

			// Assert
			Assert.AreEqual<int>(200 * 8, shifter.BitsLeft, "Bits left is not correct after calling Initialize.");
		}

		[TestMethod]
		public void GetBitsShouldReturnCorrectBits()
		{
			ArraySegment<byte> data = new ArraySegment<byte>(new byte[] { 0xFF, 0xFF, 0x00 });
			int size = 12;
			ShiftRegister shifter = new ShiftRegister(size);
			shifter.Initialize(data);
			int bitsBefore = shifter.BitsLeft;

			// Act
			byte value = shifter.GetBits(8);
			int bitsAfter = shifter.BitsLeft;

			// Assert
			Assert.AreEqual<byte>(0xFF, value, "Value from GetBits did not match first elements from source data.");
			Assert.AreEqual<int>(8, bitsBefore - bitsAfter, "Number of bits read does not match bit difference.");
		}

		[TestMethod]
		public void GetBitsWithInsufficientBitsShouldAddBytes()
		{
			ArraySegment<byte> data = new ArraySegment<byte>(new byte[] { 0xFF, 0x00 });
			int size = 12;
			ShiftRegister shifter = new ShiftRegister(size);
			shifter.Initialize(data);
			shifter.GetBits(4);

			// Act
			byte value;
			shifter.TryGetBits(8, out value);
			
			// Assert
			Assert.AreEqual<byte>(0xF0, value, "Value from GetBits did not match first elements from source data.");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void GettingMoreBitsThanRemainingShouldThrowException()
		{
			ArraySegment<byte> data = new ArraySegment<byte>(new byte[] { 0xFF });
			int size = 12;
			ShiftRegister shifter = new ShiftRegister(size);
			shifter.Initialize(data);
			shifter.GetBits(4);

			// Act
			shifter.GetBits(8);
		}

		[TestMethod]
		public void TryGettingMoreBitsThanRemainingShouldReturnFalse()
		{
			ArraySegment<byte> data = new ArraySegment<byte>(new byte[] { 0xFF });
			int size = 12;
			ShiftRegister shifter = new ShiftRegister(size);
			shifter.Initialize(data);
			shifter.GetBits(4);

			// Act
			byte value;
			bool success = shifter.TryGetBits(8, out value);

			// Assert
			Assert.IsFalse(success, "Trying to get more bits should return false");
		}
	}
}
