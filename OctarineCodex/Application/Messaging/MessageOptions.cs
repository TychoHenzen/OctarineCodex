using Microsoft.Xna.Framework;

namespace OctarineCodex.Application.Messaging;

/// <summary>
///     Options for message delivery.
/// </summary>
public class MessageOptions
{
    /// <summary>
    ///     The scope of message delivery.
    /// </summary>
    public MessageScope Scope { get; set; } = MessageScope.Local;

    /// <summary>
    ///     Target entity ID for Entity scope messages.
    /// </summary>
    public string? TargetEntityId { get; set; }

    /// <summary>
    ///     Position for spatial scope messages.
    /// </summary>
    public Vector2? Position { get; set; }

    /// <summary>
    ///     Range for spatial scope messages.
    /// </summary>
    public float? Range { get; set; }

    /// <summary>
    ///     Whether to include the sender in spatial messages.
    /// </summary>
    public bool IncludeSender { get; set; } = false;

    /// <summary>
    ///     Whether to deliver immediately or queue for next frame.
    /// </summary>
    public bool Immediate { get; set; } = true;
}
