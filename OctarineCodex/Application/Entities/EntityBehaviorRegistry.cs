// OctarineCodex/Entities/EntityBehaviorRegistry.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OctarineCodex.Application.Services;
using OctarineCodex.Domain.Entities;
using OctarineCodex.Infrastructure.Logging;

namespace OctarineCodex.Application.Entities;

[Service<EntityBehaviorRegistry>]
public class EntityBehaviorRegistry(IServiceProvider services, ILoggingService logger)
{
    private readonly Dictionary<string, bool> _applicabilityCache = new();
    private readonly List<BehaviorDescriptor> _behaviors = [];

    public void DiscoverBehaviors()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var behaviorTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
                        t.IsSubclassOf(typeof(EntityBehavior)) &&
                        t.GetCustomAttribute<EntityBehaviorAttribute>() != null)
            .ToArray();

        foreach (var behaviorType in behaviorTypes)
        {
            var attribute = behaviorType.GetCustomAttribute<EntityBehaviorAttribute>();
            var descriptor = new BehaviorDescriptor
            {
                BehaviorType = behaviorType,
                Attribute = attribute,
                Factory = () => (EntityBehavior)ActivatorUtilities.CreateInstance(services, behaviorType)
            };

            _behaviors.Add(descriptor);
            logger.Debug($"Registered behavior: {behaviorType.Name}");
        }

        // Sort by priority
        _behaviors.Sort((a, b) => b.Attribute.Priority.CompareTo(a.Attribute.Priority));
        logger.Info($"Discovered {_behaviors.Count} entity behaviors");
    }

    public void ApplyBehaviors(EntityWrapper entity)
    {
        foreach (var descriptor in _behaviors.Where(descriptor => ShouldApplyBehavior(descriptor, entity)))
        {
            try
            {
                var behavior = descriptor.Factory();
                entity.AddBehavior(behavior);
                logger.Debug($"Applied {descriptor.BehaviorType.Name} to {entity.EntityType}");
            }
            catch (Exception ex)
            {
                logger.Exception(ex, $"Failed to create behavior {descriptor.BehaviorType.Name}");
            }
        }
    }

    private bool ShouldApplyBehavior(BehaviorDescriptor descriptor, EntityWrapper entity)
    {
        // Fast path: check attribute filters first
        if (!string.IsNullOrEmpty(descriptor.Attribute.EntityType) &&
            !entity.EntityType.Equals(descriptor.Attribute.EntityType, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (descriptor.Attribute.RequiredFields.Length > 0 &&
            !descriptor.Attribute.RequiredFields.All(entity.HasField))
        {
            return false;
        }

        // Use cache if enabled
        if (descriptor.Attribute.CacheApplicability)
        {
            var cacheKey = $"{descriptor.BehaviorType.Name}:{entity.Uid}:{entity.EntityType}";
            if (_applicabilityCache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            var behavior = descriptor.Factory();
            var result = behavior.ShouldApplyTo(entity);
            _applicabilityCache[cacheKey] = result;
            return result;
        }

        // No cache - create temporary instance for check
        var tempBehavior = descriptor.Factory();
        return tempBehavior.ShouldApplyTo(entity);
    }

    private sealed class BehaviorDescriptor
    {
        public Type BehaviorType { get; set; }
        public EntityBehaviorAttribute Attribute { get; set; }
        public Func<EntityBehavior> Factory { get; set; }
    }
}
