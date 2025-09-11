namespace OctarineCodex.Messaging;

/// <summary>
///     Strongly-typed message handler interface.
/// </summary>
public interface IMessageHandler<in T>
    where T : class
{
    /// <summary>
    ///     Handle a specific message type.
    /// </summary>
    void HandleMessage(T message, string? senderId = null);
}
