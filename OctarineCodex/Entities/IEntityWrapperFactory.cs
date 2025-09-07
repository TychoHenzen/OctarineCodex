// OctarineCodex/Entities/IEntityWrapperFactory.cs

using LDtk;

namespace OctarineCodex.Entities;

/// <summary>
///     Factory for creating EntityWrapper instances with proper dependency injection.
///     Abstracts the creation process to allow for easier testing and maintainability.
/// </summary>
[Service<EntityWrapperFactory>]
public interface IEntityWrapperFactory
{
    /// <summary>
    ///     Creates a new EntityWrapper instance for the specified entity.
    /// </summary>
    /// <param name="entity">The LDtk entity to wrap.</param>
    /// <returns>A new EntityWrapper instance with proper dependencies injected.</returns>
    EntityWrapper CreateWrapper(ILDtkEntity entity);
}