using System;
using System.IO;
using SdlDotNet.Input;
using SdlDotNet.Core;
using SdlDotNet.Graphics; 
using SdlDotNet.Audio;
using System.Runtime.InteropServices;
using KillerApps.Emulation.Atari.Lynx;
using KillerApps.Emulation.Processors;

namespace PcesSDL
{
	class MainClass
	{
		// Emulator 
		private LynxHandheld emulator;
		private const int magnification = 1;
		private const int graphicsWidth = Suzy.SCREEN_WIDTH * magnification;
		private const int graphicsHeight = Suzy.SCREEN_HEIGHT * magnification;
		private IntPtr sdlBuffer;
		private Surface surfaceVideo;
		private Surface screen;
		private JoystickStates joystick = JoystickStates.None;
		AudioStream stream;
	    AudioCallback callback;
		
		public MainClass ()
		{
			emulator = new LynxHandheld();
		}
		
        [STAThread]
        public static void Main()
        {
            MainClass app = new MainClass();
            app.Go();
        }
		
		public void Go()
        {
			Initialize();
			Events.Fps = 60;
			Events.Quit += new EventHandler<QuitEventArgs>(this.Quit);
			Events.KeyboardDown += HandleEventsKeyboardDown;
			Events.KeyboardUp += HandleEventsKeyboardUp;
			Events.Tick += HandleEventsTick;
			Events.Quit += HandleEventsQuit;
            Events.Run();
        }

		void HandleEventsQuit (object sender, QuitEventArgs e)
		{
			Events.QuitApplication();        
		}		

		void HandleEventsKeyboardUp (object sender, KeyboardEventArgs e)
		{
			if (e.Key == Key.Z) joystick -= JoystickStates.Outside;
			if (e.Key == Key.X) joystick -= JoystickStates.Inside;
			if (e.Key == Key.LeftArrow) joystick -= JoystickStates.Left;
			if (e.Key == Key.RightArrow) joystick -= JoystickStates.Right;
			if (e.Key == Key.UpArrow) joystick -= JoystickStates.Up;
			if (e.Key == Key.DownArrow) joystick -= JoystickStates.Down;
			if (e.Key == Key.F1) joystick -= JoystickStates.Option1;
			if (e.Key == Key.F2) joystick -= JoystickStates.Option2;
		}

		void HandleEventsKeyboardDown (object sender, KeyboardEventArgs e)
		{
			if (e.Key == Key.Z) joystick |= JoystickStates.Outside;
			if (e.Key == Key.X) joystick |= JoystickStates.Inside;
			if (e.Key == Key.LeftArrow) joystick |= JoystickStates.Left;
			if (e.Key == Key.RightArrow) joystick |= JoystickStates.Right;
			if (e.Key == Key.UpArrow) joystick |= JoystickStates.Up;
			if (e.Key == Key.DownArrow) joystick |= JoystickStates.Down;
			if (e.Key == Key.F1) joystick |= JoystickStates.Option1;
			if (e.Key == Key.F2) joystick |= JoystickStates.Option2;
			
			if ((e.Key == Key.F4 && e.Mod == ModifierKeys.AltKeys) || e.Key == Key.Escape) Events.QuitApplication();
		}
		
		void HandleEventsTick (object sender, TickEventArgs e)
		{
			emulator.UpdateJoystickState(joystick);
			emulator.Update(200000);
			Marshal.Copy(emulator.LcdScreenDma, 0, sdlBuffer, Suzy.SCREEN_WIDTH * Suzy.SCREEN_HEIGHT);
			surfaceVideo.Blit(screen, new System.Drawing.Rectangle(0, 0, graphicsWidth, graphicsHeight));
			Video.Update();
		}
		
		private void Initialize()
		{
			InitializeEmulator();
			InitializeVideo();
			InitializeAudio();
		}

        private void Quit(object sender, QuitEventArgs e)
        {
			Events.CloseAudio();
            Events.QuitApplication();
        }
		
		private void InitializeAudio()
		{
			try
			{
				callback = new AudioCallback(SoundBufferNeeded);
				stream = new AudioStream(22050, AudioFormat.Unsigned16Little, SoundChannel.Mono, 0, callback, null);
		    	stream.Paused = false;
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
		
		private void InitializeEmulator()
		{
			// Lynx related
			emulator.BootRomImage = File.Open("lynxboot.img", FileMode.Open);
			LnxRomImageFileFormat romImage = new LnxRomImageFileFormat();
			emulator.InsertCartridge(romImage.LoadCart(File.Open("game.lnx", FileMode.Open)));
			emulator.Initialize();

			emulator.Reset();
		}

		private void InitializeVideo()
		{
			surfaceVideo = Video.SetVideoMode(graphicsWidth, graphicsHeight, 32, false, false, true, true);
			screen = new Surface(Suzy.SCREEN_WIDTH, Suzy.SCREEN_HEIGHT);
			surfaceVideo.Convert(screen);
			//scaled = screen.CreateStretchedSurface(new System.Drawing.Size(graphicsWidth, graphicsHeight));
			//scaled = screen.CreateScaledSurface(4.0);
			sdlBuffer = screen.Pixels;
			
			Video.WindowCaption = "Epic Lynx Emulator";			
		}
		
		void SoundBufferNeeded(IntPtr userData, IntPtr stream, int length)
        {	
            Marshal.Copy(emulator.Mikey.AudioFilter.Buffer, 0, stream, length);
            length = 0;
        }
	}
}