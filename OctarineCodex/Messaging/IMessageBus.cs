// OctarineCodex/Messaging/IMessageBus.cs

using Microsoft.Xna.Framework;

namespace OctarineCodex.Messaging;

/// <summary>
///     Defines the scope of message delivery
/// </summary>
public enum MessageScope
{
    /// <summary>
    ///     Message stays within the sending entity (current behavior)
    /// </summary>
    Local,

    /// <summary>
    ///     Message can be sent to a specific target entity
    /// </summary>
    Entity,

    /// <summary>
    ///     Message broadcast to all entities or global systems
    /// </summary>
    Global,

    /// <summary>
    ///     Message sent to entities within a spatial range (for magic effects)
    /// </summary>
    Spatial
}

/// <summary>
///     Options for message delivery
/// </summary>
public class MessageOptions
{
    /// <summary>
    ///     The scope of message delivery
    /// </summary>
    public MessageScope Scope { get; set; } = MessageScope.Local;

    /// <summary>
    ///     Target entity ID for Entity scope messages
    /// </summary>
    public string? TargetEntityId { get; set; }

    /// <summary>
    ///     Position for spatial scope messages
    /// </summary>
    public Vector2? Position { get; set; }

    /// <summary>
    ///     Range for spatial scope messages
    /// </summary>
    public float? Range { get; set; }

    /// <summary>
    ///     Whether to include the sender in spatial messages
    /// </summary>
    public bool IncludeSender { get; set; } = false;

    /// <summary>
    ///     Whether to deliver immediately or queue for next frame
    /// </summary>
    public bool Immediate { get; set; } = true;
}

/// <summary>
///     Central message bus for inter-entity and global communication
/// </summary>
[Service<MessageBus>]
public interface IMessageBus
{
    /// <summary>
    ///     Send a message with specified options
    /// </summary>
    void SendMessage<T>(T message, MessageOptions? options = null, string? senderId = null) where T : class;

    /// <summary>
    ///     Register a global message handler for a specific message type
    /// </summary>
    void RegisterHandler<T>(IMessageHandler<T> handler) where T : class;

    /// <summary>
    ///     Unregister a global message handler
    /// </summary>
    void UnregisterHandler<T>(IMessageHandler<T> handler) where T : class;

    /// <summary>
    ///     Register an entity for message delivery
    /// </summary>
    void RegisterEntity(string entityId, IMessageReceiver receiver);

    /// <summary>
    ///     Unregister an entity
    /// </summary>
    void UnregisterEntity(string entityId);

    /// <summary>
    ///     Process any queued messages (called once per frame)
    /// </summary>
    void ProcessQueuedMessages();

    /// <summary>
    ///     Clear all handlers and entities (useful for level transitions)
    /// </summary>
    void Clear();
}

/// <summary>
///     Interface for entities that can receive messages
/// </summary>
public interface IMessageReceiver
{
    /// <summary>
    ///     Get the position of this receiver for spatial messaging
    /// </summary>
    Vector2 Position { get; }

    /// <summary>
    ///     Receive a message from the message bus
    /// </summary>
    void ReceiveMessage<T>(T message, string? senderId = null) where T : class;
}

/// <summary>
///     Strongly-typed message handler interface
/// </summary>
public interface IMessageHandler<in T> where T : class
{
    /// <summary>
    ///     Handle a specific message type
    /// </summary>
    void HandleMessage(T message, string? senderId = null);
}