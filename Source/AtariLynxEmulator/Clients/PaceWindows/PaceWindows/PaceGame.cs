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
using Microsoft.Xna.Framework.Storage;

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
		SpriteFont font;
		ContentManager romContent;

		Stream bootRomImageStream;
		StorageDevice storage = null;
		RomCart cartridge;

		public PaceGame()
		{
			Components.Add(new GamerServicesComponent(this));
			Components.Add(new FrameRateCounter(this));
			Content.RootDirectory = "Content";

			emulator = new LynxHandheld();
			graphics = new GraphicsDeviceManager(this);
			romContent = new ResourceContentManager(Services, Roms.ResourceManager);
		}

		private void EndShowSelector(IAsyncResult result)
		{
			storage = StorageDevice.EndShowSelector(result);
			if (storage != null && storage.IsConnected)
			{
				DoOpenFile(storage);
			}
		}

		private void DoOpenFile(StorageDevice storage)
		{
			IAsyncResult result = storage.BeginOpenContainer("Roms", null, null);

			// Wait for the WaitHandle to become signaled.
			result.AsyncWaitHandle.WaitOne();

			StorageContainer container = storage.EndOpenContainer(result);

			// Close wait handle
			result.AsyncWaitHandle.Close();

			bootRomImageStream = container.OpenFile("lynxboot.img", FileMode.Open, FileAccess.Read);
			OpenGameCart(container, "APB.lnx");
			container.Dispose();
		}

		private void OpenGameCart(StorageContainer container, string cartRomImageFileName)
		{
			LnxRomImageFileFormat romImage = new LnxRomImageFileFormat();
			using (Stream stream = container.OpenFile(cartRomImageFileName, FileMode.Open, FileAccess.Read))
			{
				cartridge = romImage.LoadCart(stream);
			}
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
			TargetElapsedTime = TimeSpan.FromMilliseconds(6); // 60Hz

			lcdScreen = new Texture2D(graphics.GraphicsDevice, 160, 102, false, SurfaceFormat.Color);

			//IAsyncResult result = StorageDevice.BeginShowSelector(EndShowSelector, "Storage for Player One");
			//result.AsyncWaitHandle.WaitOne();

			// Lynx related
			emulator.BootRomImage = new MemoryStream(Roms.LYNXBOOT);
			LnxRomImageFileFormat romImage = new LnxRomImageFileFormat();
			emulator.Cartridge = romImage.LoadCart(new MemoryStream(Roms.Collision));
			emulator.Initialize();

			Window.Title = "Portable Color Entertainment System";
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

			// Use this.Content to load your game content here
			font = Content.Load<SpriteFont>("DefaultFont");
		}

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
			KeyboardState keyboard = Keyboard.GetState(PlayerIndex.One);
			GamePadState gamePad = GamePad.GetState(PlayerIndex.One);

			// Allows the game to exit
			if (gamePad.Buttons.Back == ButtonState.Pressed)
				this.Exit();

			JoystickStates joystick = GetJoystickInput(keyboard, gamePad);
			emulator.UpdateJoystickState(joystick);
			emulator.Update(50000);

			base.Update(gameTime);
		}

		private JoystickStates GetJoystickInput(KeyboardState keyboard, GamePadState gamePad)
		{
			JoystickStates joystick = JoystickStates.None;
			if (gamePad.DPad.Down == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Down)) joystick |= JoystickStates.Down;
			if (gamePad.DPad.Up == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Up)) joystick |= JoystickStates.Up;
			if (gamePad.DPad.Left == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Left)) joystick |= JoystickStates.Left;
			if (gamePad.DPad.Right == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Right)) joystick |= JoystickStates.Right;
			if (gamePad.Buttons.A == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Z)) joystick |= JoystickStates.Outside;
			if (gamePad.Buttons.B == ButtonState.Pressed || keyboard.IsKeyDown(Keys.X)) joystick |= JoystickStates.Inside;
			if (gamePad.Buttons.X == ButtonState.Pressed || keyboard.IsKeyDown(Keys.D1)) joystick |= JoystickStates.Option1;
			if (gamePad.Buttons.Y == ButtonState.Pressed || keyboard.IsKeyDown(Keys.D2)) joystick |= JoystickStates.Option2;

			return joystick;
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			if (emulator.NewVideoFrameAvailable)
			{
				lcdScreen.SetData(emulator.LcdScreenDma, 0x0, 0x3FC0);
				emulator.NewVideoFrameAvailable = false;
			}

			spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null);
			spriteBatch.Draw(lcdScreen, new Rectangle(0, 0, 640, 408), new Rectangle(0, 0, 160, 102), Color.White);
			spriteBatch.DrawString(font, emulator.SystemClock.CompatibleCycleCount.ToString("X16"), new Vector2(10, 50), Color.White);
			spriteBatch.DrawString(font, gameTime.IsRunningSlowly.ToString(), new Vector2(10, 65), Color.White);
			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
