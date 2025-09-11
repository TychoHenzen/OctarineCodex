using Microsoft.Xna.Framework;

namespace OctarineCodex.Messaging;

/// <summary>
///     Interface for entities that can receive messages.
/// </summary>
public interface IMessageReceiver
{
    /// <summary>
    ///     Get the position of this receiver for spatial messaging.
    /// </summary>
    Vector2 Position { get; }

    /// <summary>
    ///     Receive a message from the message bus.
    /// </summary>
    void ReceiveMessage<T>(T message, string? senderId = null)
        where T : class;
}
