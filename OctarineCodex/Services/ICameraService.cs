using Microsoft.Xna.Framework;
using OctarineCodex.Player;

namespace OctarineCodex.Services;

/// <summary>
///     Service for managing camera operations that behaviors can interact with
/// </summary>
[Service<CameraService>]
public interface ICameraService
{
    /// <summary>
    ///     The current camera instance
    /// </summary>
    Camera2D Camera { get; }

    /// <summary>
    ///     Updates camera to follow a target position
    /// </summary>
    void FollowTarget(Vector2 targetPosition, int targetSize, Vector2 roomPosition, Vector2 roomSize);

    /// <summary>
    ///     Gets the camera's transformation matrix for rendering
    /// </summary>
    Matrix GetTransformMatrix();

    /// <summary>
    ///     Converts screen coordinates to world coordinates
    /// </summary>
    Vector2 ScreenToWorld(Vector2 screenPosition);

    /// <summary>
    ///     Converts world coordinates to screen coordinates
    /// </summary>
    Vector2 WorldToScreen(Vector2 worldPosition);
}