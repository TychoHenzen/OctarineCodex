using System;

namespace OctarineCodex.Entities;

[AttributeUsage(AttributeTargets.Class)]
public sealed class EntityBehaviorAttribute : Attribute
{
    /// <summary>
    ///     Optional: Restrict to specific entity type for performance optimization.
    /// </summary>
    public string EntityType { get; set; }

    /// <summary>
    ///     Optional: Require specific fields to exist for quick filtering.
    /// </summary>
    public string[] RequiredFields { get; set; } = [];

    /// <summary>
    ///     Priority for behavior application (higher = applied first).
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    ///     Whether this behavior should be cached for performance.
    /// </summary>
    public bool CacheApplicability { get; set; } = true;
}
