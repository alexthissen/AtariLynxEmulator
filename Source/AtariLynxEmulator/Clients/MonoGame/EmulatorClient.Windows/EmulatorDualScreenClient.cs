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
using System.Net;
using EmulatorClient.Windows.Tools;

namespace EmulatorClient.Windows
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class EmulatorClient : Game
	{
		// Emulator 
		private LynxHandheld emulator1, emulator2;
		private ContentManager romContent;

		// Video
		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
		private Texture2D lcdScreen1, lcdScreen2;
		private int graphicsHeight;
		private int graphicsWidth;
		private Options options;

		// Input
		private InputHandler inputHandler;

		// Audio
		//private byte[] soundBuffer;
		//private DynamicSoundEffectInstance dynamicSound;

		// Network
		private int separatorWidth = 2;
		//private GamerServicesComponent gamerServices = null;

		public EmulatorClient(Options options)
			: base()
		{
			this.options = options;
			emulator1 = new LynxHandheld();
			emulator2 = new LynxHandheld();
			graphics = new GraphicsDeviceManager(this);
			romContent = new ResourceContentManager(Services, Roms.ResourceManager);

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
			InitializeEmulator(emulator1);
			InitializeEmulator(emulator2);

			emulator1.Mikey.ComLynx.Transmitter.DataTransmitting += emulator2.Mikey.ComLynx.Receiver.HandleDataTransmitting;
			emulator2.Mikey.ComLynx.Transmitter.DataTransmitting += emulator1.Mikey.ComLynx.Receiver.HandleDataTransmitting;
			emulator1.Mikey.ComLynx.Name = emulator1.Mikey.ComLynx.Transmitter.Name = emulator1.Mikey.ComLynx.Receiver.Name = "One";
			emulator2.Mikey.ComLynx.Name = emulator2.Mikey.ComLynx.Transmitter.Name = emulator2.Mikey.ComLynx.Receiver.Name = "Two";

			//InitializeAudio();

			emulator1.Update(1000000);

			//inputHandler = new GamePadHandler(this);
			inputHandler = new KeyboardHandler(this);
			Components.Add(inputHandler);
			//Components.Add(new FrameRateCounter(this));

			base.Initialize();
		}

		private ICartridge LoadCartridge()
		{
			ICartridge cartridge = null;

			try
			{
#if WINDOWS || LINUX
				string cartridgeFileName = options.ROMFilename;
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

		private Stream GetBiosStream()
		{
			try
			{
				return new FileStream(options.BiosFilename, FileMode.Open, FileAccess.Read);
			}
			catch (Exception)
			{
				return new MemoryStream(Roms.lynxtest);
			}
		}

		private void InitializeEmulator(LynxHandheld emulator)
		{
			// Lynx related
			emulator.BootRomImage = GetBiosStream();
			emulator.InsertCartridge(LoadCartridge());
			emulator.Initialize();

			//emulator.InsertComLynxCable(new SerialPortComLynxTransport(options.ComPort));

			//ICartridge cartridge = LoadCartridge();
			//emulator.InsertCartridge(cartridge);
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
			graphicsWidth = Suzy.SCREEN_WIDTH * options.Magnification;
			graphicsHeight = Suzy.SCREEN_HEIGHT * options.Magnification;

			graphics.PreferredBackBufferWidth = 2 * graphicsWidth + separatorWidth;
			graphics.PreferredBackBufferHeight = graphicsHeight;
			graphics.IsFullScreen = false;
			graphics.ApplyChanges();

			lcdScreen1 = new Texture2D(graphics.GraphicsDevice, Suzy.SCREEN_WIDTH, Suzy.SCREEN_HEIGHT, false, SurfaceFormat.Color);
			lcdScreen2 = new Texture2D(graphics.GraphicsDevice, Suzy.SCREEN_WIDTH, Suzy.SCREEN_HEIGHT, false, SurfaceFormat.Color);
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
			emulator1.UpdateJoystickState(joystick);
			emulator2.UpdateJoystickState(joystick);
			for (int i = 0; i < 7500; i++)
			{
				emulator1.Update(10); // 4 MHz worth of cycles divided by 60 seconds
				emulator2.Update(10); // 4 MHz worth of cycles divided by 60 seconds				
			}
			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			lcdScreen1.SetData(emulator1.LcdScreenDma, 0x0, 0x3FC0);
			lcdScreen2.SetData(emulator2.LcdScreenDma, 0x0, 0x3FC0);

			//  Stream stream = new ;
			//lcdScreen.SaveAsJpeg(stream, Suzy.SCREEN_WIDTH, Suzy.SCREEN_HEIGHT);

			spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null);
			//spriteBatch.Draw(border, new Rectangle(0,0, graphicsWidth *2, graphicsHeight*2), Color.White);
			spriteBatch.Draw(lcdScreen1,
				new Rectangle(0, 0, graphicsWidth, graphicsHeight),
				new Rectangle(0, 0, Suzy.SCREEN_WIDTH, Suzy.SCREEN_HEIGHT),
				Color.White);
			spriteBatch.Draw(lcdScreen2,
				new Rectangle(graphicsWidth + separatorWidth, 0, graphicsWidth, graphicsHeight),
				new Rectangle(0, 0, Suzy.SCREEN_WIDTH, Suzy.SCREEN_HEIGHT),
				Color.White);
			//spriteBatch.DrawString(font, emulator.SystemClock.CompatibleCycleCount.ToString("X16"), new Vector2(10, 50), Color.White);
			spriteBatch.End();

			base.Draw(gameTime);
		}

		protected override void OnExiting(object sender, EventArgs args)
		{
			// Stop sound before exiting
			//if (dynamicSound.State != SoundState.Stopped) dynamicSound.Stop(true);
			base.OnExiting(sender, args);
		}
	}
}
