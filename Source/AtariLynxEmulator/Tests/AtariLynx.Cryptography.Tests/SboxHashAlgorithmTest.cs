using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using KillerApps.Emulation.Atari.Lynx.Cryptography;

namespace AtariLynx.Encryption.Tests
{
	[TestClass]
	public class SboxHashAlgorithmTest
	{
		private TestContext testContextInstance;
		public const string RomImage128KFilePath = @"Quadromania.bin";
		public const string RomImage256KFilePath = @"APB.bin";
		private const int HeaderOffset = 410; 
		
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get { return testContextInstance; }
			set { testContextInstance = value; }
		}

		private readonly byte[] QuadroManiaHashValue = new byte[] 
		{ 
			0xAF, 0xC0, 0x08, 0x1E, 0xFE, 0x47, 0x4F, 0x37, 
			0xE1, 0xA2, 0xE0, 0x50, 0x34, 0x02, 0x8A, 0x24 
		};

		private readonly byte[] ApbHashValue = new byte[]
		{
			0x65, 0x39, 0x1C, 0xC5, 0x6C, 0x6D, 0x55, 0x6C, 
			0xCB, 0xA3, 0x41, 0xBB, 0xF1, 0xD8, 0xC5, 0x7C
		};

		[DeploymentItem(@"Binaries\" + RomImage128KFilePath)]
		[TestMethod]
		public void ComputeHashFor128KImageShouldReturnCorrectValue()
		{
			FileStream romImageStream = new FileStream(
				Path.Combine(TestContext.TestDeploymentDir, RomImage128KFilePath), FileMode.Open, FileAccess.Read);
			int romLength = (int)romImageStream.Length;
			byte[] image = new byte[romLength];
			int bytesRead	= romImageStream.Read(image, 0, romLength);

			SboxHashAlgorithm algorithm = SboxHashAlgorithm.Create(romLength);
			byte[] hash = algorithm.ComputeHash(image, 0, romLength);

			Assert.AreEqual<int>(romLength, bytesRead, "Data read is not same length as expected length");
			CollectionAssert.AreEqual(QuadroManiaHashValue, hash, "Computed hash is not correct.");
		}

		[DeploymentItem(@"Binaries\" + RomImage256KFilePath)]
		[TestMethod]
		public void ComputeHashFor256KImageShouldReturnCorrectValue()
		{
			FileStream romImageStream = new FileStream(
				Path.Combine(TestContext.TestDeploymentDir, RomImage256KFilePath), FileMode.Open, FileAccess.Read);
			int romLength = (int)romImageStream.Length;
			byte[] image = new byte[romLength];
			int bytesRead = romImageStream.Read(image, 0, romLength);

			SboxHashAlgorithm algorithm = SboxHashAlgorithm.Create(romLength);
			byte[] hash = algorithm.ComputeHash(image, 0, romLength);

			Assert.AreEqual<int>(romLength, bytesRead, "Data read is not same length as expected length");
			CollectionAssert.AreEqual(ApbHashValue, hash, "Computed hash is not correct.");
		}
	}
}
