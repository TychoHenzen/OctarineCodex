using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LDtk;
using Microsoft.Xna.Framework;
using OctarineCodex.Logging;

namespace OctarineCodex.Maps;

public class WorldMapService : SimpleMapService, IWorldMapService
{
    private readonly List<LDtkLevel> _loadedLevels = new();
    private readonly ILoggingService _logger;

    public WorldMapService(ILoggingService logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<LDtkLevel> LoadedLevels => _loadedLevels.AsReadOnly();

    public async Task<IReadOnlyList<LDtkLevel>> LoadWorldAsync(LDtkFile file)
    {
        try
        {
            _loadedLevels.Clear();

            // Check if it's legacy format first (single world with multiple levels)
            if (file.Levels != null && file.Levels.Any())
            {
                _loadedLevels.AddRange(file.Levels);
                _logger.Debug($"Loaded {file.Levels.Count()} levels from legacy format");
            }
            // Handle multiworld format
            else if (file.Worlds.Any())
            {
                var firstWorld = file.Worlds.FirstOrDefault();
                _logger.Debug($"Loading world: {firstWorld?.Identifier}");
                if (firstWorld != null)
                {
                    var world = file.LoadWorld(firstWorld.Iid);
                    if (world?.Levels != null)
                    {
                        _loadedLevels.AddRange(world.Levels);
                        _logger.Debug($"Loaded {world.Levels.Count()} levels from world");
                    }
                }
            }

            if (_loadedLevels.Any()) _currentLevel = _loadedLevels[0];
            return _loadedLevels.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to load Room2.ldtk: {ex.Message}");
            _loadedLevels.Clear();
            _currentLevel = null;
            return _loadedLevels.AsReadOnly();
        }
    }

    public LDtkLevel? GetLevelAt(Vector2 worldPosition)
    {
        return _loadedLevels.FirstOrDefault(level =>
            worldPosition.X >= level.WorldX &&
            worldPosition.X < level.WorldX + level.PxWid &&
            worldPosition.Y >= level.WorldY &&
            worldPosition.Y < level.WorldY + level.PxHei);
    }
}