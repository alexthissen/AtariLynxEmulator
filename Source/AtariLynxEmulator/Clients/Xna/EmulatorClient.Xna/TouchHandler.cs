using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Atari.Lynx;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace KillerApps.Gaming.Atari.Xna
{
	public class TouchHandler : InputHandler
	{
		private Texture2D thumbstick;
		private Game game;
		private ContentManager Content;
		private SpriteBatch spriteBatch;

		public TouchHandler(Game game, ContentManager content, SpriteBatch spriteBatch): base(game)
		{
			this.game = game;
			this.spriteBatch = spriteBatch;
			Content = content;
		}

		public override void Draw(GameTime gameTime)
		{
			if (VirtualThumbsticks.LeftThumbstickCenter.HasValue)
			{
				spriteBatch.Begin();
				spriteBatch.Draw(
					thumbstick,
					VirtualThumbsticks.LeftThumbstickCenter.Value - new Vector2(thumbstick.Width / 2f, thumbstick.Height / 2f),
					Color.Green);
				spriteBatch.End();
			}
			
			base.Draw(gameTime);
		}

		public override void Update(GameTime gameTime)
		{
			// Update our virtual thumbsticks
			VirtualThumbsticks.Update();

			base.Update(gameTime);
		}

		protected override void LoadContent()
		{
			thumbstick = Content.Load<Texture2D>("Thumbstick");

			base.LoadContent();
		}

		public override bool ExitGame
		{
			get
			{
				return GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed;
			}
		}
		protected override JoystickStates BuildJoystickState()
		{
			JoystickStates joystick = JoystickStates.None;

			if (VirtualThumbsticks.LeftThumbstickCenter.HasValue) 
			{
				VirtualThumbsticks.LeftThumbstick.Normalize();
				
				if (VirtualThumbsticks.LeftThumbstick.Y > 0.7f) joystick |= JoystickStates.Down;
				if (VirtualThumbsticks.LeftThumbstick.Y < -0.7f) joystick |= JoystickStates.Up;
				if (VirtualThumbsticks.LeftThumbstick.X < -0.7f) joystick |= JoystickStates.Left;
				if (VirtualThumbsticks.LeftThumbstick.X > 0.7f) joystick |= JoystickStates.Right;
			}
			
			if (VirtualThumbsticks.RightThumbstickCenter.HasValue)
				joystick |= JoystickStates.Outside;

			return joystick;
		}
	}
}
