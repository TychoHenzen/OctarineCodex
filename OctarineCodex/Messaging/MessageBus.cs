// OctarineCodex/Messaging/MessageBus.cs

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using OctarineCodex.Logging;

namespace OctarineCodex.Messaging;

/// <summary>
///     Central message bus implementation for inter-entity and global communication.
/// </summary>
public class MessageBus(ILoggingService logger) : IMessageBus
{
    private readonly Dictionary<Type, List<object>> _globalHandlers = new();
    private readonly ConcurrentQueue<QueuedMessage> _messageQueue = new();
    private readonly Dictionary<string, IMessageReceiver> _registeredEntities = new();

    public void SendMessage<T>(T message, MessageOptions? options = null, string? senderId = null)
        where T : class
    {
        options ??= new MessageOptions();

        if (options.Immediate)
        {
            DeliverMessage(message, options, senderId);
        }
        else
        {
            _messageQueue.Enqueue(new QueuedMessage(message, options, senderId));
        }
    }

    public void RegisterHandler<T>(IMessageHandler<T> handler)
        where T : class
    {
        Type messageType = typeof(T);
        if (!_globalHandlers.TryGetValue(messageType, out List<object>? value))
        {
            value = [];
            _globalHandlers[messageType] = value;
        }

        value.Add(handler);

        logger.Debug($"Registered global handler for {messageType.Name}");
    }

    public void UnregisterHandler<T>(IMessageHandler<T> handler)
        where T : class
    {
        var messageType = typeof(T);
        if (_globalHandlers.TryGetValue(messageType, out var handlers))
        {
            handlers.Remove(handler);
            if (handlers.Count == 0)
            {
                _globalHandlers.Remove(messageType);
            }
        }
    }

    public void RegisterEntity(string entityId, IMessageReceiver receiver)
    {
        _registeredEntities[entityId] = receiver;
        logger.Debug($"Registered entity {entityId} for messaging");
    }

    public void UnregisterEntity(string entityId)
    {
        if (_registeredEntities.Remove(entityId))
        {
            logger.Debug($"Unregistered entity {entityId}");
        }
    }

    public void ProcessQueuedMessages()
    {
        while (_messageQueue.TryDequeue(out QueuedMessage? queuedMessage))
        {
            DeliverMessage(queuedMessage.Message, queuedMessage.Options, queuedMessage.SenderId);
        }
    }

    public void Clear()
    {
        _globalHandlers.Clear();
        _registeredEntities.Clear();
        while (_messageQueue.TryDequeue(out _))
        {
            // only dequeue
        }

        logger.Debug("Cleared all message handlers and entities");
    }

    private void DeliverMessage<T>(T message, MessageOptions options, string? senderId)
        where T : class
    {
        switch (options.Scope)
        {
            case MessageScope.Local:
                // Local messages should not be sent anywhere
                break;

            case MessageScope.Entity:
                DeliverToTargetEntity(message, options.TargetEntityId, senderId);
                break;

            case MessageScope.Global:
                DeliverToGlobalHandlers(message, senderId);
                DeliverToAllEntities(message, senderId);
                break;

            case MessageScope.Spatial:
                DeliverToSpatialEntities(message, options, senderId);
                break;
        }
    }

    private void DeliverToTargetEntity<T>(T message, string? targetEntityId, string? senderId)
        where T : class
    {
        if (string.IsNullOrEmpty(targetEntityId))
        {
            logger.Warn("Entity scope message without target entity ID");
            return;
        }

        if (_registeredEntities.TryGetValue(targetEntityId, out var receiver))
        {
            receiver.ReceiveMessage(message, senderId);
        }
        else
        {
            logger.Debug($"Target entity {targetEntityId} not found for message {typeof(T).Name}");
        }
    }

    private void DeliverToGlobalHandlers<T>(T message, string? senderId)
        where T : class
    {
        var messageType = typeof(T);
        if (!_globalHandlers.TryGetValue(messageType, out var handlers))
        {
            return;
        }

        foreach (var handler in handlers.Cast<IMessageHandler<T>>())
        {
            try
            {
                handler.HandleMessage(message, senderId);
            }
            catch (Exception ex)
            {
                logger.Exception(ex, $"Error in global message handler for {messageType.Name}");
            }
        }
    }

    private void DeliverToAllEntities<T>(T message, string? senderId)
        where T : class
    {
        foreach (var entity in _registeredEntities.Values)
        {
            try
            {
                entity.ReceiveMessage(message, senderId);
            }
            catch (Exception ex)
            {
                logger.Exception(ex, $"Error delivering message {typeof(T).Name} to entity");
            }
        }
    }

    private void DeliverToSpatialEntities<T>(T message, MessageOptions options, string? senderId)
        where T : class
    {
        if (!options.Position.HasValue || !options.Range.HasValue)
        {
            logger.Warn("Spatial message without position or range");
            return;
        }

        var centerPosition = options.Position.Value;
        var range = options.Range.Value;
        var rangeSquared = range * range;
        var entitiesInRange = 0;

        foreach (var (entityId, receiver) in _registeredEntities)
        {
            // Skip sender unless explicitly included
            var distance = Vector2.DistanceSquared(receiver.Position, centerPosition);
            if ((!options.IncludeSender && entityId == senderId) || distance > rangeSquared)
            {
                continue;
            }

            try
            {
                receiver.ReceiveMessage(message, senderId);
                entitiesInRange++;
            }
            catch (Exception ex)
            {
                logger.Exception(ex, $"Error delivering spatial message {typeof(T).Name} to entity {entityId}");
            }
        }

        logger.Debug($"Delivered spatial message {typeof(T).Name} to {entitiesInRange} entities within range {range}");
    }

    private sealed class QueuedMessage(object message, MessageOptions options, string? senderId)
    {
        public object Message { get; } = message;
        public MessageOptions Options { get; } = options;
        public string? SenderId { get; } = senderId;
    }
}
