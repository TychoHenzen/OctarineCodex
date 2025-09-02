using System.Collections.Generic;
using System.Linq;
using LDtk;
using Microsoft.Xna.Framework;
using OctarineCodex.Logging;

namespace OctarineCodex.Maps;

public class WorldLayerService : IWorldLayerService
{
    private readonly ILoggingService _logger;
    private IReadOnlyList<LDtkLevel> _allLevels = new List<LDtkLevel>();

    public WorldLayerService(ILoggingService logger)
    {
        _logger = logger;
    }

    public int CurrentWorldDepth { get; private set; }

    public IReadOnlyList<LDtkLevel> GetCurrentLayerLevels()
    {
        return _allLevels.Where(level => level.WorldDepth == CurrentWorldDepth).ToList();
    }

    public IReadOnlyList<LDtkLevel> GetAllLevels()
    {
        return _allLevels;
    }

    public bool SwitchToLayer(int worldDepth)
    {
        var levelsAtDepth = _allLevels.Where(l => l.WorldDepth == worldDepth).ToList();
        if (!levelsAtDepth.Any())
        {
            _logger.Warn($"No levels found at world depth {worldDepth}");
            return false;
        }

        CurrentWorldDepth = worldDepth;
        _logger.Debug($"Switched to world layer {worldDepth} with {levelsAtDepth.Count} levels");
        return true;
    }

    public LDtkLevel? GetLevelAt(Vector2 worldPosition, int? worldDepth = null)
    {
        var targetDepth = worldDepth ?? CurrentWorldDepth;
        return _allLevels.FirstOrDefault(level =>
            level.WorldDepth == targetDepth &&
            worldPosition.X >= level.WorldX &&
            worldPosition.X < level.WorldX + level.PxWid &&
            worldPosition.Y >= level.WorldY &&
            worldPosition.Y < level.WorldY + level.PxHei);
    }

    public void InitializeLevels(IReadOnlyList<LDtkLevel> levels)
    {
        _allLevels = levels;

        // Set initial world depth to the most common depth, or 0
        var depthGroups = levels.GroupBy(l => l.WorldDepth).OrderByDescending(g => g.Count());
        CurrentWorldDepth = depthGroups.FirstOrDefault()?.Key ?? 0;

        _logger.Debug($"Initialized with {levels.Count} levels across {depthGroups.Count()} world depths");
        _logger.Debug($"Starting at world depth {CurrentWorldDepth}");
    }
}