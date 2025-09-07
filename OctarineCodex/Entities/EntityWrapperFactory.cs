// OctarineCodex/Entities/EntityWrapperFactory.cs

using System;
using LDtk;
using OctarineCodex.Messaging;

namespace OctarineCodex.Entities;

/// <summary>
///     Factory implementation for creating EntityWrapper instances with proper dependency injection.
/// </summary>
public class EntityWrapperFactory : IEntityWrapperFactory
{
    private readonly IMessageBus _messageBus;

    /// <summary>
    ///     Initializes a new instance of the EntityWrapperFactory class.
    /// </summary>
    /// <param name="messageBus">The message bus for entity communication.</param>
    public EntityWrapperFactory(IMessageBus messageBus)
    {
        _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
    }

    /// <summary>
    ///     Creates a new EntityWrapper instance for the specified entity.
    /// </summary>
    /// <param name="entity">The LDtk entity to wrap.</param>
    /// <returns>A new EntityWrapper instance with proper dependencies injected.</returns>
    public EntityWrapper CreateWrapper(ILDtkEntity entity)
    {
        return new EntityWrapper(entity, _messageBus);
    }
}