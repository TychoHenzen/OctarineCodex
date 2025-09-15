namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Types of animations supported by the system.
/// </summary>
public enum AnimationType
{
    Simple, // Basic looping animation (torches, water)
    Triggered, // Event-driven animation (doors, spikes)
    StateMachine // Complex state-based animation (characters)
}
