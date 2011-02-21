﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KillerApps.Emulation.Atari.Lynx
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

		public RomCart LoadCart(string location)
		{
			RomCart cart = null;

			using (FileStream stream = new FileStream(location, FileMode.Open, FileAccess.Read))
			{
				using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII))
				{
					// TODO: Handle incorrect file format more elegantly

					byte[] magic = reader.ReadBytes(4);
					if (magic[0] != 'L' || magic[1] != 'Y' || magic[2] != 'N' || magic[3] != 'X') return null;

					Bank0Size = reader.ReadUInt16();
					Bank1Size = reader.ReadUInt16();
					Version = reader.ReadUInt16();
					Title = Encoding.ASCII.GetString(reader.ReadBytes(TitleLength)).TrimEnd('\0');
					Manufacturer = Encoding.ASCII.GetString(reader.ReadBytes(ManufacturerLength)).TrimEnd('\0');
					Rotation = (CartRotation)reader.ReadByte();
					Spare = reader.ReadBytes(5);

					cart = new RomCart(Bank0Size, Bank1Size);
					cart.LoadFromStream(stream);
				}
			}

			return cart;
		}
	}
}