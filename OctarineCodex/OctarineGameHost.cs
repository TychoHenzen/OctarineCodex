using System;
using System.Collections.Generic;
using System.IO;
using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Application.Entities;
using OctarineCodex.Application.Maps;
using OctarineCodex.Domain.Physics;
using OctarineCodex.Infrastructure.Logging;
using OctarineCodex.Presentation.Camera;
using OctarineCodex.Presentation.Input;
using OctarineCodex.Presentation.Rendering;
using static OctarineCodex.OctarineConstants;

namespace OctarineCodex;

public class OctarineGameHost(
    ILoggingService logger,
    IInputService inputService,
    IMapService mapService,
    ILevelRenderer levelRenderer,
    ICollisionSystem collisionSystem,
    IEntityService entityService,
    IWorldLayerService worldLayerService,
    ITeleportService teleportService,
    ICameraService cameraService)
    : Game
{
    private GraphicsDeviceManager _graphics;

    // Rendering system
    private RenderTarget2D _renderTarget = null!;
    private SpriteBatch _spriteBatch = null!;

    public void init()
    {
        _graphics = new GraphicsDeviceManager(this);
    }

    protected override void Initialize()
    {
        // Graphics manager is already created in constructor
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Improve smoothing of input by decoupling updates from VSync and fixed timestep
        IsFixedTimeStep = false;
        _graphics.SynchronizeWithVerticalRetrace = false;
        _graphics.ApplyChanges();

        base.Initialize(); // Call base after setting up graphics
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _renderTarget = new RenderTarget2D(GraphicsDevice, FixedWidth, FixedHeight);
        Pixel = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
        Pixel.SetData([Color.White]);

        // Initialize unified renderer
        levelRenderer.Initialize(GraphicsDevice);

        try
        {
            // Try to load primary level file (multi-level support)
            var primaryFile = TryLoadLdtkFile(WorldName);
            if (primaryFile != null)
            {
                logger.Debug($"Attempting to load {primaryFile.FilePath} (multi-level)");
                if (mapService.Load(primaryFile))
                {
                    logger.Debug(
                        $"Successfully loaded {mapService.CurrentLevels.Count} levels from test_level2.ldtk");
                    InitializeLoadedWorld();
                    return;
                }
            }

            logger.Error("Failed to load any level files");
        }
        catch (Exception e)
        {
            logger.Exception(e);
        }
    }

    protected override void Update(GameTime gameTime)
    {
        inputService.Update(gameTime);
        if (inputService.IsExitPressed())
        {
            Exit();
        }

        if (!mapService.IsLoaded)
        {
            return;
        }

        // Update all entities (this includes player movement, camera following, and teleportation)
        entityService.Update(gameTime);

        // Process collision system events (triggers, collision messages, etc.)
        collisionSystem.ProcessCollisions();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);

        if (!mapService.IsLoaded)
        {
        }
        else
        {
            EntityWrapper? player = entityService.GetPlayerEntity();
            Matrix worldMatrix = cameraService.GetTransformMatrix() * Matrix.CreateScale(WorldRenderScale);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: worldMatrix);

            IReadOnlyList<LDtkLevel> currentLayerLevels = worldLayerService.GetCurrentLayerLevels();

            // Render background and collision layers (behind player)
            levelRenderer.RenderLevelsBeforePlayer(
                currentLayerLevels,
                _spriteBatch,
                player.Position);

            // Render entities at correct depth (includes teleport indicators via behaviors)
            entityService.Draw(_spriteBatch);

            // Render wall tiles in front of player (Y-sorted)
            levelRenderer.RenderLevelsAfterPlayer(
                currentLayerLevels,
                _spriteBatch,
                player.Position);

            // Render foreground tiles (always on top)
            levelRenderer.RenderForegroundLayers(
                currentLayerLevels,
                _spriteBatch,
                player.Position);

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

    protected override void UnloadContent()
    {
        _renderTarget.Dispose();
        Pixel?.Dispose();
        base.UnloadContent();
    }

    private LDtkFile? TryLoadLdtkFile(string fileName)
    {
        try
        {
            var filePath = Path.Combine(Content.RootDirectory, fileName);
            logger.Debug($"Attempting to load {fileName} from: {filePath}");
            logger.Debug($"File exists: {File.Exists(filePath)}");

            if (!File.Exists(filePath))
            {
                return null;
            }

            return LDtkFile.FromFile(filePath);
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to load {fileName}: {ex.Message}");
            return null;
        }
    }

    private void InitializeLoadedWorld()
    {
        if (!mapService.IsLoaded)
        {
            logger.Error("Cannot initialize world - no levels loaded");
            return;
        }

        // Set LDtk context and load tilesets
        levelRenderer.SetLDtkContext(mapService.LoadedFile); // All levels share same project
        levelRenderer.LoadTilesets(Content);

        // Initialize world systems with all loaded levels
        worldLayerService.InitializeLevels(mapService.CurrentLevels);
        entityService.InitializeEntities(mapService.CurrentLevels);

        // Initialize new collision system for current layer
        IReadOnlyList<LDtkLevel> currentLayerLevels = worldLayerService.GetCurrentLayerLevels();
        collisionSystem.InitializeLevels(currentLayerLevels);
        entityService.UpdateEntitiesForCurrentLayer(currentLayerLevels);
        teleportService.InitializeTeleports();
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
}
