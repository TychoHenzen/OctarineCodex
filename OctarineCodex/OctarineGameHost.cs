using System;
using System.IO;
using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Collisions;
using OctarineCodex.Entities;
using OctarineCodex.Entities.Behaviors;
using OctarineCodex.Input;
using OctarineCodex.Logging;
using OctarineCodex.Maps;
using OctarineCodex.Services;
using static OctarineCodex.OctarineConstants;

namespace OctarineCodex;

public class OctarineGameHost : Game
{
    // Services - injected via DI
    private readonly ICameraService _cameraService;
    private readonly ICollisionSystem _collisionSystem;
    private readonly IEntityService _entityService;
    private readonly IInputService _inputService;
    private readonly ILevelRenderer _levelRenderer;
    private readonly ILoggingService _logger;
    private readonly IMapService _mapService;
    private readonly ITeleportService _teleportService;
    private readonly IWorldLayerService _worldLayerService;

    // Rendering system
    private RenderTarget2D _renderTarget = null!;
    private SpriteBatch _spriteBatch = null!;
    private readonly GraphicsDeviceManager _graphics;

    public OctarineGameHost(
        ILoggingService logger,
        IInputService inputService,
        IMapService mapService,
        ILevelRenderer levelRenderer,
        ICollisionSystem collisionSystem,
        IEntityService entityService,
        IWorldLayerService worldLayerService,
        ITeleportService teleportService,
        ICameraService cameraService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
        _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
        _levelRenderer = levelRenderer ?? throw new ArgumentNullException(nameof(levelRenderer));
        _collisionSystem = collisionSystem ?? throw new ArgumentNullException(nameof(collisionSystem));
        _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        _worldLayerService = worldLayerService ?? throw new ArgumentNullException(nameof(worldLayerService));
        _teleportService = teleportService ?? throw new ArgumentNullException(nameof(teleportService));
        _cameraService = cameraService ?? throw new ArgumentNullException(nameof(cameraService));

        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Improve smoothing of input by decoupling updates from VSync and fixed timestep
        IsFixedTimeStep = false;
        _graphics.SynchronizeWithVerticalRetrace = false;
        _graphics.ApplyChanges();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _renderTarget = new RenderTarget2D(GraphicsDevice, FixedWidth, FixedHeight);
        Pixel = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
        Pixel.SetData([Color.White]);

        // Initialize unified renderer
        _levelRenderer.Initialize(GraphicsDevice);

        try
        {
            // Try to load primary level file (multi-level support)
            var primaryFile = TryLoadLdtkFile(WorldName);
            if (primaryFile != null)
            {
                _logger.Debug($"Attempting to load {primaryFile.FilePath} (multi-level)");
                if (_mapService.Load(primaryFile))
                {
                    _logger.Debug(
                        $"Successfully loaded {_mapService.CurrentLevels.Count} levels from test_level2.ldtk");
                    InitializeLoadedWorld();
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

    private LDtkFile? TryLoadLdtkFile(string fileName)
    {
        try
        {
            var filePath = Path.Combine(Content.RootDirectory, fileName);
            _logger.Debug($"Attempting to load {fileName} from: {filePath}");
            _logger.Debug($"File exists: {File.Exists(filePath)}");

            if (!File.Exists(filePath))
            {
                return null;
            }

            return LDtkFile.FromFile(filePath);
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to load {fileName}: {ex.Message}");
            return null;
        }
    }

    private void InitializeLoadedWorld()
    {
        if (!_mapService.IsLoaded)
        {
            _logger.Error("Cannot initialize world - no levels loaded");
            return;
        }

        // Set LDtk context and load tilesets
        _levelRenderer.SetLDtkContext(_mapService.LoadedFile); // All levels share same project
        _levelRenderer.LoadTilesets(Content);

        // Initialize world systems with all loaded levels
        _worldLayerService.InitializeLevels(_mapService.CurrentLevels);
        _entityService.InitializeEntities(_mapService.CurrentLevels);

        // Initialize new collision system for current layer
        var currentLayerLevels = _worldLayerService.GetCurrentLayerLevels();
        _collisionSystem.InitializeLevels(currentLayerLevels);
        _entityService.UpdateEntitiesForCurrentLayer(currentLayerLevels);
        _teleportService.InitializeTeleports();
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

    protected override void Update(GameTime gameTime)
    {
        _inputService.Update(gameTime);
        if (_inputService.IsExitPressed())
        {
            Exit();
        }

        if (!_mapService.IsLoaded)
        {
            return;
        }

        // Update all entities (this includes player movement, camera following, and teleportation)
        _entityService.Update(gameTime);

        // Process collision system events (triggers, collision messages, etc.)
        _collisionSystem.ProcessCollisions();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);

        if (_mapService.IsLoaded)
        {
            var player = _entityService.GetPlayerEntity();
            var worldMatrix = _cameraService.GetTransformMatrix() * Matrix.CreateScale(WorldRenderScale);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: worldMatrix);

            var currentLayerLevels = _worldLayerService.GetCurrentLayerLevels();

            // Render background and collision layers (behind player)
            _levelRenderer.RenderLevelsBeforePlayer(currentLayerLevels, _spriteBatch, _cameraService.Camera,
                player?.Position ?? Vector2.Zero);

            // Render player at correct depth
            _entityService.Draw(_spriteBatch);

            // Render wall tiles in front of player (Y-sorted)
            _levelRenderer.RenderLevelsAfterPlayer(currentLayerLevels, _spriteBatch, _cameraService.Camera,
                player?.Position ?? Vector2.Zero);

            // Render foreground tiles (always on top)
            _levelRenderer.RenderForegroundLayers(currentLayerLevels, _spriteBatch, _cameraService.Camera,
                player?.Position ?? Vector2.Zero);

            // Draw teleport indicator if available (only for multi-level worlds)
            if (player != null && _mapService.CurrentLevels.Count > 1)
            {
                var teleportBehavior = player.GetBehavior<PlayerTeleportBehavior>();
                if (teleportBehavior?.IsTeleportAvailable(out _, out _) == true)
                {
                    DrawTeleportIndicator(gameTime, player.Position);
                }
            }

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

    private void DrawTeleportIndicator(GameTime gameTime, Vector2 playerPosition)
    {
        // Draw a pulsing outline around teleporter area
        var pulseIntensity = 0.5f + 0.5f * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 4.0);
        var indicatorColor = Color.Cyan * pulseIntensity;

        // Draw simple indicator near player
        var indicatorRect = new Rectangle(
            (int)(playerPosition.X - 8),
            (int)(playerPosition.Y - 12),
            PlayerSize + 16,
            4);
        _spriteBatch.Draw(Pixel, indicatorRect, indicatorColor);
    }

    protected override void UnloadContent()
    {
        _renderTarget?.Dispose();
        Pixel?.Dispose();
        base.UnloadContent();
    }
}
