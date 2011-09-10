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
using KillerApps.Emulation.Processors;

namespace KillerApps.Gaming.Atari
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class PcesGame : Game
	{
		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
		private LynxHandheld emulator;
		private Texture2D lcdScreen;
		private SpriteFont font;
		private ContentManager romContent;
		private InputHandler inputHandler;

		private const int magnification = 4;
		private const int graphicsWidth = Suzy.SCREEN_WIDTH * magnification;
		private const int graphicsHeight = Suzy.SCREEN_HEIGHT * magnification;

		public PcesGame()
		{
			emulator = new LynxHandheld();
			graphics = new GraphicsDeviceManager(this);
			romContent = new ResourceContentManager(Services, Roms.ResourceManager);
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			Content.RootDirectory = "Content";

			graphics.PreferredBackBufferWidth = graphicsWidth;
			graphics.PreferredBackBufferHeight = graphicsHeight;
			graphics.IsFullScreen = false;
			graphics.ApplyChanges();

			IsFixedTimeStep = true;
			TargetElapsedTime = TimeSpan.FromMilliseconds(6); // 60Hz

			lcdScreen = new Texture2D(graphics.GraphicsDevice, Suzy.SCREEN_WIDTH, Suzy.SCREEN_HEIGHT, false, SurfaceFormat.Color);

			// Lynx related
			emulator.BootRomImage = new MemoryStream(Roms.LYNXBOOT);
			LnxRomImageFileFormat romImage = new LnxRomImageFileFormat();
			BllRomImageFileFormat romImage2 = new BllRomImageFileFormat();
			emulator.Cartridge = romImage.LoadCart(new MemoryStream(Roms.Wuerfel));
			emulator.Initialize();

			//byte[] ram = emulator.Ram.GetDirectAccess();
			//Array.Copy(romImage2.Bytes, 0, ram, romImage2.Header.LoadAddress, romImage2.Header.Size);
			//ram[VectorAddresses.BOOT_VECTOR] = (byte)(romImage2.Header.LoadAddress & 0xFF);
			//ram[VectorAddresses.BOOT_VECTOR + 1] = (byte)((romImage2.Header.LoadAddress & 0xFF00) >> 8);

			emulator.Reset();

			// Preset for homebrew cartridges
			//emulator.Mikey.Timers[0].BackupValue = 0x9E;
			//emulator.Mikey.Timers[0].StaticControlBits = new StaticTimerControl(0x18);
			//emulator.Mikey.Timers[2].BackupValue = 0x68;
			//emulator.Mikey.Timers[2].StaticControlBits = new StaticTimerControl(0x1F);
			//emulator.Mikey.DISPCTL.ByteData = 0x09;

			Window.Title = "Portable Color Entertainment System";
			Window.AllowUserResizing = false;

			spriteBatch = new SpriteBatch(GraphicsDevice);
			
			Components.Add(new FrameRateCounter(this));

#if WINDOWS_PHONE
			inputHandler = new TouchHandler(this, Content, spriteBatch);
#elif WINDOWS
			inputHandler = new KeyboardHandler(this);
#elif XBOX360
			inputHandler = new GamePadHandler(this);
#endif
			Components.Add(inputHandler);
			
			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
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
			//if (controlHandler.ExitGame == true)
			//	this.Exit();

			JoystickStates joystick = inputHandler.Joystick;
			emulator.UpdateJoystickState(joystick);
			emulator.Update(50000);

			base.Update(gameTime);
		}

		/// <summary>8
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			//if (emulator.NewVideoFrameAvailable)
			//{
				lcdScreen.SetData(emulator.LcdScreenDma, 0x0, 0x3FC0);
				//emulator.NewVideoFrameAvailable = false;
			//}

			spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null);
			spriteBatch.Draw(lcdScreen, 
				new Rectangle(0, 0, graphicsWidth, graphicsHeight), 
				new Rectangle(0, 0, Suzy.SCREEN_WIDTH, Suzy.SCREEN_HEIGHT), 
				Color.White);
			spriteBatch.DrawString(font, emulator.SystemClock.CompatibleCycleCount.ToString("X16"), new Vector2(10, 50), Color.White);
			spriteBatch.DrawString(font, "Slow" + gameTime.IsRunningSlowly.ToString(), new Vector2(10, 65), Color.White);
			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
