using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OctarineCodex.Input;

namespace OctarineCodex;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;

    // Input
    private readonly IInputService _inputService;

    // Primitive 1x1 texture for drawing rectangles
    private Texture2D _pixel = null!;

    // Player state
    private Vector2 _playerPos;
    private const int PlayerSize = 32;
    private const float PlayerSpeed = 220f; // pixels per second

    // Grid state
    private const int TileSize = 32;



    public Game1(IInputService inputService)
    {
        _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Improve smoothing of input by decoupling updates from VSync and fixed timestep
        IsFixedTimeStep = false;
        _graphics.SynchronizeWithVerticalRetrace = false;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        base.Initialize();
        // Start player roughly centered
        var vp = GraphicsDevice.Viewport;
        _playerPos = new Vector2((vp.Width - PlayerSize) / 2f, (vp.Height - PlayerSize) / 2f);
    }

    protected override void LoadContent()
    {
        
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Create a 1x1 white texture to draw colored rectangles
        _pixel = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
        _pixel.SetData(new[] { Color.White });
    }

    protected override void Update(GameTime gameTime)
    {
        _inputService.Update(gameTime);
        if (_inputService.IsExitPressed())
        {
            Exit();
        }

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var dir = _inputService.GetMovementDirection();

        var delta = Movement.ComputeDelta(dir, PlayerSpeed, dt);
        _playerPos += delta;

        // Clamp within viewport
        var vp = GraphicsDevice.Viewport;
        _playerPos.X = MathHelper.Clamp(_playerPos.X, 0, vp.Width - PlayerSize);
        _playerPos.Y = MathHelper.Clamp(_playerPos.Y, 0, vp.Height - PlayerSize);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Draw pink/black checker background
        var vp = GraphicsDevice.Viewport;
        for (int y = 0; y < vp.Height; y += TileSize)
        {
            for (int x = 0; x < vp.Width; x += TileSize)
            {
                bool isPink = (((x / TileSize) + (y / TileSize)) % 2) == 0;
                var color = isPink ? Color.HotPink : Color.Black;
                _spriteBatch.Draw(_pixel, new Rectangle(x, y, TileSize, TileSize), color);
            }
        }

        // Draw the player as a red square
        _spriteBatch.Draw(_pixel, new Rectangle((int)_playerPos.X, (int)_playerPos.Y, PlayerSize, PlayerSize), Color.Red);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}