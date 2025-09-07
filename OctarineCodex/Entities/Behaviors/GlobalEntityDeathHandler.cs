// OctarineCodex/Entities/Behaviors/GlobalEntityDeathHandler.cs

using OctarineCodex.Entities.Messages;
using OctarineCodex.Logging;
using OctarineCodex.Messaging;

namespace OctarineCodex.Entities.Behaviors;

/// <summary>
///     Global handler for entity death events - manages loot drops, scoring, and game state
/// </summary>
public class GlobalEntityDeathHandler : IMessageHandler<EntityDeathMessage>
{
    private readonly ILoggingService _logger;

    public GlobalEntityDeathHandler(ILoggingService logger)
    {
        _logger = logger;
    }

    public void HandleMessage(EntityDeathMessage message, string? senderId = null)
    {
        _logger.Debug($"Entity death detected from {senderId ?? "unknown"}");

        // Future implementations could:
        // - Drop loot based on entity type
        // - Update kill counters or scoring
        // - Trigger victory conditions
        // - Play death effects
        // - Update game statistics

        // For now, just log the event
        _logger.Info($"Processing entity death for entity: {senderId}");
    }
}