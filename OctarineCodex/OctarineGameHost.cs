using System;
using System.IO;
using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Input;
using OctarineCodex.Logging;
using OctarineCodex.Maps;

namespace OctarineCodex;

public class OctarineGameHost : Game
{
    private const int PlayerSize = 32;
    private const float PlayerSpeed = 220f; // pixels per second

    // Grid state
    private const int TileSize = 32;
    private readonly GraphicsDeviceManager _graphics;

    // Input
    private readonly IInputService _inputService;

    // Logging
    private readonly ILoggingService _logger;
    private readonly ISimpleLevelRenderer _mapRenderer;

    // Simple map services
    private readonly ISimpleMapService _mapService;

    // Current loaded level
    private LDtkLevel? _currentLevel;


    // Primitive 1x1 texture for drawing rectangles
    private Texture2D _pixel = null!;

    // Player state
    private Vector2 _playerPos;

    private SpriteBatch _spriteBatch = null!;


    public OctarineGameHost(ILoggingService logger, IInputService inputService, ISimpleMapService mapService,
        ISimpleLevelRenderer mapRenderer)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
        _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
        _mapRenderer = mapRenderer ?? throw new ArgumentNullException(nameof(mapRenderer));
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

    protected override async void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Create a 1x1 white texture to draw colored rectangles
        _pixel = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
        _pixel.SetData(new[] { Color.White });

        // Initialize simple renderer
        _mapRenderer.Initialize(GraphicsDevice);

        // Load Room1.ldtk using simplified system
        var ldtkPath = Path.Combine(Content.RootDirectory, "Room1.ldtk");
        _logger.Debug($"Loading Room1.ldtk from: {ldtkPath}");
        _currentLevel = await _mapService.LoadLevelAsync(ldtkPath);

        if (_currentLevel != null)
        {
            await _mapRenderer.LoadTilesetsAsync(_currentLevel, Content);
            _logger.Debug(
                $"Level '{_currentLevel.Identifier}' loaded successfully - Size: {_currentLevel.PxWid}x{_currentLevel.PxHei}");
        }
        else
        {
            _logger.Error("Failed to load Room1.ldtk");
        }
    }

    protected override void Update(GameTime gameTime)
    {
        _inputService.Update(gameTime);
        if (_inputService.IsExitPressed()) Exit();

        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var dir = _inputService.GetMovementDirection();

        var delta = Movement.ComputeDelta(dir, PlayerSpeed, dt);
        var newPlayerPos = _playerPos + delta;

        // Simple movement - clamp to viewport bounds
        var vp = GraphicsDevice.Viewport;
        _playerPos.X = MathHelper.Clamp(newPlayerPos.X, 0, vp.Width - PlayerSize);
        _playerPos.Y = MathHelper.Clamp(newPlayerPos.Y, 0, vp.Height - PlayerSize);

        base.Update(gameTime);
    }
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // Level rendering with 4x scale
        var scaleMatrix = Matrix.CreateScale(4.0f);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: scaleMatrix);

        if (_currentLevel != null)
        {
            var vp = GraphicsDevice.Viewport;
            var screenCenter = new Vector2(vp.Width / 2f, vp.Height / 2f) / 4.0f;
            _mapRenderer.RenderLevelCentered(_currentLevel, _spriteBatch, screenCenter);
        }

        _spriteBatch.End();

        // Player rendering at normal scale
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_pixel, new Rectangle((int)_playerPos.X, (int)_playerPos.Y, PlayerSize, PlayerSize), Color.Red);
        _spriteBatch.End();
    }
}