using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using KillerApps.Emulation.Atari.Lynx;

namespace AtariLynx.Tests
{
	/// <summary>
	/// Summary description for SuzyChipsetTest
	/// </summary>
	[TestClass]
	public class SuzyChipsetTest
	{
		Mock<ILynxDevice> device;
		SuzyChipset suzy;

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
			device = new Mock<ILynxDevice>();
			suzy = new SuzyChipset(device.Object);
		}

		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void IndividualMathABCDElementsShouldAmountToCorrectInteger()
		{
			suzy.Poke(SuzyAddresses.MATHD, 0x78);
			suzy.Poke(SuzyAddresses.MATHC, 0x56);
			suzy.Poke(SuzyAddresses.MATHB, 0x34);
			suzy.Poke(SuzyAddresses.MATHA, 0x12);
			
			// Assert
			Assert.AreEqual<uint>(0x12345678, BitConverter.ToUInt32(suzy.MathABCD, 0), "Composed integer does not have correct value.");
		}

		[TestMethod]
		public void IndividualMathJKLMElementsShouldAmountToCorrectInteger()
		{
			suzy.Poke(SuzyAddresses.MATHM, 0x78);
			suzy.Poke(SuzyAddresses.MATHL, 0x56);
			suzy.Poke(SuzyAddresses.MATHK, 0x34);
			suzy.Poke(SuzyAddresses.MATHJ, 0x12);

			// Assert
			Assert.AreEqual<uint>(0x12345678, BitConverter.ToUInt32(suzy.MathJKLM, 0), "Composed integer does not have correct value.");
		}

		[TestMethod]
		public void IndividualMathEFGHlementsShouldAmountToCorrectInteger()
		{
			suzy.Poke(SuzyAddresses.MATHH, 0x78);
			suzy.Poke(SuzyAddresses.MATHG, 0x56);
			suzy.Poke(SuzyAddresses.MATHF, 0x34);
			suzy.Poke(SuzyAddresses.MATHE, 0x12);

			// Assert
			Assert.AreEqual<uint>(0x12345678, BitConverter.ToUInt32(suzy.MathEFGH, 0), "Composed integer does not have correct value.");
		}

		[TestMethod]
		public void IndividualMathNPElementsShouldAmountToCorrectShort()
		{
			suzy.Poke(SuzyAddresses.MATHP, 0x34);
			suzy.Poke(SuzyAddresses.MATHN, 0x12);

			// Assert
			Assert.AreEqual<uint>(0x1234, BitConverter.ToUInt16(suzy.MathNP, 0), "Composed integer does not have correct value.");
		}

		[TestMethod]
		public void PeekToMathElementsShouldGivePokeValues()
		{
			// Act
			Tuple<ushort, int>[] startAddresses = 
				{ 
					new Tuple<ushort, int>(SuzyAddresses.MATHD, 4), 
					new Tuple<ushort, int>(SuzyAddresses.MATHP, 2),
					new Tuple<ushort, int>(SuzyAddresses.MATHH, 4),
					new Tuple<ushort, int>(SuzyAddresses.MATHM, 4)
				};

			foreach (var startAddress in startAddresses)
			{
				for (byte index = 0; index < startAddress.Item2; index++)
				{
					ushort address = (ushort)(startAddress.Item1 + index);

					// Act
					suzy.Poke(address, index);
					byte value = suzy.Peek(address);

					// Assert
					Assert.AreEqual<byte>(index, value, "Peek value is different from value poked at address {0:X4}", address);
				}
			}
		}

		[TestMethod]
		public void UnsignedMathMultiplyShouldCalculateCorrectProduct()
		{
			// Arrange
			suzy.MathABCD = BitConverter.GetBytes(0x78563412);
			
			// Act
			suzy.Multiply16By16();

			// Assert
			Assert.AreEqual<uint>(0x3412 * 0x7856, BitConverter.ToUInt32(suzy.MathEFGH, 0), "Multiplication result is incorrect.");
			Assert.IsFalse(suzy.SPRSYS.MathWarning, "No math warning should be given.");
		}

		[TestMethod]
		public void SignedMathMultiplyShouldCalculateCorrectProduct()
		{
			// Arrange
			suzy.Poke(SuzyAddresses.SPRSYS, 0x80); // Signed math enabled
			suzy.MathABCD = BitConverter.GetBytes(0xFFFF0002);
			
			// Act
			suzy.Multiply16By16();

			// Assert
			Assert.AreEqual<uint>(0xFFFFFFFE, BitConverter.ToUInt32(suzy.MathEFGH, 0), "Multiplication result should be negative.");
			Assert.IsFalse(suzy.SPRSYS.MathWarning, "No math warning should be given.");
		}

		[TestMethod]
		public void TwoNegativeValuesSignedMathMultiplicationShouldBePositiveProduct()
		{
			// Arrange
			suzy.Poke(SuzyAddresses.SPRSYS, 0x80); // Signed math enabled
			suzy.MathABCD = BitConverter.GetBytes(0xFFFEFFFF);
			
			// Act
			suzy.Multiply16By16();

			// Assert
			Assert.AreEqual<uint>(2, BitConverter.ToUInt32(suzy.MathEFGH, 0), "Multiplication result should be negative.");
			Assert.IsFalse(suzy.SPRSYS.MathWarning, "No math warning should be given.");
		}

		[TestMethod]
		public void MultiplicationsShouldAccumulateWhenSet()
		{
			// Arrange
			suzy.Poke(SuzyAddresses.SPRSYS, 0x40); // Accumulate
			suzy.MathABCD = BitConverter.GetBytes(0x00010002);
			
			// Act
			suzy.Multiply16By16();
			suzy.Multiply16By16();

			// Assert
			Assert.AreEqual<uint>(2*2, BitConverter.ToUInt32(suzy.MathJKLM, 0), "Accumulate result not correct.");
		}

		// "Note that you can actually initialize the accumulator to any value by writing to all 4 bytes (J,K,L,M)."
		public void MultiplicationsShouldContinueFromPresetAccumulation()
		{
			// Arrange
			suzy.Poke(SuzyAddresses.SPRSYS, 0x40); // Accumulate
			suzy.MathJKLM = BitConverter.GetBytes(0x01020304);
			suzy.MathABCD = BitConverter.GetBytes(0x00010002);
			
			// Act
			suzy.Multiply16By16();

			// Assert
			Assert.AreEqual<uint>(0x01020304 + 0x00010002, BitConverter.ToUInt32(suzy.MathJKLM, 0), "Accumulate result not correct.");
		}

		// "To initialize the accumulator, write a '0' to K and M (This will put 0 in J and L)."
		[TestMethod]
		public void InitializingAccumulatorWithKMShouldSetJLToZero()
		{
			// Arrange
			suzy.Poke(SuzyAddresses.MATHM, 0x00);
			suzy.Poke(SuzyAddresses.MATHK, 0x00);
			
			// Assert 
			Assert.AreEqual<uint>(0, BitConverter.ToUInt32(suzy.MathJKLM, 0), "J and L bytes should have been set to zero.");
		}

		[TestMethod]
		public void OverflowingAccumulatorShouldSetMathBit()
		{
			// Arrange
			suzy.SPRSYS.ByteData = 0x40; // Accumulate
			suzy.MathJKLM = BitConverter.GetBytes(0xFFFFFFFF);
			suzy.MathABCD = BitConverter.GetBytes(0xFFFFFFFF);

			// Act
			suzy.Multiply16By16();

			// Assert
			Assert.IsTrue(suzy.SPRSYS.LastCarry, "Accumulate math overflow bit should have been set.");
		}

		[TestMethod]
		public void DivideShouldCalculateCorrectValues()
		{
			// Act
			suzy.MathNP = BitConverter.GetBytes(0xFFFF);
			suzy.MathEFGH = BitConverter.GetBytes(0xFFFF0000);

			// Act
			suzy.Divide32By16();

			// Assert
			Assert.AreEqual<uint>(0x10000, BitConverter.ToUInt32(suzy.MathABCD, 0), "Division not performed correctly.");
			Assert.AreEqual<uint>(0, BitConverter.ToUInt32(suzy.MathJKLM, 0), "Remainder should be zero.");
		}
	}
}
