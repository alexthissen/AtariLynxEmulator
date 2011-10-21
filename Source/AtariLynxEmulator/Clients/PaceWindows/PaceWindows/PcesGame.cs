using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using KillerApps.Emulation.Atari.Lynx;
using KillerApps.Emulation.Processors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace KillerApps.Gaming.Atari
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class PcesGame : Game
	{
		// Emulator 
		private LynxHandheld emulator;
		private ContentManager romContent;

		// Video
		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
		private Texture2D lcdScreen;
		private SpriteFont font;
		private const int magnification = 5;
		private const int graphicsWidth = Suzy.SCREEN_WIDTH * magnification;
		private const int graphicsHeight = Suzy.SCREEN_HEIGHT * magnification;

		// Input
		private InputHandler inputHandler;

		// Audio
		private byte[] soundBuffer;
		private DynamicSoundEffectInstance dynamicSound;

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
			Window.Title = "Portable Color Entertainment System";
			Window.AllowUserResizing = false;
			IsFixedTimeStep = true;
			TargetElapsedTime = TimeSpan.FromMilliseconds(6); // 60Hz

			InitializeVideo();
			InitializeEmulator();
			InitializeAudio();

#if WINDOWS_PHONE
			inputHandler = new TouchHandler(this, Content, spriteBatch);
#elif WINDOWS
			inputHandler = new KeyboardHandler(this);
#elif XBOX360
			inputHandler = new GamePadHandler(this);
#endif
			Components.Add(inputHandler);
			Components.Add(new FrameRateCounter(this));
			
			base.Initialize();
		}

		private void InitializeEmulator()
		{
			// Lynx related
			emulator.BootRomImage = new MemoryStream(Roms.LYNXBOOT);
			LnxRomImageFileFormat romImage = new LnxRomImageFileFormat();

			BllRomImageFileFormat romImage2 = new BllRomImageFileFormat();
			emulator.Cartridge = romImage.LoadCart(new MemoryStream(Roms.Collision));
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
		}

		private void InitializeVideo()
		{
			graphics.PreferredBackBufferWidth = graphicsWidth;
			graphics.PreferredBackBufferHeight = graphicsHeight;
			graphics.IsFullScreen = false;
			graphics.ApplyChanges();

			lcdScreen = new Texture2D(graphics.GraphicsDevice, Suzy.SCREEN_WIDTH, Suzy.SCREEN_HEIGHT, false, SurfaceFormat.Color);
			spriteBatch = new SpriteBatch(GraphicsDevice);
		}

		private void InitializeAudio()
		{
			dynamicSound = new DynamicSoundEffectInstance(22050, AudioChannels.Mono);
			soundBuffer = new byte[dynamicSound.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(250))];
			dynamicSound.BufferNeeded += new EventHandler<EventArgs>(DynamicSoundBufferNeeded);
			dynamicSound.Play();
		}

		private void DynamicSoundBufferNeeded(object sender, EventArgs e)
		{
			byte[] buffer = emulator.Mikey.AudioFilter.Buffer;
			dynamicSound.SubmitBuffer(buffer, 0, buffer.Length / 2);
			dynamicSound.SubmitBuffer(buffer, buffer.Length / 2, buffer.Length / 2);
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
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
			//if (inputHandler.ExitGame == true)
			//	this.Exit();

			JoystickStates joystick = inputHandler.Joystick;
			emulator.UpdateJoystickState(joystick);
			emulator.Update(50000);

			base.Update(gameTime);
		}

		protected override void OnExiting(object sender, EventArgs args)
		{
			// Stop sound before exiting
			if (dynamicSound.State != SoundState.Stopped) dynamicSound.Stop(true);
			base.OnExiting(sender, args);
		}

		/// <summary>
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
