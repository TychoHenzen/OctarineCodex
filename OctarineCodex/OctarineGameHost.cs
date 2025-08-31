using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    private readonly ICollisionService _collisionService;
    private readonly IEntityService _entityService;

    // Graphics
    private readonly GraphicsDeviceManager _graphics;

    // Services
    private readonly IInputService _inputService;
    private readonly ILoggingService _logger;
    private readonly ISimpleLevelRenderer _simpleLevelRenderer;

    // Fallback services for Room1.ldtk compatibility
    private readonly ISimpleMapService _simpleMapService;
    private readonly IWorldMapService _worldMapService;
    private readonly IWorldRenderer _worldRenderer;

    // Game state
    private Camera2D _camera = null!;
    private LDtkLevel? _currentLevel;
    private IReadOnlyList<LDtkLevel> _loadedLevels = [];
    private Texture2D _pixel = null!;
    private Player _player = null!;

    // Rendering system
    private RenderTarget2D _renderTarget = null!;
    private SpriteBatch _spriteBatch = null!;

    public OctarineGameHost(
        ILoggingService logger,
        IInputService inputService,
        IWorldMapService worldMapService,
        ICollisionService collisionService,
        IEntityService entityService,
        IWorldRenderer worldRenderer,
        ISimpleMapService simpleMapService,
        ISimpleLevelRenderer simpleLevelRenderer)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
        _worldMapService = worldMapService ?? throw new ArgumentNullException(nameof(worldMapService));
        _collisionService = collisionService ?? throw new ArgumentNullException(nameof(collisionService));
        _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        _worldRenderer = worldRenderer ?? throw new ArgumentNullException(nameof(worldRenderer));
        _simpleMapService = simpleMapService ?? throw new ArgumentNullException(nameof(simpleMapService));
        _simpleLevelRenderer = simpleLevelRenderer ?? throw new ArgumentNullException(nameof(simpleLevelRenderer));

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

    private Rectangle CalculateDestinationRectangle()
    {
        var windowWidth = GraphicsDevice.Viewport.Width;
        var windowHeight = GraphicsDevice.Viewport.Height;

        var scaleX = (float)windowWidth / FixedWidth;
        var scaleY = (float)windowHeight / FixedHeight;
        var scale = Math.Min(scaleX, scaleY);

        var scaledWidth = (int)(FixedWidth * scale);
        var scaledHeight = (int)(FixedHeight * scale);

        var x = (windowWidth - scaledWidth) / 2;
        var y = (windowHeight - scaledHeight) / 2;

        return new Rectangle(x, y, scaledWidth, scaledHeight);
    }

    protected override async void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _renderTarget = new RenderTarget2D(GraphicsDevice, FixedWidth, FixedHeight);

        _pixel = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
        _pixel.SetData(new[] { Color.White });

        // Initialize renderers
        _worldRenderer.Initialize(GraphicsDevice);
        _simpleLevelRenderer.Initialize(GraphicsDevice);

        // Try Room2.ldtk first (multi-level support)
        var room2Path = Path.GetFullPath(Path.Combine(Content.RootDirectory, "Room2.ldtk"));
        _logger.Debug($"Attempting to load Room2.ldtk from: {room2Path}");
        _logger.Debug($"File exists: {File.Exists(room2Path)}");


        _loadedLevels = await _worldMapService.LoadWorldAsync(room2Path);

        if (_loadedLevels.Any())
        {
            _logger.Debug($"Room2.ldtk loaded successfully with {_loadedLevels.Count} levels");
            await LoadMultiLevelWorld();
        }
        else
        {
            // Fallback to Room1.ldtk (single level)
            _logger.Info("Room2.ldtk not found, falling back to Room1.ldtk");
            await LoadSingleLevel();
        }
    }

    private async Task LoadMultiLevelWorld()
    {
        // Initialize collision system
        _collisionService.InitializeCollision(_loadedLevels);

        // Initialize entity system
        _entityService.InitializeEntities(_loadedLevels);

        // Load tilesets for all levels
        await _worldRenderer.LoadTilesetsForWorldAsync(_loadedLevels, Content);

        // Find player spawn point
        var spawnPosition = _entityService.GetPlayerSpawnPoint();
        if (!spawnPosition.HasValue)
        {
            // Fallback to first level center
            var firstLevel = _loadedLevels[0];
            spawnPosition = new Vector2(
                firstLevel.WorldX + firstLevel.PxWid / 2f,
                firstLevel.WorldY + firstLevel.PxHei / 2f
            );
        }

        _player = new Player(spawnPosition.Value);

        // Calculate world bounds for camera
        var minX = _loadedLevels.Min(l => l.WorldX);
        var minY = _loadedLevels.Min(l => l.WorldY);
        var maxX = _loadedLevels.Max(l => l.WorldX + l.PxWid);
        var maxY = _loadedLevels.Max(l => l.WorldY + l.PxHei);

        var worldSize = new Vector2(maxX - minX, maxY - minY);
        _camera.FollowPlayer(_player, new Vector2(minX, minY), worldSize);

        _logger.Debug($"World bounds: ({minX}, {minY}) to ({maxX}, {maxY})");
        _logger.Debug($"Player spawn: {spawnPosition}");
    }

    private async Task LoadSingleLevel()
    {
        var room1Path = Path.Combine(Content.RootDirectory, "Room1.ldtk");
        _logger.Debug($"Loading Room1.ldtk from: {room1Path}");
        _currentLevel = await _simpleMapService.LoadLevelAsync(room1Path);

        if (_currentLevel != null)
        {
            await _simpleLevelRenderer.LoadTilesetsAsync(_currentLevel, Content);

            var spawnPosition = FindPlayerSpawnPoint(_currentLevel);
            _player = new Player(spawnPosition);

            var roomSize = new Vector2(_currentLevel.PxWid, _currentLevel.PxHei);
            _camera.FollowPlayer(_player, Vector2.Zero, roomSize);

            _logger.Debug($"Level loaded - Size: {_currentLevel.PxWid}x{_currentLevel.PxHei}");
            _logger.Debug($"Player spawn position: {_player.Position}");
        }
        else
        {
            _logger.Error("Failed to load Room1.ldtk");
        }
    }

    private Vector2 FindPlayerSpawnPoint(LDtkLevel level)
    {
        foreach (var layer in level.LayerInstances)
            if (layer._Type == LayerType.Entities)
            {
                var playerEntity = layer.EntityInstances
                    .FirstOrDefault(e => e._Identifier.Equals("Player", StringComparison.OrdinalIgnoreCase));

                if (playerEntity != null)
                {
                    var spawnPos = new Vector2(playerEntity.Px.X, playerEntity.Px.Y);
                    spawnPos.X = MathHelper.Clamp(spawnPos.X, 0, level.PxWid - Player.Size);
                    spawnPos.Y = MathHelper.Clamp(spawnPos.Y, 0, level.PxHei - Player.Size);

                    _logger.Debug($"Found player spawn point at ({spawnPos.X}, {spawnPos.Y})");
                    return spawnPos;
                }
            }

        // Fallback to level center
        var centerPos = new Vector2(
            MathHelper.Clamp(level.PxWid / 2f - Player.Size / 2f, 0, level.PxWid - Player.Size),
            MathHelper.Clamp(level.PxHei / 2f - Player.Size / 2f, 0, level.PxHei - Player.Size)
        );
        _logger.Debug($"No player spawn point found, using level center: ({centerPos.X}, {centerPos.Y})");
        return centerPos;
    }

    protected override void Update(GameTime gameTime)
    {
        _inputService.Update(gameTime);
        if (_inputService.IsExitPressed()) Exit();

        if (_player == null) return;

        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var dir = _inputService.GetMovementDirection();
        var delta = Movement.ComputeDelta(dir, PlayerSpeed, dt);

        if (_loadedLevels.Any())
        {
            // Multi-level world with collision
            var newPos = _player.Position + delta;
            var correctedPos = _collisionService.ResolveCollision(
                _player.Position, newPos, new Vector2(Player.Size, Player.Size));

            _player.SetPosition(correctedPos);

            // Update camera for world bounds
            var minX = _loadedLevels.Min(l => l.WorldX);
            var minY = _loadedLevels.Min(l => l.WorldY);
            var maxX = _loadedLevels.Max(l => l.WorldX + l.PxWid);
            var maxY = _loadedLevels.Max(l => l.WorldY + l.PxHei);

            var worldSize = new Vector2(maxX - minX, maxY - minY);
            _camera.FollowPlayer(_player, new Vector2(minX, minY), worldSize);
        }
        else if (_currentLevel != null)
        {
            // Single level with simple bounds checking
            var levelSize = new Vector2(_currentLevel.PxWid, _currentLevel.PxHei);
            _player.Update(delta, levelSize);

            var roomSize = new Vector2(_currentLevel.PxWid, _currentLevel.PxHei);
            _camera.FollowPlayer(_player, Vector2.Zero, roomSize);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);

        if (_player != null)
        {
            var worldMatrix = _camera.GetTransformMatrix() * Matrix.CreateScale(WorldRenderScale);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: worldMatrix);

            if (_loadedLevels.Any())
                // Render multi-level world
                _worldRenderer.RenderWorld(_loadedLevels, _spriteBatch, _camera);
            else if (_currentLevel != null)
                // Render single level
                _simpleLevelRenderer.RenderLevel(_currentLevel, _spriteBatch, Vector2.Zero);

            // Render player
            _player.Draw(_spriteBatch, _pixel, Color.Red);

            _spriteBatch.End();
        }

        // Render to screen
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        var destinationRect = CalculateDestinationRectangle();
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_renderTarget, destinationRect, Color.White);
        _spriteBatch.End();
    }

    protected override void UnloadContent()
    {
        _renderTarget?.Dispose();
        _pixel?.Dispose();
        base.UnloadContent();
    }
}