using System.Threading.Tasks;
using LDtk;

namespace OctarineCodex.Maps;

/// <summary>
///     Simplified map service for loading and rendering a single LDtk level without complex room transitions.
/// </summary>
public interface ISimpleMapService
{
    /// <summary>
    ///     Gets the currently loaded level.
    /// </summary>
    LDtkLevel? CurrentLevel { get; }

    /// <summary>
    ///     Indicates if a level is currently loaded.
    /// </summary>
    bool IsLevelLoaded { get; }

    /// <summary>
    ///     Loads a single level from an LDtk file.
    /// </summary>
    /// <param name="filePath">Path to the LDtk file</param>
    /// <param name="levelIdentifier">Optional level identifier. If null, loads the first level.</param>
    /// <returns>The loaded level or null if loading failed</returns>
    Task<LDtkLevel?> LoadLevelAsync(LDtkFile file, string? levelIdentifier = null);
}