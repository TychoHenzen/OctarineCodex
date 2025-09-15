using FluentAssertions;
using LDtk;
using Microsoft.Xna.Framework;
using OctarineCodex.Application.Maps;
using OctarineCodex.Infrastructure.LDtk;
using OctarineCodex.Infrastructure.Logging;
using OctarineCodex.Presentation.Rendering;

namespace OctarineCodex.Tests.Integration;

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
        var renderer = new LevelRenderer(_logger, null!);
        var filePath = Path.Combine("..", "..", "..", "..", "OctarineCodex", "Content", "Room1.ldtk");

        // Act - Load level
        var file = await Task.Run(() => LDtkFile.FromFile(filePath));
        var options = new MapLoadOptions { LoadAllLevels = false };
        var success = mapService.Load(file, options);

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

        // Test LoadTilesets properly validates arguments
        // Note: Cannot easily mock ContentManager in tests due to MonoGame initialization requirements
        Action loadTilesetsAction = () => renderer.LoadTilesets(null!);
        loadTilesetsAction.Should().Throw<ArgumentNullException>()
            .WithParameterName("content");

        // Verify depth-sorted rendering methods exist and validate arguments
        var testPlayerPosition = Vector2.Zero;

        var renderBeforeAction = () =>
            renderer.RenderLevelsBeforePlayer(mapService.CurrentLevels, null!, testPlayerPosition);
        renderBeforeAction.Should().Throw<ArgumentNullException>()
            .WithParameterName("spriteBatch");

        var renderAfterAction = () =>
            renderer.RenderLevelsAfterPlayer(mapService.CurrentLevels, null!, testPlayerPosition);
        renderAfterAction.Should().Throw<ArgumentNullException>()
            .WithParameterName("spriteBatch");

        var renderForegroundAction = () =>
            renderer.RenderForegroundLayers(mapService.CurrentLevels, null!, testPlayerPosition);
        renderForegroundAction.Should().Throw<ArgumentNullException>()
            .WithParameterName("spriteBatch");
    }

    [Fact]
    public async Task MapService_ShouldHandleMultipleLoads()
    {
        // Arrange
        var mapService = new MapService(_logger);
        var filePath = Path.Combine("..", "..", "..", "..", "OctarineCodex", "Content", "Room1.ldtk");

        // Act - Load same level multiple times with different options
        var file = await Task.Run(() => LDtkFile.FromFile(filePath));

        var success1 = mapService.Load(file, new MapLoadOptions { LoadAllLevels = false });
        var success2 = mapService.Load(file, new MapLoadOptions { LoadAllLevels = true });
        var success3 = mapService.Load(file, new MapLoadOptions { SpecificLevelIdentifier = "AutoLayer" });

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
        var renderer = new LevelRenderer(_logger, null!);

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
        var success = mapService.Load(file);
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
        var success = mapService.Load(file);
        var centerPoint = new Vector2(148, 104); // Roughly center of 296x208 level
        var level = mapService.GetLevelAt(centerPoint);

        // Assert
        success.Should().BeTrue();
        level.Should().NotBeNull();
        level!.Identifier.Should().Be("AutoLayer");
    }

    [Fact]
    public async Task LevelRenderer_DepthSortedRendering_ShouldValidateArguments()
    {
        // Arrange
        var renderer = new LevelRenderer(_logger, null!);
        var mapService = new MapService(_logger);
        var filePath = Path.Combine("..", "..", "..", "..", "OctarineCodex", "Content", "Room1.ldtk");
        var file = await Task.Run(() => LDtkFile.FromFile(filePath));

        var success = mapService.Load(file);
        success.Should().BeTrue();

        renderer.SetLDtkContext(file);

        // Act & Assert - Test that rendering methods properly validate required arguments
        // Note: Testing actual graphics rendering requires full MonoGame initialization,
        // so we focus on argument validation which can be tested in unit tests

        Vector2[] testPositions = new[] { Vector2.Zero, new Vector2(100, 100), new Vector2(-50, 200) };

        foreach (var playerPos in testPositions)
        {
            // All rendering methods should validate required arguments regardless of player position
            var renderBeforeAction = () =>
                renderer.RenderLevelsBeforePlayer(mapService.CurrentLevels, null!, playerPos);
            renderBeforeAction.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("spriteBatch");

            var renderAfterAction = () =>
                renderer.RenderLevelsAfterPlayer(mapService.CurrentLevels, null!, playerPos);
            renderAfterAction.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("spriteBatch");

            var renderForegroundAction = () =>
                renderer.RenderForegroundLayers(mapService.CurrentLevels, null!, playerPos);
            renderForegroundAction.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("spriteBatch");
        }
    }
}
