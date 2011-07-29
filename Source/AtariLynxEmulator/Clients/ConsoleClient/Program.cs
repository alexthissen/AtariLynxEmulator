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
			//SpriteControlBlock scb = new SpriteControlBlock();
			//SpriteControlBlockVisualizer.TestShowVisualizer(scb);
			//return;

			string BootRomImageFilePath = @"D:\lynxboot.img";
			string CartRomImageFilePath = @"D:\roms\chips challenge.lnx";
			Stream bootRomImageStream;
			RomCart cartridge = null;

			bootRomImageStream = new FileStream(BootRomImageFilePath, FileMode.Open, FileAccess.Read);
			LnxRomImageFileFormat romImage = new LnxRomImageFileFormat();
			// TODO: Remove comment below
			//cartridge = romImage.LoadCart(CartRomImageFilePath); 

			LynxHandheld handheld = new LynxHandheld();
			handheld.BootRomImage = bootRomImageStream;
			handheld.Cartridge = cartridge;

			handheld.Initialize();

			while (true) handheld.Update(1);
		}
	}
}
