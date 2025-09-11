using OctarineCodex.Application.Entities;

namespace OctarineCodex.Application.Messages;

public class InteractionMessage(EntityWrapper interactor, string? interactionType = null)
{
    public EntityWrapper Interactor { get; } = interactor;
    public string? InteractionType { get; } = interactionType;
}
