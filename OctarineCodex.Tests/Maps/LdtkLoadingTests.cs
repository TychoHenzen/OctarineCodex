using System.IO;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using OctarineCodex.Logging;
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
        using var logger = new LoggingService();
        var mapService = new LdtkMonoGameMapService(logger);
        var testFilePath = Path.Combine("..\\..\\..\\..\\OctarineCodex\\Content", "test_level2.ldtk");
        
        // Act
        var project = await mapService.LoadProjectAsync(testFilePath);
        
        // Assert
        project.Should().NotBeNull("test_level2.ldtk should load successfully");
        project!.Levels.Should().NotBeEmpty("project should contain levels");
        project.Definitions.Tilesets.Should().NotBeEmpty("project should contain tilesets");
    }
    
    [Fact]
    public async Task GetLevel_ShouldFindEntranceLevel()
    {
        // Arrange
        using var logger = new LoggingService();
        var mapService = new LdtkMonoGameMapService(logger);
        var testFilePath = Path.Combine("..\\..\\..\\..\\OctarineCodex\\Content", "test_level2.ldtk");
        await mapService.LoadProjectAsync(testFilePath);
        
        // Act
        var level = mapService.GetLevel("Entrance");
        
        // Assert
        level.Should().NotBeNull("'Entrance' level should be found in the project");
        level!.LayerInstances.Should().NotBeEmpty("'Entrance' level should contain layers");
    }
}