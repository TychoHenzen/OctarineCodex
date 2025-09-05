using FluentAssertions;
using LDtk;
using Microsoft.Xna.Framework;
using OctarineCodex.Logging;
using OctarineCodex.Maps;

namespace OctarineCodex.Tests.Maps;

/// <summary>
///     Integration tests for the unified map system, replacing the old SimpleMapSystemIntegrationTests.
///     Tests the interaction between MapService and LevelRenderer.
/// </summary>
public class MapSystemIntegrationTests
{
    private readonly ILoggingService _logger;

    public MapSystemIntegrationTests()
    {
        _logger = new LoggingService();
    }

    [Fact]
    public async Task MapSystem_ShouldLoadAndConfigureRoom1Successfully()
    {
        // Arrange
        var mapService = new MapService(_logger);
        var renderer = new LevelRenderer(_logger);
        var filePath = Path.Combine("..", "..", "..", "..", "OctarineCodex", "Content", "Room1.ldtk");

        // Act - Load level
        var file = await Task.Run(() => LDtkFile.FromFile(filePath));
        var options = new MapLoadOptions { LoadAllLevels = false };
        var success = await mapService.LoadAsync(file, options);

        // Assert - Level loading
        success.Should().BeTrue();
        mapService.IsLoaded.Should().BeTrue();
        mapService.CurrentLevels.Should().HaveCount(1);

        var level = mapService.CurrentLevels[0];
        level.Should().NotBeNull();
        level.Identifier.Should().Be("AutoLayer");
        level.PxWid.Should().Be(296);
        level.PxHei.Should().Be(208);

        // Test renderer configuration
        renderer.Should().NotBeNull();

        // Verify that renderer methods exist and can be called (without GraphicsDevice they'll throw)
        var initializeAction = () => renderer.Initialize(null!);
        initializeAction.Should().Throw<ArgumentNullException>();

        var setContextAction = () => renderer.SetLDtkContext(file);
        setContextAction.Should().NotThrow();

        var loadTilesetsAction = async () => await renderer.LoadTilesetsAsync(null);
        await loadTilesetsAction.Should().ThrowAsync<InvalidOperationException>();

        // Verify depth-sorted rendering methods exist (updated with playerPosition parameter)
        var testPlayerPosition = Vector2.Zero;

        var renderBeforeAction = () =>
            renderer.RenderLevelsBeforePlayer(mapService.CurrentLevels, null!, null!, testPlayerPosition);
        renderBeforeAction.Should().NotThrow<ArgumentNullException>();

        var renderAfterAction = () =>
            renderer.RenderLevelsAfterPlayer(mapService.CurrentLevels, null!, null!, testPlayerPosition);
        renderAfterAction.Should().NotThrow<ArgumentNullException>();

        var renderForegroundAction = () =>
            renderer.RenderForegroundLayers(mapService.CurrentLevels, null!, null!, testPlayerPosition);
        renderForegroundAction.Should().NotThrow<ArgumentNullException>();
    }

    [Fact]
    public async Task MapService_ShouldHandleMultipleLoads()
    {
        // Arrange
        var mapService = new MapService(_logger);
        var filePath = Path.Combine("..", "..", "..", "..", "OctarineCodex", "Content", "Room1.ldtk");

        // Act - Load same level multiple times with different options
        var file = await Task.Run(() => LDtkFile.FromFile(filePath));

        var success1 = await mapService.LoadAsync(file, new MapLoadOptions { LoadAllLevels = false });
        var success2 = await mapService.LoadAsync(file, new MapLoadOptions { LoadAllLevels = true });
        var success3 = await mapService.LoadAsync(file, new MapLoadOptions { SpecificLevelIdentifier = "AutoLayer" });

        // Assert
        success1.Should().BeTrue();
        success2.Should().BeTrue();
        success3.Should().BeTrue();

        // Should maintain consistent state (last load overwrites)
        mapService.IsLoaded.Should().BeTrue();
        mapService.CurrentLevels.Should().NotBeEmpty();
        mapService.CurrentLevels[0].Identifier.Should().Be("AutoLayer");
    }

    [Fact]
    public void LevelRenderer_ShouldImplementIDisposable()
    {
        // Arrange
        var renderer = new LevelRenderer(_logger);

        // Act & Assert - Should not throw when disposed
        var disposeAction = () => renderer.Dispose();
        disposeAction.Should().NotThrow();

        // Should be safe to dispose multiple times
        disposeAction.Should().NotThrow();
    }

    [Fact]
    public async Task MapService_GetWorldBounds_ShouldCalculateCorrectBounds()
    {
        // Arrange
        var mapService = new MapService(_logger);
        var filePath = Path.Combine("..", "..", "..", "..", "OctarineCodex", "Content", "Room1.ldtk");
        var file = await Task.Run(() => LDtkFile.FromFile(filePath));

        // Act
        var success = await mapService.LoadAsync(file);
        var bounds = mapService.GetWorldBounds();

        // Assert
        success.Should().BeTrue();
        bounds.Width.Should().BeGreaterThan(0);
        bounds.Height.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task MapService_GetLevelAt_ShouldReturnCorrectLevel()
    {
        // Arrange
        var mapService = new MapService(_logger);
        var filePath = Path.Combine("..", "..", "..", "..", "OctarineCodex", "Content", "Room1.ldtk");
        var file = await Task.Run(() => LDtkFile.FromFile(filePath));

        // Act
        var success = await mapService.LoadAsync(file);
        var centerPoint = new Vector2(148, 104); // Roughly center of 296x208 level
        var level = mapService.GetLevelAt(centerPoint);

        // Assert
        success.Should().BeTrue();
        level.Should().NotBeNull();
        level!.Identifier.Should().Be("AutoLayer");
    }

    [Fact]
    public async Task LevelRenderer_DepthSortedRendering_ShouldAcceptPlayerPosition()
    {
        // Arrange
        var renderer = new LevelRenderer(_logger);
        var mapService = new MapService(_logger);
        var filePath = Path.Combine("..", "..", "..", "..", "OctarineCodex", "Content", "Room1.ldtk");
        var file = await Task.Run(() => LDtkFile.FromFile(filePath));

        var success = await mapService.LoadAsync(file);
        success.Should().BeTrue();

        renderer.SetLDtkContext(file);

        // Act & Assert - Test that all three rendering passes accept different player positions
        var testPositions = new[]
        {
            Vector2.Zero,
            new Vector2(100, 100),
            new Vector2(-50, 200)
        };

        foreach (var playerPos in testPositions)
        {
            // All rendering methods should handle various player positions without throwing
            var renderBeforeAction = () =>
                renderer.RenderLevelsBeforePlayer(mapService.CurrentLevels, null!, null!, playerPos);
            renderBeforeAction.Should()
                .NotThrow<ArgumentException>($"RenderLevelsBeforePlayer should handle player position {playerPos}");

            var renderAfterAction = () =>
                renderer.RenderLevelsAfterPlayer(mapService.CurrentLevels, null!, null!, playerPos);
            renderAfterAction.Should()
                .NotThrow<ArgumentException>($"RenderLevelsAfterPlayer should handle player position {playerPos}");

            var renderForegroundAction = () =>
                renderer.RenderForegroundLayers(mapService.CurrentLevels, null!, null!, playerPos);
            renderForegroundAction.Should()
                .NotThrow<ArgumentException>($"RenderForegroundLayers should handle player position {playerPos}");
        }
    }
}