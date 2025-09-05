// OctarineCodex/Entities/Behaviors/IBehavior.cs

using System;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Entities.Behaviors;

public interface IBehavior
{
    void Initialize(EntityWrapper entity, IServiceProvider services);
    void Update(GameTime gameTime);
    void OnMessage<T>(T message) where T : class;
    void Cleanup();
}

public interface IBehaviorBinding
{
    int Priority { get; }
    bool ShouldApply(EntityWrapper entity);
    IBehavior CreateBehavior(IServiceProvider services);
}