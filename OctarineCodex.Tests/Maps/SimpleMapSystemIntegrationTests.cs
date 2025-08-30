using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Maps;
using Xunit;

namespace OctarineCodex.Tests.Maps;

public class SimpleMapSystemIntegrationTests
{
    [Fact]
    public async Task SimpleMapSystem_ShouldLoadAndRenderRoom1Successfully()
    {
        // Arrange
        var mapService = new SimpleMapService();
        var renderer = new SimpleLevelRenderer();
        var filePath = Path.Combine("..", "..", "..", "..", "OctarineCodex", "Content", "Room1.ldtk");

        // Act - Load level
        var level = await mapService.LoadLevelAsync(filePath);

        // Assert - Level loading
        level.Should().NotBeNull();
        level!.Identifier.Should().Be("AutoLayer");
        level.PxWid.Should().Be(296);
        level.PxHei.Should().Be(208);
        mapService.IsLevelLoaded.Should().BeTrue();
        mapService.CurrentLevel.Should().Be(level);

        // Note: We can't easily test the renderer without a real GraphicsDevice
        // The renderer initialization and rendering would require a MonoGame test framework
        // For now, we verify that the renderer can be created and configured
        renderer.Should().NotBeNull();

        // Verify that renderer methods exist and can be called (without GraphicsDevice they'll throw)
        var initializeAction = () => renderer.Initialize(null!);
        initializeAction.Should().Throw<ArgumentNullException>();

        var loadTilesetsAction = async () => await renderer.LoadTilesetsAsync(level, null);
        await loadTilesetsAction.Should().ThrowAsync<InvalidOperationException>();

        var renderAction = () => renderer.RenderLevelCentered(level, null!, Vector2.Zero);
        renderAction.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task SimpleMapService_ShouldHandleMultipleLoads()
    {
        // Arrange
        var mapService = new SimpleMapService();
        var filePath = Path.Combine("..", "..", "..", "..", "OctarineCodex", "Content", "Room1.ldtk");

        // Act - Load same level multiple times
        var level1 = await mapService.LoadLevelAsync(filePath);
        var level2 = await mapService.LoadLevelAsync(filePath);
        var level3 = await mapService.LoadLevelAsync(filePath, "AutoLayer");

        // Assert
        level1.Should().NotBeNull();
        level2.Should().NotBeNull();
        level3.Should().NotBeNull();
        
        level1!.Identifier.Should().Be("AutoLayer");
        level2!.Identifier.Should().Be("AutoLayer");
        level3!.Identifier.Should().Be("AutoLayer");
        
        // Should maintain consistent state
        mapService.IsLevelLoaded.Should().BeTrue();
        mapService.CurrentLevel.Should().Be(level3); // Last loaded level
    }

    [Fact]
    public void SimpleLevelRenderer_ShouldImplementIDisposable()
    {
        // Arrange
        var renderer = new SimpleLevelRenderer();

        // Act & Assert - Should not throw when disposed
        var disposeAction = () => renderer.Dispose();
        disposeAction.Should().NotThrow();
        
        // Should be safe to dispose multiple times
        disposeAction.Should().NotThrow();
    }
}