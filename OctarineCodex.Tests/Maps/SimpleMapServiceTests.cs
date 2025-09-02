using FluentAssertions;
using OctarineCodex.Maps;

namespace OctarineCodex.Tests.Maps;

public class SimpleMapServiceTests
{
    [Fact]
    public async Task LoadLevelAsync_WithValidRoom1File_ShouldLoadFirstLevel()
    {
        // Arrange
        var mapService = new SimpleMapService();
        var filePath = Path.Combine("..", "..", "..", "..", "OctarineCodex", "Content", "Room1.ldtk");

        // Act
        var level = await mapService.LoadLevelAsync(filePath);

        // Assert
        level.Should().NotBeNull();
        level!.Identifier.Should().Be("AutoLayer");
        level.PxWid.Should().Be(296);
        level.PxHei.Should().Be(208);
        mapService.IsLevelLoaded.Should().BeTrue();
        mapService.CurrentLevel.Should().Be(level);
    }

    [Fact]
    public async Task LoadLevelAsync_WithSpecificLevelIdentifier_ShouldLoadCorrectLevel()
    {
        // Arrange
        var mapService = new SimpleMapService();
        var filePath = Path.Combine("..", "..", "..", "..", "OctarineCodex", "Content", "Room1.ldtk");

        // Act
        var level = await mapService.LoadLevelAsync(filePath, "AutoLayer");

        // Assert
        level.Should().NotBeNull();
        level!.Identifier.Should().Be("AutoLayer");
        mapService.IsLevelLoaded.Should().BeTrue();
        mapService.CurrentLevel.Should().Be(level);
    }

    [Fact]
    public async Task LoadLevelAsync_WithInvalidFile_ShouldReturnNull()
    {
        // Arrange
        var mapService = new SimpleMapService();
        var filePath = "Content/NonExistent.ldtk";

        // Act
        var level = await mapService.LoadLevelAsync(filePath);

        // Assert
        level.Should().BeNull();
        mapService.IsLevelLoaded.Should().BeFalse();
        mapService.CurrentLevel.Should().BeNull();
    }

    [Fact]
    public async Task LoadLevelAsync_WithInvalidLevelIdentifier_ShouldReturnNull()
    {
        // Arrange
        var mapService = new SimpleMapService();
        var filePath = Path.Combine("..", "..", "..", "..", "OctarineCodex", "Content", "Room1.ldtk");

        // Act
        var level = await mapService.LoadLevelAsync(filePath, "NonExistentLevel");

        // Assert
        level.Should().BeNull();
        mapService.IsLevelLoaded.Should().BeFalse();
        mapService.CurrentLevel.Should().BeNull();
    }

    [Fact]
    public void IsLevelLoaded_WhenNoLevelLoaded_ShouldBeFalse()
    {
        // Arrange
        var mapService = new SimpleMapService();

        // Act & Assert
        mapService.IsLevelLoaded.Should().BeFalse();
        mapService.CurrentLevel.Should().BeNull();
    }
}