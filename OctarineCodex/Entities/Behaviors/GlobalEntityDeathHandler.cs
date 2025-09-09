// OctarineCodex/Entities/Behaviors/GlobalEntityDeathHandler.cs

using OctarineCodex.Logging;
using OctarineCodex.Messages;
using OctarineCodex.Messaging;

namespace OctarineCodex.Entities.Behaviors;

/// <summary>
///     Global handler for entity death events - manages loot drops, scoring, and game state.
/// </summary>
public class GlobalEntityDeathHandler(ILoggingService logger) : IMessageHandler<EntityDeathMessage>
{
    public void HandleMessage(EntityDeathMessage message, string? senderId = null)
    {
        logger.Debug($"Entity death detected from {senderId ?? "unknown"}");

        // Future implementations could:
        // - Drop loot based on entity type
        // - Update kill counters or scoring
        // - Trigger victory conditions
        // - Play death effects
        // - Update game statistics

        // For now, just log the event
        logger.Info($"Processing entity death for entity: {senderId}");
    }
}
