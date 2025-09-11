using OctarineCodex.Domain.Physics;

namespace OctarineCodex.Application.Messages;

public sealed class TriggerExitMessage(string triggerId, string entityId, CollisionLayers triggerLayers)
{
    public string TriggerId { get; } = triggerId;
    public string EntityId { get; } = entityId;
    public CollisionLayers TriggerLayers { get; } = triggerLayers;
}
