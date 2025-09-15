// OctarineCodex/Entities/EntityWrapperFactory.cs

using LDtk;
using OctarineCodex.Application.Messaging;

namespace OctarineCodex.Application.Entities;

/// <summary>
///     Factory implementation for creating EntityWrapper instances with proper dependency injection.
/// </summary>
public class EntityWrapperFactory(IMessageBus messageBus) : IEntityWrapperFactory
{
    /// <summary>
    ///     Creates a new EntityWrapper instance for the specified entity.
    /// </summary>
    /// <param name="entity">The LDtk entity to wrap.</param>
    /// <returns>A new EntityWrapper instance with proper dependencies injected.</returns>
    public EntityWrapper CreateWrapper(ILDtkEntity entity)
    {
        return new EntityWrapper(entity, messageBus);
    }
}
