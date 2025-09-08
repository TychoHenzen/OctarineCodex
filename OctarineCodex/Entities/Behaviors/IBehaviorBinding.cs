using System;

namespace OctarineCodex.Entities.Behaviors;

public interface IBehaviorBinding
{
    int Priority { get; }
    bool ShouldApply(EntityWrapper entity);
    IBehavior CreateBehavior(IServiceProvider services);
}