using FluentAssertions;
using Microsoft.Xna.Framework;
using OctarineCodex.Domain.Components;

namespace OctarineCodex.Tests.Domain.Components;

public class PositionComponentTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultScale_WhenScaleIsZero()
    {
        // Act
        var position = new PositionComponent(Vector2.Zero);

        // Assert
        position.Scale.Should().Be(Vector2.One);
    }

    [Fact]
    public void Constructor_ShouldPreserveScale_WhenScaleProvided()
    {
        // Arrange
        var expectedScale = new Vector2(2f, 3f);

        // Act
        var position = new PositionComponent(Vector2.Zero, 0f, expectedScale);

        // Assert
        position.Scale.Should().Be(expectedScale);
    }

    [Fact]
    public void ToMatrix_ShouldCreateCorrectTransformation_WhenCalled()
    {
        // Arrange
        var position = new PositionComponent(
            new Vector2(10f, 20f),
            MathHelper.PiOver2,
            new Vector2(2f, 2f));

        // Act
        var matrix = position.ToMatrix();

        // Assert
        matrix.Should().NotBe(Matrix.Identity);
        matrix.Translation.X.Should().BeApproximately(10f, 0.001f);
        matrix.Translation.Y.Should().BeApproximately(20f, 0.001f);
    }

    [Fact]
    public void Forward_ShouldReturnCorrectDirection_ForZeroRotation()
    {
        // Arrange
        var position = new PositionComponent(Vector2.Zero);

        // Act
        Vector2 forward = position.Forward;

        // Assert
        forward.X.Should().BeApproximately(1f, 0.001f);
        forward.Y.Should().BeApproximately(0f, 0.001f);
    }

    [Fact]
    public void Right_ShouldReturnCorrectDirection_ForZeroRotation()
    {
        // Arrange
        var position = new PositionComponent(Vector2.Zero);

        // Act
        Vector2 right = position.Right;

        // Assert
        right.X.Should().BeApproximately(0f, 0.001f);
        right.Y.Should().BeApproximately(1f, 0.001f);
    }
}
