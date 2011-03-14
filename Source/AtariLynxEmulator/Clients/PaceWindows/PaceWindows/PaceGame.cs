using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using KillerApps.Emulation.Atari.Lynx;
using System.Diagnostics;
using System.IO;

namespace PaceWindows
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class PaceGame : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		LynxHandheld emulator;
		Texture2D lcdScreen;

		public PaceGame()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			emulator = new LynxHandheld();
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			//IsFixedTimeStep = false;
			//TargetElapsedTime = TimeSpan.FromMilliseconds(3000);

			lcdScreen = new Texture2D(graphics.GraphicsDevice, 160, 102);
			Debug.WriteLine("SurfaceFormat: " + lcdScreen.Format.ToString());
			
			// Lynx related
			string BootRomImageFilePath = @"D:\lynxboot.img";
			string CartRomImageFilePath = @"D:\slimeworld.lnx";
			Stream bootRomImageStream;
			RomCart cartridge;

			bootRomImageStream = new FileStream(BootRomImageFilePath, FileMode.Open, FileAccess.Read);
			LnxRomImageFileFormat romImage = new LnxRomImageFileFormat();
			cartridge = romImage.LoadCart(CartRomImageFilePath);

			emulator.BootRomImage = bootRomImageStream;
			emulator.Cartridge = cartridge;

			emulator.Initialize();

			this.Window.Title = "Portable Atari Console Entertainment emulator";
			Window.AllowUserResizing = true;
			
			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);
			byte[] ram = new byte[0x3FC0 * 4];
			for (int i = 0; i < 0x3FC0 * 4; i++)
			{
				ram[i] = 0x8f;
			}
			
			// TODO: use this.Content to load your game content here
			t2 = Content.Load<Texture2D>("Test");
			font = Content.Load<SpriteFont>("DefaultFont");
		}
		SpriteFont font;
		Texture2D t2;
		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();

			for (int i = 0; i < 10000; i++)
			{
				emulator.Update();
			} 

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			//GraphicsDevice.Clear(Color.CornflowerBlue);
			//lcdScreen.SetData(emulator.Ram.GetDirectAccess(), 0x0, 0x3FC0 * 4);
			lcdScreen.SetData(emulator.LcdScreenDma, 0x0, 0x3FC0 * 4);

			spriteBatch.Begin();
			spriteBatch.Draw(lcdScreen, new Rectangle(0, 0, 480, 306), new Rectangle(0, 0, 160, 102), Color.White);
			//spriteBatch.Draw(t2, new Rectangle(0, 0, 160, 102), Color.White);
			spriteBatch.DrawString(font, DateTime.Now.ToLongTimeString(), new Vector2(100, 100), Color.White);
			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
