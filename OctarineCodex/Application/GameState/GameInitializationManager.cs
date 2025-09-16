// OctarineCodex\Application\GameState\GameInitializationManager.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LDtk;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Application.Entities;
using OctarineCodex.Application.Maps;
using OctarineCodex.Domain.Physics;
using OctarineCodex.Infrastructure.Logging;
using OctarineCodex.Presentation.Rendering;

namespace OctarineCodex.Application.GameState;

public class GameInitializationManager(
    IMapService mapService,
    ILevelRenderer levelRenderer,
    IWorldLayerService worldLayerService,
    IEntityService entityService,
    ICollisionSystem collisionSystem,
    ITeleportService teleportService,
    ILoggingService logger) : IGameInitializationManager
{
    public bool IsWorldLoaded => mapService.IsLoaded;

    public async Task<bool> InitializeWorldAsync(GraphicsDevice graphicsDevice,
        ContentManager content, string worldName)
    {
        try
        {
            // Initialize renderer first
            levelRenderer.Initialize(graphicsDevice);

            // Load LDTK file asynchronously
            LDtkFile? ldtkFile = await LoadLdtkFileAsync(content, worldName);
            if (ldtkFile == null)
            {
                logger.Error($"Failed to load LDTK file: {worldName}");
                return false;
            }

            logger.Debug($"Successfully loaded LDTK file: {worldName}");

            // Load the world into the map service
            if (!mapService.Load(ldtkFile))
            {
                logger.Error("Failed to load world into map service");
                return false;
            }

            logger.Debug($"Successfully loaded {mapService.CurrentLevels.Count} levels from {worldName}");

            // Initialize all systems in proper order
            InitializeWorldSystems(content);

            logger.Debug("World initialization completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            logger.Exception(ex);
            return false;
        }
    }

    private async Task<LDtkFile?> LoadLdtkFileAsync(ContentManager content, string fileName)
    {
        try
        {
            var filePath = Path.Combine(content.RootDirectory, fileName);
            logger.Debug($"Attempting to load {fileName} from: {filePath}");

            // Check if file exists asynchronously
            if (!File.Exists(filePath))
            {
                logger.Error($"LDTK file not found: {filePath}");
                return null;
            }

            logger.Debug($"File exists: {filePath}");

            // Parse LDTK file
            LDtkFile ldtkFile = LDtkFile.FromFile(filePath);
            ldtkFile.FilePath = filePath; // Set the file path for asset loading

            return ldtkFile;
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to load {fileName}: {ex.Message}");
            return null;
        }
    }

    private void InitializeWorldSystems(ContentManager content)
    {
        // Set LDtk context and load tilesets
        levelRenderer.SetLDtkContext(mapService.LoadedFile);
        levelRenderer.LoadTilesets(content);

        // Initialize world systems with all loaded levels
        worldLayerService.InitializeLevels(mapService.CurrentLevels);
        entityService.InitializeEntities(mapService.CurrentLevels);

        // Initialize collision system for current layer
        IReadOnlyList<LDtkLevel> currentLayerLevels = worldLayerService.GetCurrentLayerLevels();
        collisionSystem.InitializeLevels(currentLayerLevels);
        entityService.UpdateEntitiesForCurrentLayer(currentLayerLevels);

        // Initialize teleport system
        teleportService.InitializeTeleports();

        logger.Debug("All world systems initialized successfully");
    }
}
