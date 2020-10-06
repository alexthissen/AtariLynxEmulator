using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KillerApps.Emulation.AtariLynx
{
	public class LnxRomImageFileFormat
	{
		public CartRotation Rotation { get; private set; }
		public string Manufacturer { get; private set; }
		public string Title { get; private set; }
		public ushort Version { get; private set; }
		protected ushort Bank0Size { get; private set; }
		protected ushort Bank1Size { get; private set; }
		public byte[] Spare { get; private set; }

		public int SizeInBytes
		{
			get { return (Bank0Size + Bank1Size) * 256; }
		}

		public const int TitleLength = 32;
		public const int ManufacturerLength = 16;

		public LnxRomImageFileFormat()
		{
			Rotation = CartRotation.None;
		}

		public RomCart LoadCart(Stream stream)
		{
			Eeprom93C46BCart cart = null;

			using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8))
			{
				// TODO: Handle incorrect file format more elegantly

				byte[] magic = reader.ReadBytes(4);
				if (magic[0] != 'L' || magic[1] != 'Y' || magic[2] != 'N' || magic[3] != 'X')
					throw new LynxException("LNX Cartridge format is incorrect. Magic bytes are not present.");

				Bank0Size = reader.ReadUInt16();
				Bank1Size = reader.ReadUInt16();
				Version = reader.ReadUInt16();
				Title = Encoding.UTF8.GetString(reader.ReadBytes(TitleLength), 0, TitleLength).TrimEnd('\0');
				Manufacturer = Encoding.UTF8.GetString(reader.ReadBytes(ManufacturerLength), 0, ManufacturerLength).TrimEnd('\0');
				Rotation = (CartRotation)reader.ReadByte();
				Spare = reader.ReadBytes(5);

				cart = new Eeprom93C46BCart(Bank0Size, Bank1Size);
				cart.LoadFromStream(stream);
			}

			return cart;
		}

		//public RomCart LoadCart(string location)
		//{
		//  RomCart cart = null;

		//  using (FileStream stream = new FileStream(location, FileMode.Open, FileAccess.Read))
		//  {
		//    cart = LoadCart(stream);
		//  }

		//  return cart;
		//}
	}
}
