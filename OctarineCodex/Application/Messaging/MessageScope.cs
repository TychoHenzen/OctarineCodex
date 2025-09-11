namespace OctarineCodex.Application.Messaging;

/// <summary>
///     Defines the scope of message delivery.
/// </summary>
public enum MessageScope
{
    /// <summary>
    ///     Message stays within the sending entity (current behavior).
    /// </summary>
    Local,

    /// <summary>
    ///     Message can be sent to a specific target entity.
    /// </summary>
    Entity,

    /// <summary>
    ///     Message broadcast to all entities or global systems.
    /// </summary>
    Global,

    /// <summary>
    ///     Message sent to entities within a spatial range (for magic effects).
    /// </summary>
    Spatial
}
