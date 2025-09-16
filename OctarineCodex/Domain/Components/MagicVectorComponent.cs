using System;

namespace OctarineCodex.Domain.Components;

/// <summary>
///     Placeholder magic vector component for Phase 1 ECS migration.
///     Will be fully implemented in Phase 3 with 8D magic vector system.
/// </summary>
public struct MagicVectorComponent
{
    /// <summary>
    ///     Indicates whether this entity has magic vector capabilities.
    ///     Will be replaced with MagicVector8 struct in Phase 3.
    /// </summary>
    public bool HasMagicVector;

    /// <summary>
    ///     Placeholder magic intensity value.
    ///     Will be replaced with proper vector magnitude in Phase 3.
    /// </summary>
    public float Intensity;

    /// <summary>
    ///     Creates a placeholder magic vector component.
    /// </summary>
    /// <param name="hasMagicVector">Whether entity has magic capabilities</param>
    /// <param name="intensity">Magic intensity (0.0 to 1.0)</param>
    public MagicVectorComponent(bool hasMagicVector = false, float intensity = 0f)
    {
        HasMagicVector = hasMagicVector;
        Intensity = Math.Clamp(intensity, 0f, 1f);
    }

    /// <summary>
    ///     Gets whether this entity is magically active.
    /// </summary>
    public readonly bool IsActive => HasMagicVector && Intensity > 0f;
}
