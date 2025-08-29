using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using OctarineCodex.Logging;

namespace OctarineCodex.Maps;

/// <summary>
/// Simple LDtkMonoGame-based map service that bypasses complex conversion.
/// Focuses on direct LDtkMonoGame integration for loading test_level2.ldtk.
/// </summary>
public sealed class LdtkMonoGameMapService : ILdtkMapService
{
    private readonly ILoggingService _logger;
    private LdtkProject? _currentProject;
    private string? _currentFilePath;

    /// <summary>
    /// Initializes a new instance of the LdtkMonoGameMapService class.
    /// </summary>
    /// <param name="logger">The logging service for debug output.</param>
    public LdtkMonoGameMapService(ILoggingService logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a value indicating whether a project is currently loaded.
    /// </summary>
    public bool IsProjectLoaded => _currentProject is not null;

    /// <summary>
    /// Loads an LDTK project from the specified file path using direct file loading.
    /// </summary>
    /// <param name="filePath">The path to the LDTK project file.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    public async Task<LdtkProject?> LoadProjectAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.Error($"LDTK file not found: {filePath}");
                return null;
            }

            _logger.Info($"Loading LDTK file: {filePath}");
            
            // For now, let's try loading with our existing JSON approach first
            // and see if test_level2.ldtk works with our current implementation
            var jsonContent = await File.ReadAllTextAsync(filePath);
            
            // Try to parse with our existing system
            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };
            
            var project = System.Text.Json.JsonSerializer.Deserialize<LdtkProject>(jsonContent, jsonOptions);
            
            if (project is not null)
            {
                _currentProject = project;
                _currentFilePath = filePath;
                _logger.Info($"Successfully loaded LDTK project with {project.Levels.Length} levels");
                return project;
            }
            
            _logger.Error("Failed to parse LDTK project");
            return null;
        }
        catch (Exception ex)
        {
            _logger.Exception(ex, "Error loading LDTK project");
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
            _logger.Warn("No project loaded");
            return null;
        }

        var level = _currentProject.Levels.FirstOrDefault(level => 
            string.Equals(level.Identifier, identifier, StringComparison.OrdinalIgnoreCase));
            
        if (level is null)
        {
            _logger.Warn($"Level '{identifier}' not found. Available levels: {string.Join(", ", _currentProject.Levels.Select(l => l.Identifier))}");
        }
        
        return level;
    }

    /// <summary>
    /// Gets all levels from the currently loaded project.
    /// </summary>
    /// <returns>An array of all levels in the project.</returns>
    public LdtkLevel[] GetAllLevels()
    {
        if (_currentProject is null)
        {
            _logger.Warn("No project loaded");
            return [];
        }
        
        return _currentProject.Levels;
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