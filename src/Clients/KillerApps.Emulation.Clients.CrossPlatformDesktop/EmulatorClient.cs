using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using KillerApps.Emulation.AtariLynx;
using KillerApps.Gaming.MonoGame;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace KillerApps.Emulation.Clients.CrossPlatformDesktop
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class EmulatorClient : Game
    {
        // Emulator 
        private LynxHandheld emulator;
        private ContentManager romContent;

        // Video
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Texture2D lcdScreen;
        private int graphicsWidth;
        private int graphicsHeight;

        public const int DEFAULT_MAGNIFICATION = 8;
        private const int DEFAULT_GRAPHICS_WIDTH = Suzy.SCREEN_WIDTH * DEFAULT_MAGNIFICATION;
        private const int DEFAULT_GRAPHICS_HEIGHT = Suzy.SCREEN_HEIGHT * DEFAULT_MAGNIFICATION;
        private readonly EmulatorClientOptions clientOptions;

        // Input
        private InputHandler inputHandler;

        // Audio
        private byte[] soundBuffer;
        private DynamicSoundEffectInstance dynamicSound;

        // Network
        private IComLynxTransport transport = new SerialPortComLynxTransport();

        public EmulatorClient(EmulatorClientOptions options = null) : base()
        {
            emulator = new LynxHandheld();
            graphics = new GraphicsDeviceManager(this);
            romContent = new ResourceContentManager(Services, Roms.ResourceManager);

            clientOptions = options ?? EmulatorClientOptions.Default;
            graphicsHeight = clientOptions.Magnification * Suzy.SCREEN_HEIGHT;
            graphicsWidth = clientOptions.Magnification * Suzy.SCREEN_WIDTH;
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
            Window.Title = "Atari Lynx Emulator";
            Window.AllowUserResizing = false;
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromMilliseconds(6); // 60Hz

            InitializeVideo(clientOptions.FullScreen);
            InitializeEmulator(clientOptions.BootRom, clientOptions.GameRom);
            InitializeAudio();

            inputHandler = clientOptions.Controller switch
            {
                ControllerType.Gamepad => new GamePadHandler(this),
                ControllerType.Keyboard => new KeyboardHandler(this),
                _ => throw new NotImplementedException()
            };
            Components.Add(inputHandler);

            base.Initialize();
        }

        private ICartridge LoadCartridge(FileInfo gameRomFileInfo)
        {
            ICartridge cartridge = null;
            LnxRomImageFileFormat gameRomImage = new LnxRomImageFileFormat();

            Stream gameRomStream = gameRomFileInfo?.OpenRead();
            if (gameRomStream is null) gameRomStream = new MemoryStream(Roms.junglejack);

            try
            {
                cartridge = gameRomImage.LoadCart(gameRomStream);
            }
            catch (Exception)
            {
                cartridge = new FaultyCart();
            }
            return cartridge;
        }

        private void InitializeEmulator(FileInfo bootRomFileInfo, FileInfo gameRomFileInfo)
        {
            // Lynx related
            Stream bootRomImage = bootRomFileInfo?.OpenRead();
            emulator.BootRomImage = bootRomImage ?? (Stream)(new MemoryStream(Roms.LYNXBOOT));
            emulator.InsertCartridge(LoadCartridge(gameRomFileInfo));
            emulator.Initialize();
            
            emulator.Reset();
        }

        private void InitializeVideo(bool fullScreen)
        {
            // Set video options
            graphics.PreferredBackBufferWidth = graphicsWidth;
            graphics.PreferredBackBufferHeight = graphicsHeight;
            graphics.IsFullScreen = fullScreen;
            graphics.ApplyChanges();

            lcdScreen = new Texture2D(graphics.GraphicsDevice, Suzy.SCREEN_WIDTH, Suzy.SCREEN_HEIGHT, false, SurfaceFormat.Color);
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        private void InitializeAudio()
        {
            dynamicSound = new DynamicSoundEffectInstance(22050, AudioChannels.Mono);
            soundBuffer = new byte[dynamicSound.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(250))];
            //dynamicSound.BufferNeeded += new EventHandler<EventArgs>(DynamicSoundBufferNeeded);
            emulator.Mikey.AudioFilter.BufferReady += new EventHandler<BufferEventArgs>(OnAudioFilterBufferReady);
            dynamicSound.Play();
        }

        void OnAudioFilterBufferReady(object sender, BufferEventArgs e)
        {
            byte[] buffer = e.Buffer;
            dynamicSound.SubmitBuffer(buffer, 0, buffer.Length / 2);
            dynamicSound.SubmitBuffer(buffer, buffer.Length / 2, buffer.Length / 2);
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
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
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
            emulator.Update(86667); // 4 MHz worth of cycles divided by 60 seconds

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            lcdScreen.SetData(emulator.LcdScreenDma, 0x0, 0x3FC0);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null);
            spriteBatch.Draw(lcdScreen,
                new Rectangle(0, 0, graphicsWidth, graphicsHeight),
                new Rectangle(0, 0, Suzy.SCREEN_WIDTH, Suzy.SCREEN_HEIGHT),
                Color.White);
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
