using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Core;
using KillerApps.Emulation.Atari.Lynx;
using Moq;
using KillerApps.Emulation.Processors;

namespace AtariLynx.Tests
{
	/// <summary>
	/// Summary description for MemoryManagementUnitTest
	/// </summary>
	[TestClass]
	public class MemoryManagementUnitTest
	{
		IMemoryAccess<ushort, byte> rom, mikey, suzy;
		IDirectMemoryAccess<ushort, byte> ram;
		MemoryManagementUnit mmu = null;

		public const byte romValue = 0x11;
		public const byte ramValue = 0x22;
		public const int ramSize = 0x10000;
		public const byte suzyValue = 0x33;
		public const byte mikeyValue = 0x44;

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
		public void MyTestInitialize() 
		{
			rom = CreateMockMemory(0xfe00, 0xffff, romValue);
			ram = CreateMockMemoryWithDirectAccess(0x0000, 0xffff, ramValue, ramSize);
			suzy = CreateMockMemory(0xfc00, 0xfcff, suzyValue);
			mikey = CreateMockMemory(0xfd00, 0xfdff, mikeyValue);
			
			mmu = new MemoryManagementUnit(rom, ram, mikey, suzy);
		}

		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		private IMemoryAccess<ushort, byte> CreateMockMemory(ushort from, ushort to, byte value)
		{
			Mock<IMemoryAccess<ushort, byte>> mockMemory = new Mock<IMemoryAccess<ushort, byte>>();
			mockMemory.Setup(memory => memory.Peek(It.IsInRange<ushort>(from, to, Range.Inclusive))).Returns(value);
			return mockMemory.Object;
		}

		private IDirectMemoryAccess<ushort, byte> CreateMockMemoryWithDirectAccess(ushort from, ushort to, byte value, int size)
		{
			Mock<IDirectMemoryAccess<ushort, byte>> mockMemory = new Mock<IDirectMemoryAccess<ushort, byte>>();
			mockMemory.Setup(memory => memory.Peek(It.IsInRange<ushort>(from, to, Range.Inclusive))).Returns(value);
			byte[] dmaMemory = Enumerable.Repeat(value, size).ToArray();
			mockMemory.Setup(memory => memory.GetDirectAccess()).Returns(dmaMemory);
			return mockMemory.Object;
		}

		private Mock<IMemoryAccess<ushort, byte>> GetMock(IMemoryAccess<ushort, byte> mocked)
		{
			Mock<IMemoryAccess<ushort, byte>> mock = Mock.Get<IMemoryAccess<ushort, byte>>(mocked);
			return mock;
		}

		[TestMethod]
		public void MmuShouldMapToRamMemoryForRamSpaceWhenEnabled()
		{
			// Arrange
			mmu.Poke(LynxAddresses.MAPCTL, 0);
			ushort address = 0x1337;

			// Act
			byte value = mmu.Peek(address);

			Assert.AreEqual<byte>(value, ramValue, "Value from peek at RAM not correct.");
			Mock.Get(ram).Verify(r => r.Peek(address), Times.Once(), "Peek to RAM needs to be called exactly once.");
		}

		[TestMethod]
		public void MmuShouldMapToRomMemoryForRomSpaceWhenEnabled()
		{
			// Arrange
			mmu.Poke(LynxAddresses.MAPCTL, 0);
			ushort address = 0xfe00;

			// Act
			byte value = mmu.Peek(address);

			Assert.AreEqual<byte>(value, romValue, "Value from peek at ROM not correct.");
			Mock.Get(rom).Verify(r => r.Peek(address), Times.Once(), "Peek to RAM needs to be called exactly once.");
		}

		[TestMethod]
		public void MmuShouldMapToRomMemoryForVectorSpaceWhenEnabled()
		{
			// Arrange
			mmu.Poke(LynxAddresses.MAPCTL, 0);
			ushort address = VectorAddresses.IRQ_VECTOR;

			// Act
			byte value = mmu.Peek(address);

			Assert.AreEqual<byte>(value, romValue, "Value from peek at IRQ vector not correct.");
			Mock.Get(rom).Verify(r => r.Peek(address), Times.Once(), "Peek to IRQ vector needs to be called exactly once.");
		}

		[TestMethod]
		public void MmuShouldMapToMikeyForMikeySpaceWhenEnabled()
		{
			// Arrange
			mmu.Poke(LynxAddresses.MAPCTL, 0);
			ushort address = Mikey.Addresses.INTSET;

			// Act
			byte value = mmu.Peek(address);

			Assert.AreEqual<byte>(value, mikeyValue, "Value from peek at Mikey not correct.");
			Mock.Get(mikey).Verify(r => r.Peek(address), Times.Once(), "Peek to Mikey needs to be called exactly once.");
		}

		[TestMethod]
		public void MmuShouldMapToSuzyForSuzySpaceWhenEnabled()
		{
			// Arrange
			mmu.Poke(LynxAddresses.MAPCTL, 0);
			ushort address = Suzy.Addresses.RCART0;

			// Act
			byte value = mmu.Peek(address);

			Assert.AreEqual<byte>(value, suzyValue, "Value from peek at Suzy not correct.");
			Mock.Get(suzy).Verify(r => r.Peek(address), Times.Once(), "Peek to Suzy needs to be called exactly once.");
		}

		[TestMethod]
		public void MmuShouldAlwaysMapToRamWhenSpacesDisabled()
		{
			// Arrange
			mmu.Poke(LynxAddresses.MAPCTL, 0x0f);
			
			// Act
			byte value = mmu.Peek(0x1337);
			mmu.Peek(0xfe00);
			mmu.Peek(Suzy.Addresses.RCART0);
			mmu.Peek(Mikey.Addresses.INTSET);
			mmu.Peek(VectorAddresses.IRQ_VECTOR);

			Assert.AreEqual<byte>(value, ramValue, "Value from peek at RAM not correct.");

			Mock.Get(ram).Verify(r => r.Peek(It.IsAny<ushort>()), Times.Exactly(5), "All peeks to memory must be mapped to RAM when spaces are disabled.");
			Mock.Get(rom).Verify(r => r.Peek(It.IsAny<ushort>()), Times.Never(), "Peek to Suzy must be mapped to RAM.");
			Mock.Get(mikey).Verify(r => r.Peek(It.IsAny<ushort>()), Times.Never(), "Peek to Suzy must be mapped to RAM.");
			Mock.Get(suzy).Verify(r => r.Peek(It.IsAny<ushort>()), Times.Never(), "Peek to Suzy must be mapped to RAM.");
		}
	}
}
