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
	public class PaceGame : Game
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
			graphics.PreferredBackBufferWidth = 640;
			graphics.PreferredBackBufferHeight = 408;
			graphics.IsFullScreen = false;
			graphics.ApplyChanges();

			IsFixedTimeStep = true;
			TargetElapsedTime = TimeSpan.FromMilliseconds(8); // 60Hz

			lcdScreen = new Texture2D(graphics.GraphicsDevice, 160, 102);
			Debug.WriteLine("SurfaceFormat: " + lcdScreen.Format.ToString());
			
			// Lynx related
			string bootRomImageFilePath = @"D:\lynxboot.img";
			string cartRomImageFilePath = @"D:\Roms\Robotron 2084.lnx";
			Stream bootRomImageStream;
			RomCart cartridge;

			bootRomImageStream = new FileStream(bootRomImageFilePath, FileMode.Open, FileAccess.Read);
			LnxRomImageFileFormat romImage = new LnxRomImageFileFormat();
			cartridge = romImage.LoadCart(cartRomImageFilePath);

			emulator.BootRomImage = bootRomImageStream;
			emulator.Cartridge = cartridge;

			emulator.Initialize();

			this.Window.Title = "Portable Color Entertainment System";
			Window.AllowUserResizing = false;
			
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
				ram[i] = 0x00;
			}
			
			// TODO: use this.Content to load your game content here
			font = Content.Load<SpriteFont>("DefaultFont");
		}

		SpriteFont font;
		
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
			GamePadState state = GamePad.GetState(PlayerIndex.One);
			// Allows the game to exit
			if (state.Buttons.Back == ButtonState.Pressed)
				this.Exit();

			JoyStickStates joystick = JoyStickStates.None;
			if (state.DPad.Down == ButtonState.Pressed) joystick |= JoyStickStates.Down;
			if (state.DPad.Up == ButtonState.Pressed) joystick |= JoyStickStates.Up;
			if (state.DPad.Left == ButtonState.Pressed) joystick |= JoyStickStates.Left;
			if (state.DPad.Right == ButtonState.Pressed) joystick |= JoyStickStates.Right;

			emulator.UpdateJoystickState(joystick);
			emulator.Update(50000);

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			lcdScreen.SetData(emulator.LcdScreenDma, 0x0, 0x3FC0 * 4);

			spriteBatch.Begin();
			spriteBatch.Draw(lcdScreen, new Rectangle(0, 0, 640, 408), new Rectangle(0, 0, 160, 102), Color.White);
			spriteBatch.DrawString(font, DateTime.Now.ToLongTimeString(), new Vector2(10, 10), Color.White);
			spriteBatch.DrawString(font, emulator.SystemClock.CompatibleCycleCount.ToString("X16"), new Vector2(10, 40), Color.White);
			spriteBatch.DrawString(font, gameTime.IsRunningSlowly.ToString(), new Vector2(10, 25), Color.White);
			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
