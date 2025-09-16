using FluentAssertions;
using Microsoft.Xna.Framework;
using OctarineCodex.Domain.Components;

namespace OctarineCodex.Tests.Domain.Components;

public class RenderComponentTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues_WhenMinimalParametersProvided()
    {
        // Act
        var render = new RenderComponent("test_texture");

        // Assert
        render.TextureAssetName.Should().Be("test_texture");
        render.TintColor.Should().Be(Color.White);
        render.LayerDepth.Should().Be(0.5f);
        render.IsVisible.Should().BeTrue();
        render.SourceRectangle.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenTextureNameIsNull()
    {
        // Act & Assert
        Action act = () => new RenderComponent(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldClampLayerDepth_WhenOutOfRange()
    {
        // Act
        var renderTooLow = new RenderComponent("test", layerDepth: -1f);
        var renderTooHigh = new RenderComponent("test", layerDepth: 2f);

        // Assert
        renderTooLow.LayerDepth.Should().Be(0f);
        renderTooHigh.LayerDepth.Should().Be(1f);
    }

    [Fact]
    public void ForSpriteSheet_ShouldCreateCorrectSourceRectangle_WhenCalled()
    {
        // Act
        RenderComponent render = RenderComponent.ForSpriteSheet(
            "spritesheet",
            2,
            1,
            32,
            48);

        // Assert
        render.SourceRectangle.Should().NotBeNull();
        render.SourceRectangle!.Value.X.Should().Be(64); // 2 * 32
        render.SourceRectangle!.Value.Y.Should().Be(48); // 1 * 48
        render.SourceRectangle!.Value.Width.Should().Be(32);
        render.SourceRectangle!.Value.Height.Should().Be(48);
    }
}
