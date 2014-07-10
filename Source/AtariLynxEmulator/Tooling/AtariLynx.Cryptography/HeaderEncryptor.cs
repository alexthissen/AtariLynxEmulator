using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx.Cryptography
{
	class HeaderEncryptor
	{
		int[] bchk = new int[] { 410, 512, 0 };

		public void BuildCheck(int romsize)
		{
			romsize = 128 * 1024;
			string fileName = String.Empty;
			byte[] image = new byte[romsize];
			Stream file = File.OpenRead(@"D:\SkyDrive\Gaming\Atari\Lynx\Community\Robert Maidorn\Quadromania2.rom");
			int romLength = file.Read(image, 0, romsize);

			int i, j, k, pagesize = romsize / 256;

			/* Search for directory entries */
			i = 0;
			do
			{
				j = bchk[i++];
				k = j + FileEntry.ROMDIR_ENTRY_SIZE;
				if ((image[j + FileEntry.ROMDIR_DEST + 1] >= 0x24) &&
					(image[k + FileEntry.ROMDIR_DEST + 1] >= 0x04) &&
					(image[j + FileEntry.ROMDIR_OFFSET + 1] < (pagesize >> 8)) &&
					(image[k + FileEntry.ROMDIR_OFFSET + 1] < (pagesize >> 8)) &&
					(image[j + FileEntry.ROMDIR_DEST] + (image[j + FileEntry.ROMDIR_DEST + 1] << 8) + image[j + FileEntry.ROMDIR_SIZE] + (image[j + FileEntry.ROMDIR_SIZE + 1] << 8) <= 0xfc00) &&
					(image[k + FileEntry.ROMDIR_DEST] + (image[k + FileEntry.ROMDIR_DEST + 1] << 8) + image[k + FileEntry.ROMDIR_SIZE] + (image[k + FileEntry.ROMDIR_SIZE + 1] << 8) <= 0xfc00)
					)
					break;
			} while (i < 3);

			// Write entries to ROM header
			//Console.WriteLine("{0:X2} {1:X2} {2:X2} {3:X2} {4:X2} {5:X2} {6:X2}",
			//	image[j + ROMDIR_PAGE],
			//	image[j + ROMDIR_OFFSET + 1],
			//	image[j + ROMDIR_OFFSET],
			//	image[j + ROMDIR_DEST + 1],
			//	image[j + ROMDIR_DEST],
			//	image[j + ROMDIR_SIZE + 1],
			//	image[j + ROMDIR_SIZE]
			//);
			//j += ROMDIR_ENTRY_SIZE;

			//Console.WriteLine("{0:X2} {1:X2} {2:X2} {3:X2} {4:X2} {5:X2} {6:X2}",
			//	image[j + ROMDIR_PAGE],
			//	image[j + ROMDIR_OFFSET + 1],
			//	image[j + ROMDIR_OFFSET],
			//	image[j + ROMDIR_DEST + 1],
			//	image[j + ROMDIR_DEST],
			//	image[j + ROMDIR_SIZE + 1],
			//	image[j + ROMDIR_SIZE]
			//);
		}
	}
}
