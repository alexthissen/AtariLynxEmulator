using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace KillerApps.Emulation.Atari.Lynx.Cryptography
{
	public class BootLoaderCryptoAlgorithm
	{
		public const int EncryptedBlockSize = 51;
		public const int DecryptedBlockSize = 50;

		static BootLoaderCryptoAlgorithm()
		{
			publicExponent = new BigInteger(LynxRsaCryptographicKeys.PublicExponent.Reverse().ToArray());
			privateExponent = new BigInteger(LynxRsaCryptographicKeys.PrivateExponent.Reverse().ToArray());
			modulus = new BigInteger(LynxRsaCryptographicKeys.PublicModulus.Reverse().ToArray());
		}

		private static BigInteger publicExponent, privateExponent, modulus;

		public byte[] Decrypt(byte[] block)
		{
			BigInteger data = new BigInteger(block);
			BigInteger decryptedData = BigInteger.ModPow(data, publicExponent, modulus);

			return decryptedData.ToByteArray();
		}

		internal void Obfuscate(byte[] data)
		{
			// decrypted 			  0x9c, 0xf9, 0xff, 0xa9, 0x03, 0x8d,
			// obfuscated				0x9c, 0x5d, 0x06, 0xaa, 0x5a, 0x8a, 
			byte accumulator = 0;
			for (int i = 0; i < data.Length; i++)
		  {
				byte value = data[i];
				data[i] -= accumulator;
				accumulator = value;
		  }
		}

		internal void Deobfuscate(byte[] data)
		{
			byte accumulator = 0;
			for (int i = 0; i < data.Length; i++)
			{
				accumulator += data[i];
				accumulator &= 0xFF;
				data[i] = (byte)accumulator;
			}
		}

		public byte[] Encrypt(byte[] block)
		{
			BigInteger data = new BigInteger(block);
			BigInteger decryptedData = BigInteger.ModPow(data, privateExponent, modulus);
			return decryptedData.ToByteArray();
		}
	}
}
