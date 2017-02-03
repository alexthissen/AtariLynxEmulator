using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx.Cryptography;

namespace AtariLynx.Encryption.Tests
{
	[TestClass]
	public class FileEntryTest
	{
		private byte[] romImage = new byte[] {
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE2, 0x01, 0x00, 0x00, 0x24,
            0xF1, 0x09, 0x05, 0xD3, 0x01, 0x00, 0x00, 0x08, 0xCC, 0x8F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
		};
		private byte[] singleFileEntry = new byte[] {
			0x02, 0xE2, 0x01, 0x88, 0x00, 0x24, 0xF1, 0x09
		};

		[TestMethod]
		public void DeserializeSingleFileEntryShouldGiveCorrectProperties()
		{
//			FileStream romImageStream = new FileStream(
	//			Path.Combine(TestContext.TestDeploymentDir, RomImageFilePath), FileMode.Open, FileAccess.Read);
			FileEntry entry = FileEntry.FromByteArray(singleFileEntry);

			Assert.AreEqual<byte>(0x02, entry.PageOffset, "Page offset for file not correct.");
			Assert.AreEqual<ushort>(0x01E2, entry.ByteOffset, "Byte offset for file entry not correct.");
			Assert.AreEqual<byte>(0x88, entry.Flag, "Flag for file entry not correct.");
			Assert.AreEqual<ushort>(0x2400, entry.RamDestination, "Ram destination for file not correct.");
			Assert.AreEqual<ushort>(0x09F1, entry.FileSize, "File size for file not correct.");
		}
	}
}