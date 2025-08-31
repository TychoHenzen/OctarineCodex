using System;
using System.IO;
using System.Linq;
using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Input;
using OctarineCodex.Logging;
using OctarineCodex.Maps;

namespace OctarineCodex;

public class OctarineGameHost : Game
{
    private const float PlayerSpeed = 220f; // pixels per second
    private const float WorldRenderScale = 4.0f; // Scale factor for world/level rendering
    
    // Fixed resolution constants
    private const int FixedWidth = 640;
    private const int FixedHeight = 480;

    // Grid state
    private readonly GraphicsDeviceManager _graphics;

    // Input
    private readonly IInputService _inputService;

    // Logging
    private readonly ILoggingService _logger;
    private readonly ISimpleLevelRenderer _mapRenderer;

    // Simple map services
    private readonly ISimpleMapService _mapService;
    private Camera2D _camera = null!;

    // Current loaded level
    private LDtkLevel? _currentLevel;

    // Rendering system
    private RenderTarget2D _renderTarget = null!;
    private Texture2D _pixel = null!;
    private Player _player = null!;
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

        // Initialize camera with FIXED viewport size instead of dynamic window size
        var worldViewportSize = new Vector2(FixedWidth / WorldRenderScale, FixedHeight / WorldRenderScale);
        _camera = new Camera2D(worldViewportSize);

        _logger.Debug($"Window size: {GraphicsDevice.Viewport.Width}x{GraphicsDevice.Viewport.Height}");
        _logger.Debug($"Fixed resolution: {FixedWidth}x{FixedHeight}");
        _logger.Debug($"World viewport (camera sees): {worldViewportSize}");
        _logger.Debug($"Player size: {Player.Size}x{Player.Size}");
    }

    /// <summary>
    /// Calculates the destination rectangle for scaling the fixed resolution render target to the window
    /// while maintaining aspect ratio with letterboxing/pillarboxing as needed.
    /// </summary>
    /// <returns>Destination rectangle for drawing the render target.</returns>
    private Rectangle CalculateDestinationRectangle()
    {
        var windowWidth = GraphicsDevice.Viewport.Width;
        var windowHeight = GraphicsDevice.Viewport.Height;
        
        // Calculate scaling factor (use minimum to maintain aspect ratio)
        var scaleX = (float)windowWidth / FixedWidth;
        var scaleY = (float)windowHeight / FixedHeight;
        var scale = Math.Min(scaleX, scaleY);
        
        // Calculate scaled size
        var scaledWidth = (int)(FixedWidth * scale);
        var scaledHeight = (int)(FixedHeight * scale);
        
        // Center the scaled image
        var x = (windowWidth - scaledWidth) / 2;
        var y = (windowHeight - scaledHeight) / 2;
        
        return new Rectangle(x, y, scaledWidth, scaledHeight);
    }

    /// <summary>
    ///     Finds the player spawn point from LDTK entity layers.
    /// </summary>
    /// <param name="level">The LDTK level to search.</param>
    /// <returns>Player spawn position, or level center if no spawn point found.</returns>
    private Vector2 FindPlayerSpawnPoint(LDtkLevel level)
    {
        // Search for Player entity in all layers
        foreach (var layer in level.LayerInstances)
            if (layer._Type == LayerType.Entities)
            {
                var playerEntity = layer.EntityInstances
                    .FirstOrDefault(e => e._Identifier.Equals("Player", StringComparison.OrdinalIgnoreCase));

                if (playerEntity != null)
                {
                    var spawnPos = new Vector2(playerEntity.Px.X, playerEntity.Px.Y);

                    // Validate spawn position is within level bounds
                    spawnPos.X = MathHelper.Clamp(spawnPos.X, 0, level.PxWid - Player.Size);
                    spawnPos.Y = MathHelper.Clamp(spawnPos.Y, 0, level.PxHei - Player.Size);

                    _logger.Debug($"Found player spawn point at ({spawnPos.X}, {spawnPos.Y})");
                    return spawnPos;
                }
            }

        // Fallback to level center with bounds validation
        var centerPos = new Vector2(
            MathHelper.Clamp(level.PxWid / 2f - Player.Size / 2f, 0, level.PxWid - Player.Size),
            MathHelper.Clamp(level.PxHei / 2f - Player.Size / 2f, 0, level.PxHei - Player.Size)
        );
        _logger.Debug($"No player spawn point found, using level center: ({centerPos.X}, {centerPos.Y})");
        return centerPos;
    }

    protected override async void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Create render target for fixed resolution rendering
        _renderTarget = new RenderTarget2D(GraphicsDevice, FixedWidth, FixedHeight);
        
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

            // Create player at spawn position
            var spawnPosition = FindPlayerSpawnPoint(_currentLevel);
            _player = new Player(spawnPosition);

            // Initialize camera to follow player
            var roomSize = new Vector2(_currentLevel.PxWid, _currentLevel.PxHei);
            _camera.FollowPlayer(_player, Vector2.Zero, roomSize);

            _logger.Debug($"Level loaded - Size: {_currentLevel.PxWid}x{_currentLevel.PxHei}");
            _logger.Debug($"Player spawn position: {_player.Position}");
            _logger.Debug($"Camera position: {_camera.Position}");
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

        if (_player != null && _currentLevel != null)
        {
            // Calculate movement delta
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var dir = _inputService.GetMovementDirection();
            var delta = Movement.ComputeDelta(dir, PlayerSpeed, dt);

            // Update player position (includes bounds checking)
            var levelSize = new Vector2(_currentLevel.PxWid, _currentLevel.PxHei);
            _player.Update(delta, levelSize);

            // Update camera to follow player
            var roomSize = new Vector2(_currentLevel.PxWid, _currentLevel.PxHei);
            _camera.FollowPlayer(_player, Vector2.Zero, roomSize);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (_currentLevel != null && _player != null)
        {
            // STEP 1: Render game to fixed resolution render target
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.Black);

            // FIXED: Apply camera transform first, then scale (fixes 1/4 speed issue)
            var worldMatrix = _camera.GetTransformMatrix() * Matrix.CreateScale(WorldRenderScale);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: worldMatrix);

            // Render level in world coordinates
            _mapRenderer.RenderLevel(_currentLevel, _spriteBatch, Vector2.Zero);

            // Render player in world coordinates (same coordinate system as level)
            _player.Draw(_spriteBatch, _pixel, Color.Red);

            _spriteBatch.End();

            // STEP 2: Render the fixed resolution target to screen with aspect ratio scaling
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            var destinationRect = CalculateDestinationRectangle();
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_renderTarget, destinationRect, Color.White);
            _spriteBatch.End();
        }
        else
        {
            // Fallback if no level loaded
            GraphicsDevice.Clear(Color.Black);
        }
    }

    protected override void UnloadContent()
    {
        _renderTarget?.Dispose();
        base.UnloadContent();
    }
}