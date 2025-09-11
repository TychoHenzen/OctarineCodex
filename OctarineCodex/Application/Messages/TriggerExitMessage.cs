using OctarineCodex.Collisions;

namespace OctarineCodex.Messages;

public sealed class TriggerExitMessage(string triggerId, string entityId, CollisionLayers triggerLayers)
{
    public string TriggerId { get; } = triggerId;
    public string EntityId { get; } = entityId;
    public CollisionLayers TriggerLayers { get; } = triggerLayers;
}
