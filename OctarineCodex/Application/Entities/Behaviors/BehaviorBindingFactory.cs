using System;

namespace OctarineCodex.Entities.Behaviors;

public static class BehaviorBindingFactory
{
    public static BehaviorBinding<T> Create<T>(Func<IServiceProvider, T>? factory = null)
        where T : IBehavior, new()
    {
        return new BehaviorBinding<T>(factory);
    }
}
