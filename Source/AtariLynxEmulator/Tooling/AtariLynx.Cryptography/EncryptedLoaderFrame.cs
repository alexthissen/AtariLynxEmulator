using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KillerApps.Emulation.Atari.Lynx.Cryptography
{
	public class EncryptedLoaderFrame
	{
		private byte accumulator;
		private byte[] encryptedData = null;

		private EncryptedLoaderFrame(byte[] data)
		{
			this.encryptedData = data;
		}

		public EncryptedLoaderFrame(int blockCount)
		{
			this.encryptedData = new byte[blockCount * BootLoaderCryptoAlgorithm.EncryptedBlockSize];
		}

		public int BlockCount
		{
			get { return 256 - Header; }
		}

		internal static bool IsValidHeader(byte header)
		{
			int blockCount = GetBlockCount(header);

			return (blockCount > 0 && blockCount <= 6);
		}

		internal static int GetBlockCount(byte header)
		{
			return 256 - header;
		}

		public byte Header { get { return (byte)(256 - encryptedData.Length / BootLoaderCryptoAlgorithm.EncryptedBlockSize); } }

		public byte[] Decrypt()
		{
			// Allocate memory for decrypted data
			byte[] decryptedData = new byte[BlockCount * BootLoaderCryptoAlgorithm.DecryptedBlockSize];
			BootLoaderCryptoAlgorithm algorithm = new BootLoaderCryptoAlgorithm();
			int offset = 0;
			
			// Decryption one block at a time
			foreach (byte[] encryptedBlock in Blocks)
			{
				byte[] decryptedBlock = algorithm.Decrypt(encryptedBlock);
				
				// Copy everything except last fixed byte (0x15)
				Array.Copy(decryptedBlock, 0, decryptedData, offset, decryptedBlock.Length - 1);
				offset += BootLoaderCryptoAlgorithm.DecryptedBlockSize;
			}

			// Deobfuscate all blocks in one go
			algorithm.Deobfuscate(decryptedData);

			return decryptedData;
		}

		internal IEnumerable<byte[]> Blocks
		{
			get
			{
				int offset = 0;
				byte[] blockData = new byte[BootLoaderCryptoAlgorithm.EncryptedBlockSize];
				while (offset < encryptedData.Length)
				{
					Array.Copy(encryptedData, offset, blockData, 0, blockData.Length);
					offset += BootLoaderCryptoAlgorithm.EncryptedBlockSize;
					yield return blockData;
				}
			}
		}

		public static EncryptedLoaderFrame Load(Stream encryptedStream)
		{
			byte header = (byte)encryptedStream.ReadByte();
			
			if (!IsValidHeader(header))
				throw new ArgumentException("Invalid header information in stream.");
			
			int frameLength = GetBlockCount(header) * BootLoaderCryptoAlgorithm.EncryptedBlockSize;
			byte[] data = new byte[frameLength];
			int bytesRead = encryptedStream.Read(data, 0, frameLength);

			if (bytesRead != frameLength)
				throw new ArgumentException("Stream does not contain enough data.");
			
			EncryptedLoaderFrame frame = new EncryptedLoaderFrame(data);
			return frame;
		}

		public static EncryptedLoaderFrame Create(byte[] unencryptedData)
		{
			int length = unencryptedData.Length;
			if (length % BootLoaderCryptoAlgorithm.DecryptedBlockSize != 0)
				throw new ArgumentException("Unencrypted data is not a multiple of 50 byte block size.");

			int blockCount = length / BootLoaderCryptoAlgorithm.DecryptedBlockSize;
			if (blockCount == 0 || blockCount > 5)
				throw new ArgumentException("Unencrypted data must be 1 to 5 blocks of 50 bytes.");
			
			EncryptedLoaderFrame frame = new EncryptedLoaderFrame(blockCount);
			frame.Encrypt(unencryptedData);

			return frame;
		}

		private void Encrypt(byte[] data)
		{
			BootLoaderCryptoAlgorithm algorithm = new BootLoaderCryptoAlgorithm();
			algorithm.Obfuscate(data);

			// Encrypt one block at a time
			byte[] block = new byte[BootLoaderCryptoAlgorithm.EncryptedBlockSize]; 
			for (int i = 0; i < BlockCount; i++)
			{
				Array.Copy(data, i * BootLoaderCryptoAlgorithm.DecryptedBlockSize, block, 0, BootLoaderCryptoAlgorithm.DecryptedBlockSize);
				block[50] = 0x15;
				
				byte[] encryptedBlock = algorithm.Encrypt(block);
				Array.Copy(encryptedBlock, 0, encryptedData, i * BootLoaderCryptoAlgorithm.EncryptedBlockSize, encryptedBlock.Length);
			}
		}

		public byte[] GetBytes() 
		{
			int frameLength = encryptedData.Length + 1;
			byte[] data = new byte[frameLength];
			Array.Copy(encryptedData, 0, data, 1, encryptedData.Length);
			data[0] = Header;
			return data; 
		}

		private static byte[] CreateCopy(byte[] data)
		{
			byte[] copy = new byte[data.Length];
			Array.Copy(data, copy, data.Length);
			return copy;
		}
	}
}