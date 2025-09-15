namespace OctarineCodex.Domain.Entities;

/// <summary>
///     Player animation states for proper state machine behavior.
/// </summary>
public enum PlayerAnimationState
{
    Idle, // No input, not moving
    Walk, // Input active, movement successful
    Push // Input active, movement blocked (pushing against wall)
}
