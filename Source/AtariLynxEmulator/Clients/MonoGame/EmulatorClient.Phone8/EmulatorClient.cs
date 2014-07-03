#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using KillerApps.Emulation.Atari.Lynx;
using KillerApps.Gaming.Atari.MonoGame;
using System.IO;
using KillerApps.Gaming;
#endregion

namespace EmulatorClient.Phone8
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class EmulatorClient : Game
	{
		// Emulator 
		private LynxHandheld emulator;

		// Video
		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
		private Texture2D lcdScreen;
		private const int magnification = 3;
		private const int graphicsWidth = Suzy.SCREEN_WIDTH * magnification;
		private const int graphicsHeight = Suzy.SCREEN_HEIGHT * magnification;

		// Input
		private InputHandler inputHandler;

		// Audio
		//private byte[] soundBuffer;
		//private DynamicSoundEffectInstance dynamicSound;

		// Network
		//private IComLynxTransport transport = null;
		//private GamerServicesComponent gamerServices = null;

		public string[] CommandLine { get; set; }

		public EmulatorClient()
			: base()
		{
			emulator = new LynxHandheld();
			graphics = new GraphicsDeviceManager(this);
			//romContent = new ResourceContentManager(Services, Roms.ResourceManager);

			//gamerServices = new GamerServicesComponent(this);
			//Components.Add(gamerServices);
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
			//InitializeAudio();

			inputHandler = new TouchHandler(this, Content, spriteBatch); 
			//Components.Add(inputHandler);
			//Components.Add(new FrameRateCounter(this));

			base.Initialize();
		}

		private ICartridge LoadCartridge()
		{
			ICartridge cartridge = null;

			try
			{
#if WINDOWS || LINUX
				string cartridgeFileName = CommandLine.Length > 0 ? CommandLine[0] : String.Empty;
				using (FileStream stream = File.OpenRead(cartridgeFileName))
				{
					LnxRomImageFileFormat romImage = new LnxRomImageFileFormat();
					cartridge = romImage.LoadCart(stream);
				}
#endif
			}
			catch (Exception)
			{
				cartridge = new FaultyCart();
			}
			return cartridge;
		}

		private void InitializeEmulator()
		{
			// Lynx related
			emulator.BootRomImage = new MemoryStream(Roms.lynxtest);
			LnxRomImageFileFormat romImage = new LnxRomImageFileFormat();
			emulator.InsertCartridge(romImage.LoadCart(new MemoryStream(Roms.Collision)));
			//ICartridge cartridge = LoadCartridge();
			//emulator.InsertCartridge(cartridge);
			emulator.Initialize();
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

		//private void InitializeAudio()
		//{
		//	dynamicSound = new DynamicSoundEffectInstance(22050, AudioChannels.Mono);
		//	soundBuffer = new byte[dynamicSound.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(250))];
		//	//dynamicSound.BufferNeeded += new EventHandler<EventArgs>(DynamicSoundBufferNeeded);
		//	emulator.Mikey.AudioFilter.BufferReady += new EventHandler<BufferEventArgs>(OnAudioFilterBufferReady);
		//	dynamicSound.Play();
		//}

		//void OnAudioFilterBufferReady(object sender, BufferEventArgs e)
		//{
		//	byte[] buffer = e.Buffer;
		//	dynamicSound.SubmitBuffer(buffer, 0, buffer.Length / 2);
		//	dynamicSound.SubmitBuffer(buffer, buffer.Length / 2, buffer.Length / 2);
		//}

		//private void DynamicSoundBufferNeeded(object sender, EventArgs e)
		//{
		//	byte[] buffer = emulator.Mikey.AudioFilter.Buffer;
		//	dynamicSound.SubmitBuffer(buffer, 0, buffer.Length / 2);
		//	dynamicSound.SubmitBuffer(buffer, buffer.Length / 2, buffer.Length / 2);
		//}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			//font = Content.Load<SpriteFont>("DefaultFont");
			//border = Content.Load<Texture2D>("BackgroundLynxBorder");
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
			if (inputHandler.ExitGame == true)
				this.Exit();

			inputHandler.Update(gameTime);

			JoystickStates joystick = inputHandler.Joystick;
			emulator.UpdateJoystickState(joystick);
			emulator.Update(66667); // 4 MHz worth of cycles divided by 60 seconds

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			lcdScreen.SetData(emulator.LcdScreenDma, 0x0, 0x3FC0);
			GraphicsDevice.Clear(Color.Black);
			//  Stream stream = new ;
			//lcdScreen.SaveAsJpeg(stream, Suzy.SCREEN_WIDTH, Suzy.SCREEN_HEIGHT);
			spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null);
			//spriteBatch.Draw(border, new Rectangle(0,0, graphicsWidth *2, graphicsHeight*2), Color.White);
			spriteBatch.Draw(lcdScreen,
				new Rectangle(0, 0, graphicsWidth, graphicsHeight),
				new Rectangle(0, 0, Suzy.SCREEN_WIDTH, Suzy.SCREEN_HEIGHT),
				Color.White);
			//spriteBatch.DrawString(font, emulator.SystemClock.CompatibleCycleCount.ToString("X16"), new Vector2(10, 50), Color.White);
			spriteBatch.End();

			//base.Draw(gameTime);
		}

		protected override void OnExiting(object sender, EventArgs args)
		{
			// Stop sound before exiting
			//if (dynamicSound.State != SoundState.Stopped) dynamicSound.Stop(true);
			base.OnExiting(sender, args);
		}
	}
}
