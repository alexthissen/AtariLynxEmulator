using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using KillerApps.Emulation.AtariLynx;

namespace KillerApps.Emulation.Clients.ARMOptimized
{
    /// <summary>
    /// Base class for input handling
    /// </summary>
    public abstract class InputHandler : GameComponent
    {
        protected readonly EmulatorClient client;

        public InputHandler(EmulatorClient client) : base(client)
        {
            this.client = client;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected void SendJoystickState(JoystickStates state)
        {
            client.HandleJoystickChange(state);
        }
    }

    /// <summary>
    /// Keyboard input handler
    /// </summary>
    public class KeyboardHandler : InputHandler
    {
        private KeyboardState previousState;

        public KeyboardHandler(EmulatorClient client) : base(client)
        {
            previousState = Keyboard.GetState();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            KeyboardState currentState = Keyboard.GetState();

            JoystickStates state = JoystickStates.None;
            
            // Map keyboard keys to emulator buttons
            if (currentState.IsKeyDown(Keys.Up)) state |= JoystickStates.Up;
            if (currentState.IsKeyDown(Keys.Down)) state |= JoystickStates.Down;
            if (currentState.IsKeyDown(Keys.Left)) state |= JoystickStates.Left;
            if (currentState.IsKeyDown(Keys.Right)) state |= JoystickStates.Right;
            if (currentState.IsKeyDown(Keys.A)) state |= JoystickStates.A;
            if (currentState.IsKeyDown(Keys.S)) state |= JoystickStates.B;
            if (currentState.IsKeyDown(Keys.Q)) state |= JoystickStates.Option1;
            if (currentState.IsKeyDown(Keys.W)) state |= JoystickStates.Option2;
            if (currentState.IsKeyDown(Keys.Enter)) state |= JoystickStates.Pause;

            // Send state to emulator
            SendJoystickState(state);
            
            // Handle escape key to exit
            if (currentState.IsKeyDown(Keys.Escape) && !previousState.IsKeyDown(Keys.Escape))
            {
                client.Exit();
            }

            previousState = currentState;
        }
    }

    /// <summary>
    /// Game pad input handler
    /// </summary>
    public class GamePadHandler : InputHandler
    {
        private GamePadState previousState;
        private const float THUMBSTICK_THRESHOLD = 0.5f;

        public GamePadHandler(EmulatorClient client) : base(client)
        {
            previousState = GamePad.GetState(PlayerIndex.One);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            GamePadState currentState = GamePad.GetState(PlayerIndex.One);

            if (!currentState.IsConnected)
            {
                return;
            }

            JoystickStates state = JoystickStates.None;
            
            // D-pad mapping
            if (currentState.DPad.Up == ButtonState.Pressed) state |= JoystickStates.Up;
            if (currentState.DPad.Down == ButtonState.Pressed) state |= JoystickStates.Down;
            if (currentState.DPad.Left == ButtonState.Pressed) state |= JoystickStates.Left;
            if (currentState.DPad.Right == ButtonState.Pressed) state |= JoystickStates.Right;
            
            // Left thumbstick mapping
            if (currentState.ThumbSticks.Left.Y > THUMBSTICK_THRESHOLD) state |= JoystickStates.Up;
            if (currentState.ThumbSticks.Left.Y < -THUMBSTICK_THRESHOLD) state |= JoystickStates.Down;
            if (currentState.ThumbSticks.Left.X < -THUMBSTICK_THRESHOLD) state |= JoystickStates.Left;
            if (currentState.ThumbSticks.Left.X > THUMBSTICK_THRESHOLD) state |= JoystickStates.Right;
            
            // Button mapping
            if (currentState.Buttons.A == ButtonState.Pressed) state |= JoystickStates.A;
            if (currentState.Buttons.B == ButtonState.Pressed) state |= JoystickStates.B;
            if (currentState.Buttons.X == ButtonState.Pressed) state |= JoystickStates.Option1;
            if (currentState.Buttons.Y == ButtonState.Pressed) state |= JoystickStates.Option2;
            if (currentState.Buttons.Start == ButtonState.Pressed) state |= JoystickStates.Pause;

            // Send state to emulator
            SendJoystickState(state);
            
            // Handle Back button to exit
            if (currentState.Buttons.Back == ButtonState.Pressed && previousState.Buttons.Back == ButtonState.Released)
            {
                client.Exit();
            }

            previousState = currentState;
        }
    }
}
