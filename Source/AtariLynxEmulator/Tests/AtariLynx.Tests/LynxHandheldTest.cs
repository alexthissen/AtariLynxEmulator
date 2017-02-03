using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using KillerApps.Emulation.Atari.Lynx;

namespace AtariLynx.Tests
{
	[TestClass]
	[DeploymentItem(@"Roms\LynxBoot.img")]
	[DeploymentItem(@"Roms\Collision.lnx")]
	public class LynxHandheldTest
	{
		public const string BootRomImageFilePath = @"lynxboot.img";
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

		[TestInitialize()]
		public void TestInitialize() 
		{
			bootRomImageStream = new FileStream(Path.Combine(TestContext.TestDeploymentDir, BootRomImageFilePath), FileMode.Open, FileAccess.Read);
			LnxRomImageFileFormat romImage = new LnxRomImageFileFormat();
			//cartridge = romImage.LoadCart(CartRomImageFilePath); 
		}

		[TestMethod]
		public void EmulatorShouldInitializeAtStartup()
		{
			// Arrange
			LynxHandheld handheld = new LynxHandheld();
			handheld.BootRomImage = bootRomImageStream;
			handheld.InsertCartridge(cartridge);

			// Act
			handheld.Initialize();

			// Assert
			Assert.AreEqual<ushort>(0xff80, handheld.Cpu.PC, "After reset program counter of processor should be at boot address.");
		}

		[TestMethod]
		[ExpectedException(typeof(LynxException), "Fake boot ROM image should be detected.")]
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
			handheld.InsertCartridge(cartridge);

			// Act
			handheld.Initialize();

			// Assert
			Assert.AreEqual<ushort>(0xff80, handheld.Cpu.PC, "After reset program counter of processor should be at boot address.");
		}
	}
}
