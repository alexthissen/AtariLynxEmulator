using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Atari.Lynx;
using System.IO;

namespace ConsoleClient
{
	class Program
	{
		static void Main(string[] args)
		{
			string BootRomImageFilePath = @"D:\lynxboot.img";
			string CartRomImageFilePath = @"D:\game.lnx";
			Stream bootRomImageStream;
			RomCart cartridge;

			bootRomImageStream = new FileStream(BootRomImageFilePath, FileMode.Open, FileAccess.Read);
			LnxRomImageFileFormat romImage = new LnxRomImageFileFormat();
			cartridge = romImage.LoadCart(CartRomImageFilePath); 

			LynxHandheld handheld = new LynxHandheld();
			handheld.BootRomImage = bootRomImageStream;
			handheld.Cartridge = cartridge;

			handheld.Initialize();

			while (true) handheld.Update(1);
		}
	}
}
