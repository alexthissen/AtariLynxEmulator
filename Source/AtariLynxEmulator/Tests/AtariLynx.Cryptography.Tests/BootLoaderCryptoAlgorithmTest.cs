using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx.Cryptography;

namespace AtariLynx.Encryption.Tests
{
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestClass]
	public class BootLoaderCryptoAlgorithmTest
	{
		public BootLoaderCryptoAlgorithmTest() { }

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
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
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		private static byte[] CreateCopy(byte[] data)
		{
			return CreateCopy(data, data.Length);
		}
	
		private static byte[] CreateCopy(byte[] data, int length)
		{
			byte[] copy = new byte[length];
			Array.Copy(data, copy, length);
			return copy;
		}	

		byte[] encryptedSingleBlockData = new byte[51]
			{
				0x88, 0x6c, 0x24, 0xd0, 0xf5, 0x9a, 0x62, 0x8c, 0xa1, 0x08, 0x7e, 0xda, 0x87, 0x3f, 0x1b, 0xeb, 
				0x48, 0x50, 0xba, 0x0d, 0xc9, 0xcb, 0x7b, 0x3e, 0x10, 0x7c, 0xfd, 0x7e, 0xde, 0x8c, 0x06, 0x3a, 
				0x12, 0x35, 0x1a, 0x8c, 0x74, 0x07, 0xdb, 0xd1, 0x60, 0x7e, 0xe5, 0x88, 0x90, 0x60, 0x5b, 0x2e, 
				0x4b, 0xa2, 0x25
			};

		byte[] decryptedSingleBlockData = new byte[51]
			{
				0x9c, 0x5d, 0x06, 0xaa, 0x5a, 0x8a, 0xfd, 0x73, 0xac, 0x5b, 0x89, 0xff, 0x71, 0xac, 0x5f, 0x85,
				0xfe, 0x72, 0xa5, 0x5e, 0xad, 0x05, 0x4a, 0xa1, 0x63, 0x03, 0xe5, 0xe8, 0x27, 0x55, 0xb4, 0x03, 
				0xfd, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
				0x00, 0x00, 0x15
			};

		byte[] obfuscatedSingleBlockData = new byte[50]
			{
				0x9c, 0x5d, 0x06, 0xaa, 0x5a, 0x8a, 0xfd, 0x73, 0xac, 0x5b, 0x89, 0xff, 0x71, 0xac, 0x5f, 0x85,
				0xfe, 0x72, 0xa5, 0x5e, 0xad, 0x05, 0x4a, 0xa1, 0x63, 0x03, 0xe5, 0xe8, 0x27, 0x55, 0xb4, 0x03, 
				0xfd, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
				0x00, 0x00
			};

		byte[] originalSingleBlockFrameData = new byte[50]
			{
			  0x9c, 0xf9, 0xff, 0xa9, 0x03, 0x8d, 0x8a, 0xfd, 0xa9, 0x04, 0x8d, 0x8c, 0xfd, 0xa9, 0x08, 0x8d, 
				0x8b, 0xfd, 0xa2, 0x00, 0xad, 0xb2, 0xfc, 0x9d, 0x00, 0x03, 0xe8, 0xd0, 0xf7, 0x4c, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00
			};

		[TestMethod]
		public void EncryptingBlockShouldReturnEncryptedData()
		{
			BootLoaderCryptoAlgorithm algorithm = new BootLoaderCryptoAlgorithm();
			byte[] encryptedData = algorithm.Encrypt(decryptedSingleBlockData);

			CollectionAssert.AreEqual(encryptedSingleBlockData, encryptedData, "Decrypted data is not same as known data.");
		}

		[TestMethod]
		public void DecryptingBlockShouldReturnObfuscatedData()
		{
			BootLoaderCryptoAlgorithm algorithm = new BootLoaderCryptoAlgorithm();
			byte[] decryptedData = algorithm.Decrypt(encryptedSingleBlockData);

			CollectionAssert.AreEqual(decryptedSingleBlockData, decryptedData, "Decrypted data is not same as known data.");
		}

		[TestMethod]
		public void ObfuscatingDataShouldObfuscateDataCorrectly()
		{
			BootLoaderCryptoAlgorithm algorithm = new BootLoaderCryptoAlgorithm();
			byte[] data = CreateCopy(originalSingleBlockFrameData);
			algorithm.Obfuscate(data);

			CollectionAssert.AreEqual(obfuscatedSingleBlockData, data, "Obfuscated data is not same as known data.");
		}

		[TestMethod]
		public void DeobfuscatingDataShouldChangeDataCorrectly()
		{
			BootLoaderCryptoAlgorithm algorithm = new BootLoaderCryptoAlgorithm();
			byte[] data = CreateCopy(obfuscatedSingleBlockData, 50);
			algorithm.Deobfuscate(data);

			CollectionAssert.AreEqual(originalSingleBlockFrameData, data, "Obfuscated data is not same as known data.");
		}
	}
}
