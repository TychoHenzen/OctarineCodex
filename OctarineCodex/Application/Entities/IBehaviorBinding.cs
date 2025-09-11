using System;
using OctarineCodex.Domain.Entities;

namespace OctarineCodex.Application.Entities;

public interface IBehaviorBinding
{
    int Priority { get; }
    bool ShouldApply(EntityWrapper entity);
    IBehavior CreateBehavior(IServiceProvider services);
}
