using FluentAssertions;
using Microsoft.Xna.Framework;
using OctarineCodex;
using Xunit;

namespace OctarineCodex.Tests;

public class CameraTests
{
    [Fact]
    public void Camera2D_Constructor_InitializesCorrectly()
    {
        // Arrange
        var viewportSize = new Vector2(800, 600);
        
        // Act
        var camera = new Camera2D(viewportSize);
        
        // Assert
        camera.ViewportSize.Should().Be(viewportSize);
        camera.Position.Should().Be(Vector2.Zero);
    }

    [Fact]
    public void Camera2D_FollowPlayer_CentersOnPlayer()
    {
        // Arrange
        var viewportSize = new Vector2(800, 600);
        var camera = new Camera2D(viewportSize);
        var playerPosition = new Vector2(500, 400); // Player more centered in room
        var playerSize = 32;
        var roomSize = new Vector2(1200, 1000); // Larger than viewport
        
        // Act
        camera.FollowPlayer(playerPosition, playerSize, Vector2.Zero, roomSize);
        
        // Assert
        // Camera should center on player (player center - viewport center)
        var expectedCameraX = (playerPosition.X + playerSize / 2f) - (viewportSize.X / 2f);
        var expectedCameraY = (playerPosition.Y + playerSize / 2f) - (viewportSize.Y / 2f);
        camera.Position.X.Should().BeApproximately(expectedCameraX, 0.1f);
        camera.Position.Y.Should().BeApproximately(expectedCameraY, 0.1f);
    }

    [Fact]
    public void Camera2D_FollowPlayer_ConstrainedByRoomBounds_Left()
    {
        // Arrange
        var viewportSize = new Vector2(800, 600);
        var camera = new Camera2D(viewportSize);
        var playerPosition = new Vector2(50, 300); // Near left edge
        var playerSize = 32;
        var roomSize = new Vector2(1200, 1000);
        
        // Act
        camera.FollowPlayer(playerPosition, playerSize, Vector2.Zero, roomSize);
        
        // Assert
        // Camera should be constrained to not show outside room bounds
        camera.Position.X.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Camera2D_FollowPlayer_ConstrainedByRoomBounds_Right()
    {
        // Arrange
        var viewportSize = new Vector2(800, 600);
        var camera = new Camera2D(viewportSize);
        var playerPosition = new Vector2(1150, 300); // Near right edge
        var playerSize = 32;
        var roomSize = new Vector2(1200, 1000);
        
        // Act
        camera.FollowPlayer(playerPosition, playerSize, Vector2.Zero, roomSize);
        
        // Assert
        // Camera should be constrained to not show outside room bounds
        var maxCameraX = roomSize.X - viewportSize.X;
        camera.Position.X.Should().BeLessThanOrEqualTo(maxCameraX);
    }

    [Fact]
    public void Camera2D_FollowPlayer_SmallRoom_CentersRoomInViewport()
    {
        // Arrange
        var viewportSize = new Vector2(800, 600);
        var camera = new Camera2D(viewportSize);
        var playerPosition = new Vector2(200, 150);
        var playerSize = 32;
        var roomSize = new Vector2(400, 300); // Smaller than viewport
        
        // Act
        camera.FollowPlayer(playerPosition, playerSize, Vector2.Zero, roomSize);
        
        // Assert
        // Room should be centered in viewport
        var expectedCameraX = -(viewportSize.X - roomSize.X) / 2f;
        var expectedCameraY = -(viewportSize.Y - roomSize.Y) / 2f;
        camera.Position.X.Should().BeApproximately(expectedCameraX, 0.1f);
        camera.Position.Y.Should().BeApproximately(expectedCameraY, 0.1f);
    }

    [Fact]
    public void Camera2D_GetTransformMatrix_ReturnsCorrectMatrix()
    {
        // Arrange
        var viewportSize = new Vector2(800, 600);
        var camera = new Camera2D(viewportSize);
        camera.Position = new Vector2(100, 50);
        
        // Act
        var matrix = camera.GetTransformMatrix();
        
        // Assert
        // Transform matrix should translate by negative camera position
        var expectedMatrix = Matrix.CreateTranslation(-100, -50, 0);
        matrix.Should().Be(expectedMatrix);
    }

    [Theory]
    [InlineData(100, 150, 32, 200, 200, true)]  // Player near right edge (200-132=68 > 32, but 200-(100+32)=68, close to edge)
    [InlineData(300, 250, 32, 200, 200, false)] // Player outside room bounds
    [InlineData(20, 100, 32, 200, 200, true)]   // Player very close to left edge (20 <= 32)
    public void Camera2D_IsPlayerNearRoomEdge_DetectsCorrectly(
        float playerX, float playerY, int playerSize, 
        float roomWidth, float roomHeight, bool expectedNearEdge)
    {
        // Arrange
        var camera = new Camera2D(new Vector2(800, 600));
        var playerPosition = new Vector2(playerX, playerY);
        var roomSize = new Vector2(roomWidth, roomHeight);
        var edgeThreshold = 32; // One tile width
        
        // Act
        var isNearEdge = camera.IsPlayerNearRoomEdge(playerPosition, playerSize, Vector2.Zero, roomSize, edgeThreshold);
        
        // Assert
        isNearEdge.Should().Be(expectedNearEdge);
    }
}