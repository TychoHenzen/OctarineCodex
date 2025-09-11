using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OctarineCodex.Input;

/// <summary>
/// Composite input service that combines input from keyboard and controller sources.
/// Provides unified game input actions by aggregating multiple input providers.
/// </summary>
public sealed class CompositeInputService : IInputService
{
    private readonly IKeyboardInputProvider _keyboard;
    private readonly IControllerInputProvider _controller;

    public CompositeInputService(IKeyboardInputProvider keyboard, IControllerInputProvider controller)
    {
        _keyboard = keyboard ?? throw new ArgumentNullException(nameof(keyboard));
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
    }

    public void Update(GameTime gameTime)
    {
        _keyboard.Update(gameTime);
        _controller.Update(gameTime);
    }

    public Vector2 GetMovementDirection()
    {
        Vector2 direction = Vector2.Zero;

        // Keyboard input (WASD)
        if (_keyboard.IsKeyDown(Keys.W))
        {
            direction.Y -= 1f;
        }

        if (_keyboard.IsKeyDown(Keys.S))
        {
            direction.Y += 1f;
        }

        if (_keyboard.IsKeyDown(Keys.A))
        {
            direction.X -= 1f;
        }

        if (_keyboard.IsKeyDown(Keys.D))
        {
            direction.X += 1f;
        }

        // Controller input (left thumbstick)
        if (_controller.IsConnected)
        {
            var thumbstick = _controller.LeftThumbstick;
            // Apply deadzone (MonoGame already applies default deadzone)
            direction += new Vector2(thumbstick.X, -thumbstick.Y); // Invert Y for screen coordinates
        }

        // Normalize if we have any input
        if (direction != Vector2.Zero)
        {
            direction.Normalize();
        }

        return direction;
    }

    public bool IsExitPressed()
    {
        // Keyboard: Escape key
        bool keyboardExit = _keyboard.IsKeyDown(Keys.Escape);
        
        // Controller: Back button
        bool controllerExit = _controller.IsConnected && _controller.IsButtonDown(Buttons.Back);

        return keyboardExit || controllerExit;
    }

    public bool IsPrimaryActionPressed()
    {
        // Keyboard: Space or Enter
        bool keyboardAction = _keyboard.IsKeyPressed(Keys.Space) || _keyboard.IsKeyPressed(Keys.Enter);
        
        // Controller: A button
        bool controllerAction = _controller.IsConnected && _controller.IsButtonPressed(Buttons.A);

        return keyboardAction || controllerAction;
    }

    public bool IsSecondaryActionPressed()
    {
        // Keyboard: Left Shift
        bool keyboardAction = _keyboard.IsKeyPressed(Keys.LeftShift);
        
        // Controller: B button
        bool controllerAction = _controller.IsConnected && _controller.IsButtonPressed(Buttons.B);

        return keyboardAction || controllerAction;
    }
}