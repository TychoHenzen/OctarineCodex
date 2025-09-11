// OctarineCodex/Messaging/IMessageBus.cs

using OctarineCodex.Services;

namespace OctarineCodex.Messaging;

/// <summary>
///     Central message bus for inter-entity and global communication.
/// </summary>
[Service<MessageBus>]
public interface IMessageBus
{
    /// <summary>
    ///     Send a message with specified options.
    /// </summary>
    void SendMessage<T>(T message, MessageOptions? options = null, string? senderId = null)
        where T : class;

    /// <summary>
    ///     Register a global message handler for a specific message type.
    /// </summary>
    void RegisterHandler<T>(IMessageHandler<T> handler)
        where T : class;

    /// <summary>
    ///     Unregister a global message handler.
    /// </summary>
    void UnregisterHandler<T>(IMessageHandler<T> handler)
        where T : class;

    /// <summary>
    ///     Register an entity for message delivery.
    /// </summary>
    void RegisterEntity(string entityId, IMessageReceiver receiver);

    /// <summary>
    ///     Unregister an entity.
    /// </summary>
    void UnregisterEntity(string entityId);

    /// <summary>
    ///     Process any queued messages (called once per frame).
    /// </summary>
    void ProcessQueuedMessages();

    /// <summary>
    ///     Clear all handlers and entities (useful for level transitions).
    /// </summary>
    void Clear();
}
