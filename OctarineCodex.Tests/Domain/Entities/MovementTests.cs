using Microsoft.Xna.Framework;

namespace OctarineCodex.Tests.Domain.Entities;

public class MovementTests
{
    [Fact]
    public void GetDirection_None_ReturnsZero()
    {
        var dir = Movement.GetDirection(false, false, false, false);
        dir.Should().Be(Vector2.Zero);
    }

    [Fact]
    public void GetDirection_Right_ReturnsUnitX()
    {
        var dir = Movement.GetDirection(false, false, false, true);
        dir.Should().Be(new Vector2(1f, 0f));
        dir.Length().Should().BeApproximately(1f, 1e-6f);
    }

    [Fact]
    public void GetDirection_UpLeft_ReturnsNormalizedDiagonal()
    {
        var dir = Movement.GetDirection(true, false, true, false);
        dir.Length().Should().BeApproximately(1f, 1e-6f);
        dir.X.Should().BeApproximately(-1f / (float)Math.Sqrt(2.0), 1e-6f);
        dir.Y.Should().BeApproximately(-1f / (float)Math.Sqrt(2.0), 1e-6f);
    }

    [Fact]
    public void ComputeDelta_ZeroDirection_ReturnsZero()
    {
        var delta = Movement.ComputeDelta(Vector2.Zero, 200f, 0.016f);
        delta.Should().Be(Vector2.Zero);
    }

    [Fact]
    public void ComputeDelta_NormalizesDirectionAndScalesBySpeedAndDt()
    {
        var dir = new Vector2(3f, 4f); // length 5
        var speed = 100f;
        var dt = 0.1f; // 100ms
        var delta = Movement.ComputeDelta(dir, speed, dt);

        // Normalized dir is (0.6, 0.8); scaled by 10 => (6, 8)
        delta.X.Should().BeApproximately(6f, 1e-6f);
        delta.Y.Should().BeApproximately(8f, 1e-6f);
        delta.Length().Should().BeApproximately(speed * dt, 1e-6f);
    }
}