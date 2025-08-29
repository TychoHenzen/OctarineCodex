using System.IO;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using OctarineCodex.Maps;

namespace OctarineCodex.Tests.Maps;

/// <summary>
/// Tests for LDTK file loading and debugging the rendering issue.
/// </summary>
public class LdtkLoadingTests
{
    [Fact]
    public async Task LoadProjectAsync_ShouldLoadTestLevel2Successfully()
    {
        // Arrange
        var mapService = new LdtkMonoGameMapService();
        var testFilePath = Path.Combine("..\\..\\..\\..\\OctarineCodex\\Content", "test_level2.ldtk");
        
        // Act
        var project = await mapService.LoadProjectAsync(testFilePath);
        
        // Assert
        project.Should().NotBeNull("test_level2.ldtk should load successfully");
        project!.Levels.Should().NotBeEmpty("project should contain levels");
        
        System.Console.WriteLine($"[DEBUG_LOG] Loaded project with {project.Levels.Length} levels");
        foreach (var level in project.Levels)
        {
            System.Console.WriteLine($"[DEBUG_LOG] Level: {level.Identifier} ({level.PixelWidth}x{level.PixelHeight}) with {level.LayerInstances.Length} layers");
            foreach (var layer in level.LayerInstances)
            {
                System.Console.WriteLine($"[DEBUG_LOG]   Layer: {layer.Identifier} (type: {layer.Type}, GridSize: {layer.GridSize}, TilesetDefUid: {layer.TilesetDefUid})");
                System.Console.WriteLine($"[DEBUG_LOG]     GridTiles: {layer.GridTiles.Length}, EntityInstances: {layer.EntityInstances.Length}, IntGridCsv: {layer.IntGridCsv.Length}");
            }
        }
        
        System.Console.WriteLine($"[DEBUG_LOG] Tilesets in project: {project.Definitions.Tilesets.Length}");
        foreach (var tileset in project.Definitions.Tilesets)
        {
            System.Console.WriteLine($"[DEBUG_LOG]   Tileset: {tileset.Identifier} (UID: {tileset.Uid}, {tileset.PixelWidth}x{tileset.PixelHeight}, GridSize: {tileset.TileGridSize})");
        }
    }
    
    [Fact]
    public async Task GetLevel_ShouldFindEntranceLevel()
    {
        // Arrange
        var mapService = new LdtkMonoGameMapService();
        var testFilePath = Path.Combine("..\\..\\..\\..\\OctarineCodex\\Content", "test_level2.ldtk");
        await mapService.LoadProjectAsync(testFilePath);
        
        // Act
        var level = mapService.GetLevel("Entrance");
        
        // Assert & Debug
        if (level != null)
        {
            System.Console.WriteLine($"[DEBUG_LOG] Found 'Entrance' level with {level.LayerInstances.Length} layers");
        }
        else
        {
            System.Console.WriteLine("[DEBUG_LOG] 'Entrance' level not found! Available levels:");
            var allLevels = mapService.GetAllLevels();
            foreach (var availableLevel in allLevels)
            {
                System.Console.WriteLine($"[DEBUG_LOG]   Available: {availableLevel.Identifier}");
            }
        }
    }
}