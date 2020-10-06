using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KillerApps.Emulation.AtariLynx
{
	public class BllRomImageFileFormat
	{
		public BllRomImageHeader Header { get; private set; }
		private byte[] bytes;

		public byte[] Bytes
		{
			get { return bytes; }
		}

		public RomCart LoadCart(Stream stream)
		{
			using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8))
			{
				BllRomImageHeader header;
				header.Jump = reader.ReadUInt16();
				header.LoadAddress = (ushort)(reader.ReadUInt16(true) - 10); // Header bytes need to be loaded as well
				header.Size = reader.ReadUInt16(true);
				byte[] magic = reader.ReadBytes(4);
				if (magic[0] != 'B' || magic[1] != 'S' || magic[2] != '9' || magic[3] != '3')
					throw new LynxException("BLL Cartridge format is incorrect. Magic bytes are not present.");
				Header = header;

				bytes = new byte[header.Size];
				reader.BaseStream.Position = 0;
				int bytesRead = reader.Read(bytes, 0, header.Size);
				if (bytesRead != header.Size)
					throw new LynxException("Size of cartridge is not correct.");

				return new RomCart(0, 0);
			}
		}
	}
}
