using System.Collections.Generic;
using System.Threading.Tasks;
using LDtk;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Maps;

/// <summary>
///     Extended map service supporting multiple levels from LDtk files.
/// </summary>
public interface IWorldMapService : ISimpleMapService
{
    /// <summary>
    ///     Gets all loaded levels from the current world.
    /// </summary>
    IReadOnlyList<LDtkLevel> LoadedLevels { get; }

    /// <summary>
    ///     Loads all levels from an LDtk file.
    /// </summary>
    /// <param name="filePath">Path to the LDtk file</param>
    /// <returns>List of loaded levels or empty list if loading failed</returns>
    Task<IReadOnlyList<LDtkLevel>> LoadWorldAsync(LDtkFile file);

    /// <summary>
    ///     Gets the level containing the specified world position.
    /// </summary>
    /// <param name="worldPosition">World coordinates</param>
    /// <returns>Level containing the position, or null</returns>
    LDtkLevel? GetLevelAt(Vector2 worldPosition);
}