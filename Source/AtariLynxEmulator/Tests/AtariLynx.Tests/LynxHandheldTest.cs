using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using KillerApps.Emulation.Atari.Lynx;

namespace AtariLynx.Tests
{
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestClass]
	[DeploymentItem(@"Roms\LynxBoot.img")]
	[DeploymentItem(@"Roms\Collision.lnx")]
	public class LynxHandheldTest
	{
		public const string BootRomImageFilePath = @"LynxBoot.img";
		public const string CartRomImageFilePath = @"Collision.lnx";
		Stream bootRomImageStream;
		RomCart cartridge;

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get { return testContextInstance; }
			set { testContextInstance = value; }
		}

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
			bootRomImageStream = new FileStream(Path.Combine(TestContext.TestDeploymentDir, BootRomImageFilePath), FileMode.Open, FileAccess.Read);
			LnxRomImageFileFormat romImage = new LnxRomImageFileFormat();
			//cartridge = romImage.LoadCart(CartRomImageFilePath); 
		}

		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void EmulatorShouldInitializeAtStartup()
		{
			// Arrange
			LynxHandheld handheld = new LynxHandheld();
			handheld.BootRomImage = bootRomImageStream;
			handheld.Cartridge = cartridge;

			// Act
			handheld.Initialize();

			// Assert
			Assert.AreEqual<ushort>(0xff80, handheld.Cpu.PC, "After reset program counter of processor should be at boot address.");
		}

		[TestMethod]
		[ExpectedException(typeof(LynxException), "Boot image file appears to be fake.")]
		public void EmulatorShouldThrowForFakeBootRomAtStartup()
		{
			// Arrange
			LynxHandheld handheld = new LynxHandheld();
			handheld.BootRomImage =
				new MemoryStream(new byte[] 
				{ 
					0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
					0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF 
				});
			handheld.Cartridge = cartridge;

			// Act
			handheld.Initialize();

			// Assert
			Assert.AreEqual<ushort>(0xff80, handheld.Cpu.PC, "After reset program counter of processor should be at boot address.");
		}
	}
}
