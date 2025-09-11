using OctarineCodex.Domain.Physics;

namespace OctarineCodex.Application.Messages;

public sealed class TriggerEnterMessage(string triggerId, string entityId, CollisionLayers triggerLayers)
{
    public string TriggerId { get; } = triggerId;
    public string EntityId { get; } = entityId;
    public CollisionLayers TriggerLayers { get; } = triggerLayers;
}
