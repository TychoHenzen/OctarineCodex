using System.Collections.Generic;
using System.Threading.Tasks;
using LDtk;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Maps;

/// <summary>
///     Unified map service interface that handles loading and managing LDtk levels.
///     Supports both single-level and multi-level scenarios through a consistent API.
/// </summary>
public interface IMapService
{
    /// <summary>
    ///     Gets all currently loaded levels. Even single-level scenarios return a collection with one item.
    /// </summary>
    IReadOnlyList<LDtkLevel> CurrentLevels { get; }

    /// <summary>
    ///     Gets all currently loaded levels. Even single-level scenarios return a collection with one item.
    /// </summary>
    LDtkFile LoadedFile { get; }

    /// <summary>
    ///     Indicates if any levels are currently loaded.
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    ///     Loads levels from an LDtk file according to the specified options.
    /// </summary>
    /// <param name="file">The LDtk file to load from</param>
    /// <param name="options">Optional loading configuration. If null, loads all levels.</param>
    /// <returns>True if at least one level was loaded successfully</returns>
    Task<bool> LoadAsync(LDtkFile file, MapLoadOptions? options = null);

    /// <summary>
    ///     Gets the level containing the specified world position.
    /// </summary>
    /// <param name="worldPosition">World coordinates to query</param>
    /// <returns>The level containing the position, or null if no level contains it</returns>
    LDtkLevel? GetLevelAt(Vector2 worldPosition);

    /// <summary>
    ///     Calculates the bounding rectangle that encompasses all loaded levels.
    /// </summary>
    /// <returns>World bounds rectangle, or empty rectangle if no levels loaded</returns>
    RectangleF GetWorldBounds();
}

/// <summary>
///     Configuration options for loading LDtk files.
/// </summary>
public class MapLoadOptions
{
    /// <summary>
    ///     Load a specific level by identifier. If null, behavior depends on LoadAllLevels.
    /// </summary>
    public string? SpecificLevelIdentifier { get; set; }

    /// <summary>
    ///     If true, loads all levels from the file. If false, loads only the first level or the specified level.
    /// </summary>
    public bool LoadAllLevels { get; set; } = true;
}