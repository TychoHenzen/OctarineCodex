using Microsoft.Xna.Framework;

namespace OctarineCodex.Presentation.Camera;

/// <summary>
///     Service implementation for camera management.
/// </summary>
public class CameraService : ICameraService
{
    public CameraService()
    {
        Camera = new Camera2D(new Vector2(OctarineConstants.FixedWidth, OctarineConstants.FixedHeight) / OctarineConstants.WorldRenderScale);
    }

    public Camera2D Camera { get; }

    public void FollowTarget(Vector2 targetPosition, int targetSize, Vector2 roomPosition, Vector2 roomSize)
    {
        Camera.FollowPlayer(targetPosition, targetSize, roomPosition, roomSize);
    }

    public Matrix GetTransformMatrix()
    {
        return Camera.GetTransformMatrix();
    }

    public Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        return Camera.ScreenToWorld(screenPosition);
    }

    public Vector2 WorldToScreen(Vector2 worldPosition)
    {
        return Camera.WorldToScreen(worldPosition);
    }
}