using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Input;
using OctarineCodex.Logging;
using OctarineCodex.Maps;
using OctarineCodex.Player;

namespace OctarineCodex;

public class OctarineGameHost : Game
{
    private const float PlayerSpeed = 220f; // pixels per second
    private const float WorldRenderScale = 4.0f; // Scale factor for world/level rendering

    // Fixed resolution constants
    private const int FixedWidth = 640;
    private const int FixedHeight = 480;

    // Services - now unified
    private readonly ICollisionService _collisionService;
    private readonly IEntityService _entityService;
    private readonly GraphicsDeviceManager _graphics;
    private readonly IInputService _inputService;
    private readonly ILevelRenderer _levelRenderer;
    private readonly ILoggingService _logger;
    private readonly IMapService _mapService;
    private readonly ITeleportService _teleportService;
    private readonly IWorldLayerService _worldLayerService;

    // Game state
    private Camera2D _camera = null!;
    private Texture2D _pixel = null!;
    private PlayerControl _player = null!;

    // Rendering system
    private RenderTarget2D _renderTarget = null!;
    private SpriteBatch _spriteBatch = null!;

    public OctarineGameHost(
        ILoggingService logger,
        IInputService inputService,
        IMapService mapService,
        ILevelRenderer levelRenderer,
        ICollisionService collisionService,
        IEntityService entityService,
        IWorldLayerService worldLayerService,
        ITeleportService teleportService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
        _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
        _levelRenderer = levelRenderer ?? throw new ArgumentNullException(nameof(levelRenderer));
        _collisionService = collisionService ?? throw new ArgumentNullException(nameof(collisionService));
        _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        _worldLayerService = worldLayerService ?? throw new ArgumentNullException(nameof(worldLayerService));
        _teleportService = teleportService ?? throw new ArgumentNullException(nameof(teleportService));

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
        var worldViewportSize = new Vector2(FixedWidth, FixedHeight) / WorldRenderScale;
        _camera = new Camera2D(worldViewportSize);

        _logger.Debug($"Window size: {GraphicsDevice.Viewport.Width}x{GraphicsDevice.Viewport.Height}");
        _logger.Debug($"Fixed resolution: {FixedWidth}x{FixedHeight}");
        _logger.Debug($"World viewport (camera sees): {worldViewportSize}");
        _logger.Debug($"Player size: {PlayerControl.Size}x{PlayerControl.Size}");
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
        try
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderTarget = new RenderTarget2D(GraphicsDevice, FixedWidth, FixedHeight);

            _pixel = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            _pixel.SetData(new[] { Color.White });

            // Initialize unified renderer
            _levelRenderer.Initialize(GraphicsDevice);

            // Try to load primary level file (multi-level support)
            var primaryFile = await TryLoadLdtkFile("Room2.ldtk");
            if (primaryFile != null)
            {
                _logger.Debug("Attempting to load test_level2.ldtk (multi-level)");
                if (await _mapService.LoadAsync(primaryFile))
                {
                    _logger.Debug(
                        $"Successfully loaded {_mapService.CurrentLevels.Count} levels from test_level2.ldtk");
                    await InitializeLoadedWorld();
                    return;
                }
            }

            // Fallback to single level file
            _logger.Info("test_level2.ldtk not available, falling back to Room1.ldtk");
            var fallbackFile = await TryLoadLdtkFile("Room1.ldtk");
            if (fallbackFile != null)
            {
                // Load as single level (don't load all levels if it's a multi-level file)
                var singleLevelOptions = new MapLoadOptions { LoadAllLevels = false };
                if (await _mapService.LoadAsync(fallbackFile, singleLevelOptions))
                {
                    _logger.Debug("Successfully loaded single level from Room1.ldtk");
                    await InitializeLoadedWorld();
                    return;
                }
            }

            _logger.Error("Failed to load any level files");
        }
        catch (Exception e)
        {
            _logger.Exception(e);
        }
    }

    private async Task<LDtkFile?> TryLoadLdtkFile(string fileName)
    {
        try
        {
            var filePath = Path.Combine(Content.RootDirectory, fileName);
            _logger.Debug($"Attempting to load {fileName} from: {filePath}");
            _logger.Debug($"File exists: {File.Exists(filePath)}");

            if (!File.Exists(filePath))
                return null;

            return await Task.Run(() => LDtkFile.FromFile(filePath));
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to load {fileName}: {ex.Message}");
            return null;
        }
    }

    private async Task InitializeLoadedWorld()
    {
        if (!_mapService.IsLoaded)
        {
            _logger.Error("Cannot initialize world - no levels loaded");
            return;
        }

        // Set LDtk context and load tilesets
        _levelRenderer.SetLDtkContext(_mapService.LoadedFile); // All levels share same project
        await _levelRenderer.LoadTilesetsAsync(Content);

        // Initialize world systems with all loaded levels
        _worldLayerService.InitializeLevels(_mapService.CurrentLevels);
        _entityService.InitializeEntities(_mapService.CurrentLevels);

        // Initialize systems for current layer
        var currentLayerLevels = _worldLayerService.GetCurrentLayerLevels();
        _collisionService.InitializeCollision(currentLayerLevels);
        _entityService.UpdateEntitiesForCurrentLayer(currentLayerLevels);
        _teleportService.InitializeTeleports();

        // Find player spawn point
        var spawnPosition = FindPlayerSpawn();
        if (!spawnPosition.HasValue) spawnPosition = CalculateDefaultSpawn();

        _player = new PlayerControl(spawnPosition.Value);

        // Set up camera bounds
        UpdateCameraBounds();

        _logger.Debug($"World initialized - Player spawn: {spawnPosition}");
        _logger.Debug($"World bounds: {_mapService.GetWorldBounds()}");
    }

    private Vector2? FindPlayerSpawn()
    {
        // Try entity service first (works for multi-level worlds with entities)
        var entitySpawn = _entityService.GetPlayerSpawnPoint();
        if (entitySpawn.HasValue)
        {
            _logger.Debug($"Found player spawn from entity service: {entitySpawn}");
            return entitySpawn;
        }

        // Fallback to manual search in current layer levels
        var currentLevels = _worldLayerService.GetCurrentLayerLevels();
        foreach (var level in currentLevels)
        {
            var levelSpawn = FindPlayerSpawnInLevel(level);
            if (levelSpawn.HasValue)
            {
                _logger.Debug($"Found player spawn in level {level.Identifier}: {levelSpawn}");
                return levelSpawn;
            }
        }

        return null;
    }

    private Vector2? FindPlayerSpawnInLevel(LDtkLevel level)
    {
        if (level.LayerInstances == null)
            return null;

        foreach (var layer in level.LayerInstances)
            if (layer._Type == LayerType.Entities)
            {
                var playerEntity = layer.EntityInstances
                    .FirstOrDefault(e => e._Identifier.Equals("Player", StringComparison.OrdinalIgnoreCase) ||
                                         e._Identifier.Equals("PlayerSpawn", StringComparison.OrdinalIgnoreCase));

                if (playerEntity != null)
                {
                    var spawnPos = new Vector2(level.WorldX + playerEntity.Px.X, level.WorldY + playerEntity.Px.Y);
                    return ClampToWorldBounds(spawnPos);
                }
            }

        return null;
    }

    private Vector2 CalculateDefaultSpawn()
    {
        // Use center of first level in current layer as fallback
        var currentLevels = _worldLayerService.GetCurrentLayerLevels();
        var firstLevel = currentLevels.FirstOrDefault();

        Vector2 centerPos;
        if (firstLevel != null)
            centerPos = new Vector2(
                firstLevel.WorldX + firstLevel.PxWid / 2f - PlayerControl.Size / 2f,
                firstLevel.WorldY + firstLevel.PxHei / 2f - PlayerControl.Size / 2f
            );
        else
            // Ultimate fallback - origin
            centerPos = Vector2.Zero;

        var clampedPos = ClampToWorldBounds(centerPos);
        _logger.Debug($"Using default spawn position: {clampedPos}");
        return clampedPos;
    }

    private Vector2 ClampToWorldBounds(Vector2 position)
    {
        var bounds = _mapService.GetWorldBounds();
        return new Vector2(
            MathHelper.Clamp(position.X, bounds.X, bounds.X + bounds.Width - PlayerControl.Size),
            MathHelper.Clamp(position.Y, bounds.Y, bounds.Y + bounds.Height - PlayerControl.Size)
        );
    }

    private void UpdateCameraBounds()
    {
        var currentLayerLevels = _worldLayerService.GetCurrentLayerLevels();
        if (!currentLayerLevels.Any())
            return;

        var minX = currentLayerLevels.Min(l => l.WorldX);
        var minY = currentLayerLevels.Min(l => l.WorldY);
        var maxX = currentLayerLevels.Max(l => l.WorldX + l.PxWid);
        var maxY = currentLayerLevels.Max(l => l.WorldY + l.PxHei);

        var worldSize = new Vector2(maxX - minX, maxY - minY);
        _camera.FollowPlayer(_player, new Vector2(minX, minY), worldSize);
    }

    protected override void Update(GameTime gameTime)
    {
        _inputService.Update(gameTime);
        if (_inputService.IsExitPressed())
            Exit();

        if (_player == null || !_mapService.IsLoaded)
            return;

        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var dir = _inputService.GetMovementDirection();
        var delta = Movement.ComputeDelta(dir, PlayerSpeed, dt);

        // Handle teleportation (only works if multiple levels/layers available)
        if (_mapService.CurrentLevels.Count > 1)
        {
            var teleportPressed = _inputService.IsPrimaryActionPressed();
            if (_teleportService.CheckTeleportInteraction(_player.Position, teleportPressed, out var targetDepth,
                    out var targetPos))
                if (_worldLayerService.SwitchToLayer(targetDepth))
                {
                    _logger.Debug($"Player teleported to world depth {targetDepth}");
                    if (targetPos.HasValue)
                        _player.SetPosition(targetPos.Value);

                    // Update all systems for new layer
                    var newLayerLevels = _worldLayerService.GetCurrentLayerLevels();
                    _collisionService.InitializeCollision(newLayerLevels);
                    _entityService.UpdateEntitiesForCurrentLayer(newLayerLevels);
                    _teleportService.InitializeTeleports();

                    UpdateCameraBounds();
                }
        }

        // Handle movement with collision detection
        var newPos = _player.Position + delta;

        // Use collision detection for multi-level worlds, simple bounds checking for single levels
        Vector2 correctedPos;
        if (_mapService.CurrentLevels.Count > 1)
        {
            correctedPos = _collisionService.ResolveCollision(
                _player.Position, newPos, new Vector2(PlayerControl.Size, PlayerControl.Size));
        }
        else
        {
            // Simple bounds checking for single level
            var bounds = _mapService.GetWorldBounds();
            correctedPos = new Vector2(
                MathHelper.Clamp(newPos.X, bounds.X, bounds.X + bounds.Width - PlayerControl.Size),
                MathHelper.Clamp(newPos.Y, bounds.Y, bounds.Y + bounds.Height - PlayerControl.Size)
            );
        }

        _player.SetPosition(correctedPos);
        if (_mapService.CurrentLevels.Count > 1)
        {
            // Multi-level world: update camera bounds for current layer
            UpdateCameraBounds();
        }
        else
        {
            // Single level: follow player within the single level bounds
            var bounds = _mapService.GetWorldBounds();
            var roomPosition = new Vector2(bounds.X, bounds.Y);
            var roomSize = new Vector2(bounds.Width, bounds.Height);
            _camera.FollowPlayer(_player, roomPosition, roomSize);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);

        if (_player != null && _mapService.IsLoaded)
        {
            var worldMatrix = _camera.GetTransformMatrix() * Matrix.CreateScale(WorldRenderScale);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: worldMatrix);

            var currentLayerLevels = _worldLayerService.GetCurrentLayerLevels();

            // Render background and collision layers (behind player)
            _levelRenderer.RenderLevelsBeforePlayer(currentLayerLevels, _spriteBatch, _camera);

            // Render player at correct depth
            _player.Draw(_spriteBatch, _pixel, Color.Red);

            // Render foreground layers (in front of player)
            _levelRenderer.RenderLevelsAfterPlayer(currentLayerLevels, _spriteBatch, _camera);

            // Draw teleport indicator if available (only for multi-level worlds)
            if (_mapService.CurrentLevels.Count > 1 &&
                _teleportService.IsTeleportAvailable(_player.Position, out var targetDepth, out var targetPos))
                DrawTeleportIndicator(gameTime);

            _spriteBatch.End();
        }

        // Render to screen with scaling
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        var destinationRect = CalculateDestinationRectangle();
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_renderTarget, destinationRect, Color.White);
        _spriteBatch.End();
    }

    private void DrawTeleportIndicator(GameTime gameTime)
    {
        // Draw a pulsing outline around teleporter area
        var pulseIntensity = 0.5f + 0.5f * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 4.0);
        var indicatorColor = Color.Cyan * pulseIntensity;

        // Draw simple indicator near player
        var indicatorRect = new Rectangle(
            (int)(_player.Position.X - 8),
            (int)(_player.Position.Y - 12),
            PlayerControl.Size + 16,
            4);
        _spriteBatch.Draw(_pixel, indicatorRect, indicatorColor);
    }

    protected override void UnloadContent()
    {
        _renderTarget?.Dispose();
        _pixel?.Dispose();
        base.UnloadContent();
    }
}