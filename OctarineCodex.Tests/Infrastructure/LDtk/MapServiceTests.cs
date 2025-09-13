using FluentAssertions;
using LDtk;
using OctarineCodex.Application.Maps;
using OctarineCodex.Infrastructure.LDtk;
using OctarineCodex.Infrastructure.Logging;

namespace OctarineCodex.Tests.Infrastructure.LDtk;

/// <summary>
///     Tests for the unified MapService, replacing the old SimpleMapService tests.
///     These tests verify single-level loading scenarios using the new unified API.
/// </summary>
public class MapServiceTests
{
    private readonly ILoggingService _logger;

    public MapServiceTests()
    {
        _logger = new LoggingService();
    }

    [Fact]
    public async Task LoadAsync_WithValidRoom1File_ShouldLoadFirstLevel()
    {
        // Arrange
        var mapService = new MapService(_logger);
        var filePath = Path.Combine("..", "..", "..", "..", "OctarineCodex", "Content", "Room1.ldtk");
        var file = await Task.Run(() => LDtkFile.FromFile(filePath));
        var options = new MapLoadOptions { LoadAllLevels = false }; // Load only first level

        // Act
        var success = mapService.Load(file, options);

        // Assert
        success.Should().BeTrue();
        mapService.IsLoaded.Should().BeTrue();
        mapService.CurrentLevels.Should().HaveCount(1);

        var level = mapService.CurrentLevels[0];
        level.Should().NotBeNull();
        level.Identifier.Should().Be("AutoLayer");
        level.PxWid.Should().Be(296);
        level.PxHei.Should().Be(208);
    }

    [Fact]
    public async Task LoadAsync_WithSpecificLevelIdentifier_ShouldLoadCorrectLevel()
    {
        // Arrange
        var mapService = new MapService(_logger);
        var filePath = Path.Combine("..", "..", "..", "..", "OctarineCodex", "Content", "Room1.ldtk");
        var file = await Task.Run(() => LDtkFile.FromFile(filePath));
        var options = new MapLoadOptions { SpecificLevelIdentifier = "AutoLayer" };

        // Act
        var success = mapService.Load(file, options);

        // Assert
        success.Should().BeTrue();
        mapService.IsLoaded.Should().BeTrue();
        mapService.CurrentLevels.Should().HaveCount(1);

        var level = mapService.CurrentLevels[0];
        level.Should().NotBeNull();
        level.Identifier.Should().Be("AutoLayer");
    }

    [Fact]
    public async Task LoadAsync_WithInvalidFile_ShouldReturnFalse()
    {
        // Arrange
        var mapService = new MapService(_logger);
        var filePath = "Content/NonExistent.ldtk";

        // Act
        var loadAction = async () =>
        {
            var file = await Task.Run(() => LDtkFile.FromFile(filePath));
            return mapService.Load(file);
        };

        // Assert
        await loadAction.Should().ThrowAsync<Exception>(); // File loading will throw before we get to LoadAsync
        mapService.IsLoaded.Should().BeFalse();
        mapService.CurrentLevels.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_WithInvalidLevelIdentifier_ShouldReturnFalse()
    {
        // Arrange
        var mapService = new MapService(_logger);
        var filePath = Path.Combine("..", "..", "..", "..", "OctarineCodex", "Content", "Room1.ldtk");
        var file = await Task.Run(() => LDtkFile.FromFile(filePath));
        var options = new MapLoadOptions { SpecificLevelIdentifier = "NonExistentLevel" };

        // Act
        var success = mapService.Load(file, options);

        // Assert
        success.Should().BeFalse();
        mapService.IsLoaded.Should().BeFalse();
        mapService.CurrentLevels.Should().BeEmpty();
    }

    [Fact]
    public void IsLoaded_WhenNoLevelLoaded_ShouldBeFalse()
    {
        // Arrange
        var mapService = new MapService(_logger);

        // Act & Assert
        mapService.IsLoaded.Should().BeFalse();
        mapService.CurrentLevels.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_LoadAllLevelsOption_ShouldLoadAllLevels()
    {
        // Arrange
        var mapService = new MapService(_logger);
        var filePath = Path.Combine("..", "..", "..", "..", "OctarineCodex", "Content", "Room1.ldtk");
        var file = await Task.Run(() => LDtkFile.FromFile(filePath));
        var options = new MapLoadOptions { LoadAllLevels = true };

        // Act
        var success = mapService.Load(file, options);

        // Assert
        success.Should().BeTrue();
        mapService.IsLoaded.Should().BeTrue();
        mapService.CurrentLevels.Should().NotBeEmpty();
    }
}
