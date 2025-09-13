// OctarineCodex.Tests/Domain/Animation/AnimationComponentTests.cs

using Microsoft.Xna.Framework;
using OctarineCodex.Domain.Animation;

namespace OctarineCodex.Tests.Domain.Animation;

public class AnimationComponentTests
{
    [Fact]
    public void SimpleAnimation_ShouldLoopCorrectly()
    {
        // Arrange
        var animData = LDtkAnimationData.CreateSimple("TestAnim", 100, 4, 10f);
        var component = new SimpleAnimationComponent();
        component.SetAnimation(animData);

        // Act & Assert - First loop
        Assert.Equal(100, component.GetCurrentTileId());

        component.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1f))); // 1 frame
        Assert.Equal(101, component.GetCurrentTileId());

        component.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1f))); // 2 frames
        Assert.Equal(102, component.GetCurrentTileId());

        component.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1f))); // 3 frames
        Assert.Equal(103, component.GetCurrentTileId());

        component.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1f))); // Should loop back
        Assert.Equal(100, component.GetCurrentTileId());
    }

    [Fact]
    public void LayeredAnimation_ShouldSynchronizeLayers()
    {
        // Arrange
        var controller = new LayeredAnimationController();

        var baseAnimations = new Dictionary<string, LDtkAnimationData>
        {
            ["Idle"] = LDtkAnimationData.CreateSimple("BaseIdle", 100, 2, 5f)
        };

        var armorAnimations = new Dictionary<string, LDtkAnimationData>
        {
            ["Idle"] = LDtkAnimationData.CreateSimple("ArmorIdle", 200, 2, 5f)
        };

        controller.AddLayer("Base", baseAnimations);
        controller.AddLayer("Armor", armorAnimations, 1);

        // Act
        controller.PlayAnimation("Idle");
        controller.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.2f))); // 1 frame

        // Assert
        LayerRenderData[] renderData = controller.GetLayerRenderData().ToArray();
        Assert.Equal(2, renderData.Length);
        Assert.Equal("Base", renderData[0].LayerName);
        Assert.Equal(101, renderData[0].TileId); // Second frame
        Assert.Equal("Armor", renderData[1].LayerName);
        Assert.Equal(201, renderData[1].TileId); // Second frame
    }
}
