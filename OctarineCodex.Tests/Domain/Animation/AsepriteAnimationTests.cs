// OctarineCodex.Tests/Domain/Animation/AsepriteAnimationTests.cs

using FluentAssertions;
using Microsoft.Xna.Framework;
using OctarineCodex.Domain.Animation;

namespace OctarineCodex.Tests.Domain.Animation;

public class AsepriteAnimationTests
{
    private const string SampleAsepriteJson = """
                                              {
                                                "frames": {
                                                  "still_right.0": {
                                                    "frame": { "x": 0, "y": 0, "w": 16, "h": 32 },
                                                    "rotated": false,
                                                    "trimmed": false,
                                                    "spriteSourceSize": { "x": 0, "y": 0, "w": 16, "h": 32 },
                                                    "sourceSize": { "w": 16, "h": 32 },
                                                    "duration": 100
                                                  },
                                                  "idle_right.0": {
                                                    "frame": { "x": 0, "y": 32, "w": 16, "h": 32 },
                                                    "rotated": false,
                                                    "trimmed": false,
                                                    "spriteSourceSize": { "x": 0, "y": 0, "w": 16, "h": 32 },
                                                    "sourceSize": { "w": 16, "h": 32 },
                                                    "duration": 150
                                                  },
                                                  "idle_right.1": {
                                                    "frame": { "x": 16, "y": 32, "w": 16, "h": 32 },
                                                    "rotated": false,
                                                    "trimmed": false,
                                                    "spriteSourceSize": { "x": 0, "y": 0, "w": 16, "h": 32 },
                                                    "sourceSize": { "w": 16, "h": 32 },
                                                    "duration": 150
                                                  }
                                                },
                                                "meta": {
                                                  "app": "https://www.aseprite.org/",
                                                  "version": "1.3.15.2-x64",
                                                  "format": "RGBA8888",
                                                  "size": { "w": 896, "h": 640 },
                                                  "scale": "1",
                                                  "frameTags": [
                                                    { "name": "still_right", "from": 0, "to": 0, "direction": "forward", "color": "#000000ff" },
                                                    { "name": "idle_right", "from": 1, "to": 2, "direction": "forward", "color": "#000000ff" }
                                                  ]
                                                }
                                              }
                                              """;

    [Fact]
    public void AsepriteAnimationData_ShouldParseJsonCorrectly()
    {
        // Arrange & Act
        AsepriteAnimationData animData = AsepriteAnimationData.FromJson(SampleAsepriteJson);

        // Assert
        animData.Should().NotBeNull();
        animData.Frames.Should().HaveCount(3);
        animData.Meta.FrameTags.Should().HaveCount(2);

        AsepriteFrame stillRightFrame = animData.Frames["still_right.0"];
        stillRightFrame.Frame.X.Should().Be(0);
        stillRightFrame.Frame.Y.Should().Be(0);
        stillRightFrame.Frame.W.Should().Be(16);
        stillRightFrame.Frame.H.Should().Be(32);
        stillRightFrame.Duration.Should().Be(100);
    }

    [Fact]
    public void AsepriteAnimationLoader_ShouldCreateAnimationsFromFrameTags()
    {
        // Arrange
        AsepriteAnimationData asepriteData = AsepriteAnimationData.FromJson(SampleAsepriteJson);

        // Act
        Dictionary<string, AsepriteAnimation> animations = AsepriteAnimationLoader.LoadAnimations(asepriteData);

        // Assert
        animations.Should().HaveCount(2);
        animations.Should().ContainKey("still_right");
        animations.Should().ContainKey("idle_right");

        AsepriteAnimation idleRightAnim = animations["idle_right"];
        idleRightAnim.Name.Should().Be("idle_right");
        idleRightAnim.Frames.Should().HaveCount(2);
        idleRightAnim.Loop.Should().BeTrue(); // Default for forward direction
    }

    [Fact]
    public void AsepriteAnimationComponent_ShouldPlayAnimationsCorrectly()
    {
        // Arrange
        AsepriteAnimationData asepriteData = AsepriteAnimationData.FromJson(SampleAsepriteJson);
        Dictionary<string, AsepriteAnimation> animations = AsepriteAnimationLoader.LoadAnimations(asepriteData);

        var component = new AsepriteAnimationComponent(animations, asepriteData);

        // Act
        component.PlayAnimation("idle_right");

        // Assert
        component.CurrentAnimation.Should().Be("idle_right");
        component.IsPlaying.Should().BeTrue();

        // First frame
        AsepriteFrame currentFrame = component.CurrentFrameData;
        currentFrame.Frame.X.Should().Be(0);
        currentFrame.Frame.Y.Should().Be(32);

        // Update to second frame (duration 150ms = 0.15s)
        component.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(150)));

        currentFrame = component.CurrentFrameData;
        currentFrame.Frame.X.Should().Be(16);
        currentFrame.Frame.Y.Should().Be(32);
    }

    [Fact]
    public void AsepriteAnimationComponent_ShouldLoopAnimationsCorrectly()
    {
        // Arrange
        AsepriteAnimationData asepriteData = AsepriteAnimationData.FromJson(SampleAsepriteJson);
        Dictionary<string, AsepriteAnimation> animations = AsepriteAnimationLoader.LoadAnimations(asepriteData);

        var component = new AsepriteAnimationComponent(animations, asepriteData);
        component.PlayAnimation("idle_right");

        // Act - Advance through complete animation cycle
        component.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(150))); // Frame 1
        component.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(150))); // Should loop back to Frame 0

        // Assert
        AsepriteFrame currentFrame = component.CurrentFrameData;
        currentFrame.Frame.X.Should().Be(0); // Back to first frame
        currentFrame.Frame.Y.Should().Be(32);
        component.IsComplete.Should().BeFalse(); // Looping animations are never complete
    }

    [Fact]
    public void AsepriteToLDtkBridge_ShouldConvertToCompatibleFormat()
    {
        // Arrange
        AsepriteAnimationData asepriteData = AsepriteAnimationData.FromJson(SampleAsepriteJson);
        Dictionary<string, AsepriteAnimation> animations = AsepriteAnimationLoader.LoadAnimations(asepriteData);

        // Act
        LDtkAnimationData ldtkAnimData = AsepriteToLDtkBridge.ConvertToLDtkFormat(animations["idle_right"]);

        // Assert
        ldtkAnimData.Name.Should().Be("idle_right");
        ldtkAnimData.Loop.Should().BeTrue();
        ldtkAnimData.Type.Should().Be(AnimationType.Simple);

        // Should calculate frame rate from average duration
        var expectedFrameRate = 1000f / 150f; // 150ms average duration
        ldtkAnimData.FrameRate.Should().BeApproximately(expectedFrameRate, 0.1f);
    }
}
