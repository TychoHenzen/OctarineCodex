using System;
using System.Collections.Generic;
using System.Linq;
using LDtk;
using Microsoft.Xna.Framework;
using OctarineCodex.Logging;

namespace OctarineCodex.Maps;

/// <summary>
///     Unified map service that handles loading and managing LDtk levels, whether single or multiple.
///     Replaces both SimpleMapService and WorldMapService with a single, flexible implementation.
/// </summary>
public class MapService : IMapService
{
    private readonly List<LDtkLevel> _currentLevels = new();
    private readonly ILoggingService _logger;

    public MapService(ILoggingService logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Gets all currently loaded levels. Even single-level scenarios return a collection with one item.
    /// </summary>
    public IReadOnlyList<LDtkLevel> CurrentLevels => _currentLevels.AsReadOnly();

    /// <summary>
    ///     Gets all currently loaded levels. Even single-level scenarios return a collection with one item.
    /// </summary>
    public LDtkFile LoadedFile { get; private set; }

    /// <summary>
    ///     Indicates if any levels are currently loaded.
    /// </summary>
    public bool IsLoaded => _currentLevels.Any();

    /// <summary>
    ///     Gets the level containing the specified world position.
    /// </summary>
    /// <param name="worldPosition">World coordinates to query</param>
    /// <returns>The level containing the position, or null if no level contains it</returns>
    public LDtkLevel? GetLevelAt(Vector2 worldPosition)
    {
        return _currentLevels.FirstOrDefault(level =>
            worldPosition.X >= level.WorldX &&
            worldPosition.X < level.WorldX + level.PxWid &&
            worldPosition.Y >= level.WorldY &&
            worldPosition.Y < level.WorldY + level.PxHei);
    }

    /// <summary>
    ///     Calculates the bounding rectangle that encompasses all loaded levels.
    /// </summary>
    /// <returns>World bounds rectangle, or empty rectangle if no levels loaded</returns>
    public RectangleF GetWorldBounds()
    {
        if (!_currentLevels.Any())
            return new RectangleF(0, 0, 0, 0);

        var minX = _currentLevels.Min(l => l.WorldX);
        var minY = _currentLevels.Min(l => l.WorldY);
        var maxX = _currentLevels.Max(l => l.WorldX + l.PxWid);
        var maxY = _currentLevels.Max(l => l.WorldY + l.PxHei);

        return new RectangleF(minX, minY, maxX - minX, maxY - minY);
    }

    /// <summary>
    ///     Loads levels from an LDtk file according to the specified options.
    /// </summary>
    /// <param name="file">The LDtk file to load from</param>
    /// <param name="options">Loading configuration options</param>
    /// <returns>True if at least one level was loaded successfully</returns>
    public bool Load(LDtkFile file, MapLoadOptions? options = null)
    {
        options ??= new MapLoadOptions();

        try
        {
            _currentLevels.Clear();

            // Extract levels from file (handles both world format and legacy format)
            var availableLevels = ExtractLevelsFromFile(file);

            if (!availableLevels.Any())
            {
                _logger.Debug("No levels found in LDtk file");
                return false;
            }

            // Apply loading options to determine which levels to load
            var levelsToLoad = ApplyLoadingOptions(availableLevels, options);

            if (!levelsToLoad.Any())
            {
                _logger.Debug(
                    $"No levels matched loading criteria. Available: [{string.Join(", ", availableLevels.Select(l => l.Identifier))}]");
                return false;
            }

            _currentLevels.AddRange(levelsToLoad);

            _logger.Debug(
                $"Successfully loaded {_currentLevels.Count} level(s): [{string.Join(", ", _currentLevels.Select(l => l.Identifier))}]");
            LoadedFile = file;
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to load LDtk file: {ex.Message}");
            _currentLevels.Clear();
            return false;
        }
    }

    /// <summary>
    ///     Extracts all available levels from an LDtk file, handling both world format and legacy format.
    /// </summary>
    private IEnumerable<LDtkLevel> ExtractLevelsFromFile(LDtkFile file)
    {
        var levels = new List<LDtkLevel>();

        // Try newer world format first
        if (file.Worlds.Any())
        {
            _logger.Debug($"Found {file.Worlds.Length} world(s) in file");

            foreach (var worldDef in file.Worlds)
                try
                {
                    var world = file.LoadWorld(worldDef.Iid);
                    if (world?.Levels != null)
                    {
                        levels.AddRange(world.Levels);
                        _logger.Debug($"Loaded {world.Levels.Count()} levels from world '{worldDef.Identifier}'");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to load world '{worldDef.Identifier}': {ex.Message}");
                }
        }
        // Fall back to legacy format (levels directly in project)
        else if (file.Levels != null && file.Levels.Any())
        {
            levels.AddRange(file.Levels);
            _logger.Debug($"Loaded {file.Levels.Count()} levels from legacy format");
        }

        return levels;
    }

    /// <summary>
    ///     Filters the available levels according to the loading options.
    /// </summary>
    private IEnumerable<LDtkLevel> ApplyLoadingOptions(IEnumerable<LDtkLevel> availableLevels, MapLoadOptions options)
    {
        var levelsList = availableLevels.ToList();

        // If a specific level is requested, try to find it
        if (!string.IsNullOrEmpty(options.SpecificLevelIdentifier))
        {
            var specificLevel = levelsList.FirstOrDefault(level =>
                string.Equals(level.Identifier, options.SpecificLevelIdentifier, StringComparison.OrdinalIgnoreCase));

            if (specificLevel != null)
            {
                _logger.Debug($"Found specific level: {options.SpecificLevelIdentifier}");
                return new[] { specificLevel };
            }

            _logger.Debug($"Specific level '{options.SpecificLevelIdentifier}' not found");
            return Enumerable.Empty<LDtkLevel>();
        }

        // If LoadAllLevels is true, return all levels
        if (options.LoadAllLevels)
        {
            _logger.Debug("Loading all available levels");
            return levelsList;
        }

        // If LoadAllLevels is false, return only the first level
        if (levelsList.Any())
        {
            var firstLevel = levelsList[0];
            _logger.Debug($"Loading single level: {firstLevel.Identifier}");
            return [firstLevel];
        }

        return Enumerable.Empty<LDtkLevel>();
    }
}