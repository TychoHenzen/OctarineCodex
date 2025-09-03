using System;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Player;

/// <summary>
///     2D camera system for handling player following, room constraints, and viewport transformations.
/// </summary>
public class Camera2D
{
    /// <summary>
    ///     Initializes a new instance of the Camera2D class.
    /// </summary>
    /// <param name="viewportSize">The size of the viewport.</param>
    public Camera2D(Vector2 viewportSize)
    {
        ViewportSize = viewportSize;
        Position = Vector2.Zero;
    }

    /// <summary>
    ///     The current position of the camera in world coordinates.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    ///     The size of the viewport (screen dimensions).
    /// </summary>
    public Vector2 ViewportSize { get; }

    /// <summary>
    ///     Updates the camera position to follow the player while respecting room boundaries.
    /// </summary>
    /// <param name="player">The player to follow.</param>
    /// <param name="roomPosition">The position of the current room in world coordinates.</param>
    /// <param name="roomSize">The size of the current room.</param>
    public void FollowPlayer(PlayerControl player, Vector2 roomPosition, Vector2 roomSize)
    {
        FollowPlayer(player.Position, PlayerControl.Size, roomPosition, roomSize);
    }

    /// <summary>
    ///     Updates the camera position to follow the player while respecting room boundaries.
    /// </summary>
    /// <param name="playerPosition">The current position of the player.</param>
    /// <param name="playerSize">The size of the player sprite.</param>
    /// <param name="roomPosition">The position of the current room in world coordinates.</param>
    /// <param name="roomSize">The size of the current room.</param>
    public void FollowPlayer(Vector2 playerPosition, int playerSize, Vector2 roomPosition, Vector2 roomSize)
    {
        // Calculate player center
        var playerCenter = new Vector2(
            playerPosition.X + playerSize / 2f,
            playerPosition.Y + playerSize / 2f
        );

        // Calculate desired camera position (center camera on player)
        var desiredPosition = new Vector2(
            playerCenter.X - ViewportSize.X / 2f,
            playerCenter.Y - ViewportSize.Y / 2f
        );

        // Constrain camera to room bounds (prevent showing areas outside the level)
        // Handle edge case where room is smaller than viewport
        var maxCameraX = Math.Max(roomPosition.X, roomPosition.X + roomSize.X - ViewportSize.X);
        var maxCameraY = Math.Max(roomPosition.Y, roomPosition.Y + roomSize.Y - ViewportSize.Y);

        var constrainedPosition = new Vector2(
            MathHelper.Clamp(desiredPosition.X, roomPosition.X, maxCameraX),
            MathHelper.Clamp(desiredPosition.Y, roomPosition.Y, maxCameraY)
        );

        Position = constrainedPosition;
    }

    /// <summary>
    ///     Gets the transformation matrix for rendering with this camera.
    /// </summary>
    /// <returns>A transformation matrix that translates world coordinates to screen coordinates.</returns>
    public Matrix GetTransformMatrix()
    {
        return Matrix.CreateTranslation(-Position.X, -Position.Y, 0);
    }

    /// <summary>
    ///     Determines if the player is near the edge of the current room.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="roomPosition">The position of the current room in world coordinates.</param>
    /// <param name="roomSize">The size of the current room.</param>
    /// <param name="edgeThreshold">The distance from the edge to consider "near".</param>
    /// <returns>True if the player is near any room edge, false otherwise.</returns>
    public static bool IsPlayerNearRoomEdge(PlayerControl player, Vector2 roomPosition, Vector2 roomSize,
        float edgeThreshold)
    {
        return IsPlayerNearRoomEdge(player.Position, PlayerControl.Size, roomPosition, roomSize, edgeThreshold);
    }

    /// <summary>
    ///     Determines if the player is near the edge of the current room.
    /// </summary>
    /// <param name="playerPosition">The current position of the player.</param>
    /// <param name="playerSize">The size of the player sprite.</param>
    /// <param name="roomPosition">The position of the current room in world coordinates.</param>
    /// <param name="roomSize">The size of the current room.</param>
    /// <param name="edgeThreshold">The distance from the edge to consider "near".</param>
    /// <returns>True if the player is near any room edge, false otherwise.</returns>
    public static bool IsPlayerNearRoomEdge(Vector2 playerPosition, int playerSize, Vector2 roomPosition,
        Vector2 roomSize, float edgeThreshold)
    {
        var playerBounds = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, playerSize, playerSize);
        var roomBounds = new Rectangle((int)roomPosition.X, (int)roomPosition.Y, (int)roomSize.X, (int)roomSize.Y);

        // First check if player is actually inside the room
        if (!roomBounds.Contains(playerBounds))
            // Player is outside room bounds, not "near edge" in the intended sense
            return false;

        // Check if player is near left edge
        if (playerBounds.Left - roomBounds.Left <= edgeThreshold)
            return true;

        // Check if player is near right edge
        if (roomBounds.Right - playerBounds.Right <= edgeThreshold)
            return true;

        // Check if player is near top edge
        if (playerBounds.Top - roomBounds.Top <= edgeThreshold)
            return true;

        // Check if player is near bottom edge
        if (roomBounds.Bottom - playerBounds.Bottom <= edgeThreshold)
            return true;

        return false;
    }

    /// <summary>
    ///     Converts screen coordinates to world coordinates.
    /// </summary>
    /// <param name="screenPosition">Position in screen coordinates.</param>
    /// <returns>Position in world coordinates.</returns>
    public Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        return screenPosition + Position;
    }

    /// <summary>
    ///     Converts world coordinates to screen coordinates.
    /// </summary>
    /// <param name="worldPosition">Position in world coordinates.</param>
    /// <returns>Position in screen coordinates.</returns>
    public Vector2 WorldToScreen(Vector2 worldPosition)
    {
        return worldPosition - Position;
    }
}