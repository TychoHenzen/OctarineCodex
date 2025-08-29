using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OctarineCodex.Maps;

/// <summary>
/// Implementation of LDTK map loading service.
/// Handles parsing LDTK JSON files and managing loaded project data.
/// </summary>
public sealed class LdtkMapService : ILdtkMapService
{
    private LdtkProject? _currentProject;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Gets a value indicating whether a project is currently loaded.
    /// </summary>
    public bool IsProjectLoaded => _currentProject is not null;

    /// <summary>
    /// Loads an LDTK project from the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the LDTK project file.</param>
    /// <returns>A task that represents the asynchronous load operation. The task result contains the loaded project or null if loading failed.</returns>
    public async Task<LdtkProject?> LoadProjectAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            var jsonContent = await File.ReadAllTextAsync(filePath);
            var project = JsonSerializer.Deserialize<LdtkProject>(jsonContent, JsonOptions);
            
            if (project is not null)
            {
                _currentProject = project;
            }

            return project;
        }
        catch (JsonException)
        {
            // Invalid JSON format
            return null;
        }
        catch (Exception)
        {
            // Other file system or parsing errors
            return null;
        }
    }

    /// <summary>
    /// Gets a level from the currently loaded project by its identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the level to retrieve.</param>
    /// <returns>The level with the specified identifier, or null if not found.</returns>
    public LdtkLevel? GetLevel(string identifier)
    {
        if (_currentProject is null)
        {
            return null;
        }

        return _currentProject.Levels.FirstOrDefault(level => 
            string.Equals(level.Identifier, identifier, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all levels from the currently loaded project.
    /// </summary>
    /// <returns>An array of all levels in the project.</returns>
    public LdtkLevel[] GetAllLevels()
    {
        return _currentProject?.Levels ?? [];
    }

    /// <summary>
    /// Gets the currently loaded project.
    /// </summary>
    /// <returns>The currently loaded project, or null if no project is loaded.</returns>
    public LdtkProject? GetCurrentProject()
    {
        return _currentProject;
    }
}