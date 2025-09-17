// OctarineCodex/Domain/Components/MagicVectorComponent.cs

using System;
using OctarineCodex.Domain.Magic;

namespace OctarineCodex.Domain.Components;

/// <summary>
///     ECS component containing an entity's magical properties and capabilities.
///     Stores the entity's 8D magic signature, intensity, and state information.
/// </summary>
public struct MagicVectorComponent
{
    /// <summary>
    ///     The entity's 8-dimensional magic signature representing all elemental affinities.
    /// </summary>
    public MagicSignature Vector;

    /// <summary>
    ///     Overall magical intensity multiplier (0.0 to positive values).
    ///     Applied to all magical calculations involving this entity.
    /// </summary>
    public float Intensity;

    /// <summary>
    ///     Whether the entity's magic is currently active and can participate in interactions.
    /// </summary>
    public bool IsActive;

    /// <summary>
    ///     Base magic signature for restoration after temporary modifications.
    ///     Allows magic effects to be reversed or restored to original state.
    /// </summary>
    public MagicSignature BaseVector;

    /// <summary>
    ///     Timestamp of when the magic vector was last modified.
    ///     Useful for tracking magic effect durations and decay.
    /// </summary>
    public double LastModifiedTime;

    /// <summary>
    ///     Creates a new magic vector component with specified properties.
    /// </summary>
    /// <param name="vector">The initial magic signature</param>
    /// <param name="intensity">Magic intensity multiplier (default: 1.0)</param>
    /// <param name="isActive">Whether magic is active (default: true)</param>
    /// <param name="baseVector">Base vector for restoration (defaults to vector if not specified)</param>
    public MagicVectorComponent(MagicSignature vector, float intensity = 1f, bool isActive = true,
        MagicSignature? baseVector = null)
    {
        Vector = vector;
        Intensity = intensity;
        IsActive = isActive;
        BaseVector = baseVector ?? vector;
        LastModifiedTime = 0; // Will be set by system when component is created
    }

    /// <summary>
    ///     Creates a magic vector component from a dominant aspect.
    /// </summary>
    /// <param name="element">The primary element</param>
    /// <param name="aspect">The primary aspect</param>
    /// <param name="strength">The strength of the aspect (default: 1.0)</param>
    /// <param name="intensity">Overall intensity multiplier (default: 1.0)</param>
    /// <returns>A new magic vector component</returns>
    public static MagicVectorComponent FromAspect(EleAspects.Element element, EleAspects.Aspect aspect,
        float strength = 1f, float intensity = 1f)
    {
        MagicSignature vector = AspectCalculator.CreateFromAspect(element, aspect, strength);
        return new MagicVectorComponent(vector, intensity, true, vector);
    }

    /// <summary>
    ///     Creates a magic vector component from multiple aspects.
    /// </summary>
    /// <param name="aspects">Array of (element, aspect, strength) tuples</param>
    /// <param name="intensity">Overall intensity multiplier (default: 1.0)</param>
    /// <returns>A new magic vector component</returns>
    public static MagicVectorComponent FromMultipleAspects(
        (EleAspects.Element element, EleAspects.Aspect aspect, float strength)[] aspects,
        float intensity = 1f)
    {
        MagicSignature vector = AspectCalculator.CreateFromMultipleAspects(aspects);
        return new MagicVectorComponent(vector, intensity, true, vector);
    }

    /// <summary>
    ///     Gets the effective magic signature (Vector * Intensity) if active, otherwise zero.
    /// </summary>
    public readonly MagicSignature EffectiveVector => IsActive ? Vector * Intensity : MagicSignature.Zero;

    /// <summary>
    ///     Gets the dominant aspect of this entity's magic signature.
    /// </summary>
    public readonly (EleAspects.Element element, EleAspects.Aspect aspect, float strength) DominantAspect =>
        AspectCalculator.GetDominantAspect(Vector);

    /// <summary>
    ///     Gets all active aspects above the specified threshold.
    /// </summary>
    /// <param name="threshold">Minimum strength threshold (default: 0.1)</param>
    /// <returns>Array of active aspects with their strengths</returns>
    public readonly (EleAspects.Aspect aspect, float strength)[] GetActiveAspects(float threshold = 0.1f)
    {
        return AspectCalculator.GetActiveAspects(Vector, threshold);
    }

    /// <summary>
    ///     Restores the magic signature to its base state.
    /// </summary>
    public void RestoreToBase()
    {
        Vector = BaseVector;
    }

    /// <summary>
    ///     Modifies the magic signature temporarily, keeping the base vector unchanged.
    /// </summary>
    /// <param name="modification">The modification vector to add</param>
    public void ModifyTemporary(MagicSignature modification)
    {
        Vector = Vector + modification;
        LastModifiedTime = DateTime.UtcNow.Ticks / 10000000.0; // Convert to seconds
    }

    /// <summary>
    ///     Sets a new base signature (permanent change).
    /// </summary>
    /// <param name="newBase">The new base signature</param>
    public void SetBaseVector(MagicSignature newBase)
    {
        BaseVector = newBase;
        Vector = newBase;
        LastModifiedTime = DateTime.UtcNow.Ticks / 10000000.0;
    }

    /// <summary>
    ///     Gets whether this entity has significant magical presence.
    /// </summary>
    public readonly bool HasMagicalPresence => IsActive && Vector.Magnitude > 0.01f && Intensity > 0.01f;

    /// <summary>
    ///     Calculates magical affinity with another magic component.
    /// </summary>
    /// <param name="other">The other magic component</param>
    /// <returns>Affinity value (-1 to 1)</returns>
    public readonly float CalculateAffinityWith(MagicVectorComponent other)
    {
        if (!IsActive || !other.IsActive)
        {
            return 0f;
        }

        return AspectCalculator.CalculateAspectAffinity(EffectiveVector, other.EffectiveVector);
    }
}
