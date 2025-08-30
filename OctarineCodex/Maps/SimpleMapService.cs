using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LDtk;

namespace OctarineCodex.Maps;

/// <summary>
/// Simple implementation of ISimpleMapService for loading single LDtk levels.
/// </summary>
public sealed class SimpleMapService : ISimpleMapService
{
    private LDtkLevel? _currentLevel;

    public LDtkLevel? CurrentLevel => _currentLevel;

    public bool IsLevelLoaded => _currentLevel is not null;

    public async Task<LDtkLevel?> LoadLevelAsync(string filePath, string? levelIdentifier = null)
    {
        try
        {
            // Clear current level
            _currentLevel = null;

            // Check if file exists
            if (!File.Exists(filePath))
                return null;

            // Try loading with standard method first (for multiworld files)
            try
            {
                var file = await Task.Run(() => LDtkFile.FromFile(filePath));

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
                    {
                        _currentLevel = world.Levels.FirstOrDefault(level =>
                            string.Equals(level.Identifier, levelIdentifier, StringComparison.OrdinalIgnoreCase));
                    }
                    else
                    {
                        _currentLevel = world.Levels.FirstOrDefault();
                    }

                    return _currentLevel;
                }

                // Handle older format where levels are stored directly in the project
                if (file.Levels != null && file.Levels.Any())
                {
                    // In legacy format, levels are accessible directly from the file
                    // Find the specific level or get the first one
                    if (levelIdentifier != null)
                    {
                        _currentLevel = file.Levels.FirstOrDefault(level =>
                            string.Equals(level.Identifier, levelIdentifier, StringComparison.OrdinalIgnoreCase));
                    }
                    else
                    {
                        _currentLevel = file.Levels.FirstOrDefault();
                    }

                    return _currentLevel;
                }
            }
            catch (LDtk.LDtkException ex) when (ex.Message.Contains("multiworld"))
            {
                // If it fails because it's not a multiworld file, try loading without validation
                return await LoadSingleWorldFileAsync(filePath, levelIdentifier);
            }
            catch (Exception)
            {
                // For any other exception, try the fallback too
                return await LoadSingleWorldFileAsync(filePath, levelIdentifier);
            }

            return null;
        }
        catch
        {
            _currentLevel = null;
            return null;
        }
    }

    private async Task<LDtkLevel?> LoadSingleWorldFileAsync(string filePath, string? levelIdentifier)
    {
        try
        {
            // Load file content directly without validation
            var jsonContent = await File.ReadAllTextAsync(filePath);
            
            // Use the same serializer settings as the LDtk library but without validation
            var file = System.Text.Json.JsonSerializer.Deserialize<LDtk.LDtkFile>(jsonContent, LDtk.Constants.SerializeOptions);

            if (file == null) return null;

            // Try to access levels directly (legacy format)
            if (file.Levels != null && file.Levels.Any())
            {
                // Find the specific level or get the first one
                if (levelIdentifier != null)
                {
                    _currentLevel = file.Levels.FirstOrDefault(level =>
                        string.Equals(level.Identifier, levelIdentifier, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    _currentLevel = file.Levels.FirstOrDefault();
                }

                return _currentLevel;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}