using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace KillerApps.Gaming.Atari.MonoGame
{
	public class FrameRateCounter : DrawableGameComponent
	{
		SpriteBatch spriteBatch;
		SpriteFont spriteFont;

		int frameRate = 0;
		int frameCounter = 0;
		TimeSpan elapsedTime = TimeSpan.Zero;

		public FrameRateCounter(Game game) : base(game) { }

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			spriteFont = Game.Content.Load<SpriteFont>("FrameRateCounter");
		}

		public override void Update(GameTime gameTime)
		{
			elapsedTime += gameTime.ElapsedGameTime;

			if (elapsedTime > TimeSpan.FromSeconds(1))
			{
				elapsedTime -= TimeSpan.FromSeconds(1);
				frameRate = frameCounter;
				frameCounter = 0;
			}
		}

		public override void Draw(GameTime gameTime)
		{
			frameCounter++;
			string fps = string.Format("FPS: {0}", frameRate);

			spriteBatch.Begin();
			spriteBatch.DrawString(spriteFont, fps, new Vector2(35, 33), Color.Black);
			spriteBatch.DrawString(spriteFont, fps, new Vector2(35, 32), Color.White);
			spriteBatch.End();
		}
	}
}
