using Microsoft.Xna.Framework;
using OctarineCodex.Application.Services;

namespace OctarineCodex.Application.Maps;

[Service<TeleportService>]
public interface ITeleportService
{
    void InitializeTeleports();
    bool IsTeleportAvailable(Vector2 playerPosition, out int targetWorldDepth, out Vector2? targetPosition);

    bool CheckTeleportInteraction(
        Vector2 playerPosition,
        bool inputPressed,
        out int targetWorldDepth,
        out Vector2? targetPosition);
}
