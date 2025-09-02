using Microsoft.Xna.Framework;

namespace OctarineCodex.Maps;

public interface ITeleportService
{
    void InitializeTeleports();
    bool IsTeleportAvailable(Vector2 playerPosition, out int targetWorldDepth, out Vector2? targetPosition);

    bool CheckTeleportInteraction(Vector2 playerPosition, bool inputPressed, out int targetWorldDepth,
        out Vector2? targetPosition);
}