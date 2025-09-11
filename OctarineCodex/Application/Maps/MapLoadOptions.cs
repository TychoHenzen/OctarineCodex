namespace OctarineCodex.Application.Maps;

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
