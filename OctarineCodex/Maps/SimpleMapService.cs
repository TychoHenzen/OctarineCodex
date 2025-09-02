using System;
using System.Linq;
using System.Threading.Tasks;
using LDtk;

namespace OctarineCodex.Maps;

/// <summary>
///     Simple implementation of ISimpleMapService for loading single LDtk levels.
/// </summary>
public class SimpleMapService : ISimpleMapService
{
    protected LDtkLevel? _currentLevel;

    public LDtkLevel? CurrentLevel => _currentLevel;

    public bool IsLevelLoaded => _currentLevel is not null;

    public async Task<LDtkLevel?> LoadLevelAsync(LDtkFile file, string? levelIdentifier = null)
    {
        try
        {
            // Clear current level
            _currentLevel = null;


            // Try to load from worlds first (newer format)
            if (file.Worlds.Any())
            {
                var firstWorld = file.Worlds.FirstOrDefault();
                if (firstWorld == null) return null;

                var world = file.LoadWorld(firstWorld.Iid);
                if (world?.Levels == null || !world.Levels.Any())
                    return null;

                // Find the specific level or get the first one
                if (levelIdentifier != null)
                    _currentLevel = world.Levels.FirstOrDefault(level =>
                        string.Equals(level.Identifier, levelIdentifier, StringComparison.OrdinalIgnoreCase));
                else
                    _currentLevel = world.Levels.FirstOrDefault();

                return _currentLevel;
            }

            // Handle older format where levels are stored directly in the project
            if (file.Levels != null && file.Levels.Any())
            {
                // In legacy format, levels are accessible directly from the file
                // Find the specific level or get the first one
                if (levelIdentifier != null)
                    _currentLevel = file.Levels.FirstOrDefault(level =>
                        string.Equals(level.Identifier, levelIdentifier, StringComparison.OrdinalIgnoreCase));
                else
                    _currentLevel = file.Levels.FirstOrDefault();

                return _currentLevel;
            }

            return null;
        }
        catch
        {
            _currentLevel = null;
            return null;
        }
    }

    public async Task<LDtkLevel?> LoadLevelAsync(string filePath, string? levelIdentifier = null)
    {
        var file = await Task.Run(() => LDtkFile.FromFile(filePath));
        return await LoadLevelAsync(file, levelIdentifier);
    }
}