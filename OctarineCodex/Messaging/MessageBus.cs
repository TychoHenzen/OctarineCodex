// OctarineCodex/Messaging/MessageBus.cs

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using OctarineCodex.Logging;

namespace OctarineCodex.Messaging;

/// <summary>
///     Central message bus implementation for inter-entity and global communication
/// </summary>
public class MessageBus : IMessageBus
{
    private readonly Dictionary<Type, List<object>> _globalHandlers = new();
    private readonly ILoggingService _logger;
    private readonly ConcurrentQueue<QueuedMessage> _messageQueue = new();
    private readonly Dictionary<string, IMessageReceiver> _registeredEntities = new();

    public MessageBus(ILoggingService logger)
    {
        _logger = logger;
    }

    public void SendMessage<T>(T message, MessageOptions? options = null, string? senderId = null) where T : class
    {
        options ??= new MessageOptions();

        if (options.Immediate)
            DeliverMessage(message, options, senderId);
        else
            _messageQueue.Enqueue(new QueuedMessage(message, options, senderId));
    }

    public void RegisterHandler<T>(IMessageHandler<T> handler) where T : class
    {
        var messageType = typeof(T);
        if (!_globalHandlers.ContainsKey(messageType)) _globalHandlers[messageType] = new List<object>();
        _globalHandlers[messageType].Add(handler);

        _logger.Debug($"Registered global handler for {messageType.Name}");
    }

    public void UnregisterHandler<T>(IMessageHandler<T> handler) where T : class
    {
        var messageType = typeof(T);
        if (_globalHandlers.TryGetValue(messageType, out var handlers))
        {
            handlers.Remove(handler);
            if (handlers.Count == 0) _globalHandlers.Remove(messageType);
        }
    }

    public void RegisterEntity(string entityId, IMessageReceiver receiver)
    {
        _registeredEntities[entityId] = receiver;
        _logger.Debug($"Registered entity {entityId} for messaging");
    }

    public void UnregisterEntity(string entityId)
    {
        if (_registeredEntities.Remove(entityId)) _logger.Debug($"Unregistered entity {entityId}");
    }

    public void ProcessQueuedMessages()
    {
        while (_messageQueue.TryDequeue(out var queuedMessage))
            DeliverMessage(queuedMessage.Message, queuedMessage.Options, queuedMessage.SenderId);
    }

    public void Clear()
    {
        _globalHandlers.Clear();
        _registeredEntities.Clear();
        while (_messageQueue.TryDequeue(out _))
        {
        }

        _logger.Debug("Cleared all message handlers and entities");
    }

    private void DeliverMessage<T>(T message, MessageOptions options, string? senderId) where T : class
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

    private void DeliverToTargetEntity<T>(T message, string? targetEntityId, string? senderId) where T : class
    {
        if (string.IsNullOrEmpty(targetEntityId))
        {
            _logger.Warn("Entity scope message without target entity ID");
            return;
        }

        if (_registeredEntities.TryGetValue(targetEntityId, out var receiver))
            receiver.ReceiveMessage(message, senderId);
        else
            _logger.Debug($"Target entity {targetEntityId} not found for message {typeof(T).Name}");
    }

    private void DeliverToGlobalHandlers<T>(T message, string? senderId) where T : class
    {
        var messageType = typeof(T);
        if (_globalHandlers.TryGetValue(messageType, out var handlers))
            foreach (var handler in handlers.Cast<IMessageHandler<T>>())
                try
                {
                    handler.HandleMessage(message, senderId);
                }
                catch (Exception ex)
                {
                    _logger.Exception(ex, $"Error in global message handler for {messageType.Name}");
                }
    }

    private void DeliverToAllEntities<T>(T message, string? senderId) where T : class
    {
        foreach (var entity in _registeredEntities.Values)
            try
            {
                entity.ReceiveMessage(message, senderId);
            }
            catch (Exception ex)
            {
                _logger.Exception(ex, $"Error delivering message {typeof(T).Name} to entity");
            }
    }

    private void DeliverToSpatialEntities<T>(T message, MessageOptions options, string? senderId) where T : class
    {
        if (!options.Position.HasValue || !options.Range.HasValue)
        {
            _logger.Warn("Spatial message without position or range");
            return;
        }

        var centerPosition = options.Position.Value;
        var range = options.Range.Value;
        var rangeSquared = range * range;
        var entitiesInRange = 0;

        foreach (var (entityId, receiver) in _registeredEntities)
        {
            // Skip sender unless explicitly included
            if (!options.IncludeSender && entityId == senderId)
                continue;

            var distance = Vector2.DistanceSquared(receiver.Position, centerPosition);
            if (distance <= rangeSquared)
                try
                {
                    receiver.ReceiveMessage(message, senderId);
                    entitiesInRange++;
                }
                catch (Exception ex)
                {
                    _logger.Exception(ex, $"Error delivering spatial message {typeof(T).Name} to entity {entityId}");
                }
        }

        _logger.Debug($"Delivered spatial message {typeof(T).Name} to {entitiesInRange} entities within range {range}");
    }

    private class QueuedMessage
    {
        public QueuedMessage(object message, MessageOptions options, string? senderId)
        {
            Message = message;
            Options = options;
            SenderId = senderId;
        }

        public object Message { get; }
        public MessageOptions Options { get; }
        public string? SenderId { get; }
    }
}