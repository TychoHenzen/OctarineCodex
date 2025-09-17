// OctarineCodex/Domain/Magic/AspectCalculator.cs (Updated key methods that reference MagicSignature)

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace OctarineCodex.Domain.Magic;

/// <summary>
///     Enhanced utility class for Element/Aspect conversion and 8D vector integration.
///     Provides high-performance mathematical operations for the magic system.
/// </summary>
public static class AspectCalculator
{
    /// <summary>
    ///     Converts a MagicSignature to its dominant Element/Aspect representation.
    ///     Uses the component with the highest absolute value.
    /// </summary>
    /// <param name="vector">The 8D magic vector</param>
    /// <returns>The dominant element and aspect with strength</returns>
    public static (EleAspects.Element element, EleAspects.Aspect aspect, float strength) GetDominantAspect(
        MagicSignature vector)
    {
        var maxElement = EleAspects.Element.Solidum;
        var maxValue = MathF.Abs(vector.GetComponent(EleAspects.Element.Solidum));

        // Find element with highest absolute value
        foreach (EleAspects.Element element in Enum.GetValues<EleAspects.Element>())
        {
            var value = MathF.Abs(vector.GetComponent(element));
            if (value > maxValue)
            {
                maxValue = value;
                maxElement = element;
            }
        }

        // Determine positive or negative aspect based on sign
        var componentValue = vector.GetComponent(maxElement);
        EleAspects.Aspect aspect = componentValue >= 0 ? maxElement.Positive() : maxElement.Negative();

        return (maxElement, aspect, maxValue);
    }

    /// <summary>
    ///     Creates a MagicSignature from a single Element/Aspect pair.
    /// </summary>
    /// <param name="element">The element axis</param>
    /// <param name="aspect">The aspect (positive/negative)</param>
    /// <param name="strength">The strength/magnitude (default: 1.0)</param>
    /// <returns>A magic vector with the specified component set</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MagicSignature CreateFromAspect(EleAspects.Element element, EleAspects.Aspect aspect,
        float strength = 1f)
    {
        // Validate that aspect belongs to element
        if (aspect.IsElement() != element)
        {
            throw new ArgumentException($"Aspect {aspect} does not belong to element {element}", nameof(aspect));
        }

        var values = new float[8];
        var sign = IsPositiveAspect(element, aspect) ? 1f : -1f;
        values[(int)element] = strength * sign;

        return new MagicSignature(values);
    }

    /// <summary>
    ///     Creates a balanced MagicSignature from multiple Element/Aspect pairs.
    /// </summary>
    /// <param name="aspects">Array of (element, aspect, strength) tuples</param>
    /// <returns>A magic vector with combined components</returns>
    public static MagicSignature CreateFromMultipleAspects(
        params (EleAspects.Element element, EleAspects.Aspect aspect, float strength)[] aspects)
    {
        var values = new float[8];

        foreach (var (element, aspect, strength) in aspects)
        {
            // Validate aspect belongs to element
            if (aspect.IsElement() != element)
            {
                throw new ArgumentException($"Aspect {aspect} does not belong to element {element}");
            }

            var sign = IsPositiveAspect(element, aspect) ? 1f : -1f;
            values[(int)element] += strength * sign;
        }

        return new MagicSignature(values);
    }

    /// <summary>
    ///     Calculates the aspect affinity between two magic vectors.
    ///     Returns a value from -1 (complete opposition) to 1 (perfect harmony).
    /// </summary>
    /// <param name="vector1">First magic vector</param>
    /// <param name="vector2">Second magic vector</param>
    /// <returns>Affinity value (-1 to 1)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CalculateAspectAffinity(MagicSignature vector1, MagicSignature vector2)
    {
        var dot = MagicSignature.Dot(vector1, vector2);
        var magnitude1 = vector1.Magnitude;
        var magnitude2 = vector2.Magnitude;

        if (magnitude1 < float.Epsilon || magnitude2 < float.Epsilon)
        {
            return 0f;
        }

        return dot / (magnitude1 * magnitude2);
    }

    /// <summary>
    ///     Calculates magical interaction strength between two vectors.
    ///     Uses both magnitude and directional alignment.
    /// </summary>
    /// <param name="caster">The casting magic vector</param>
    /// <param name="target">The target magic vector</param>
    /// <returns>Interaction strength (0 to positive values)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CalculateInteractionStrength(MagicSignature caster, MagicSignature target)
    {
        var affinity = CalculateAspectAffinity(caster, target);
        var combinedMagnitude = (caster.Magnitude + target.Magnitude) * 0.5f;

        // Positive affinity amplifies interaction, negative affinity reduces it
        var affinityMultiplier = MathF.Max(0f, (affinity + 1f) * 0.5f);

        return combinedMagnitude * affinityMultiplier;
    }

    /// <summary>
    ///     Resolves elemental opposition effects between two vectors.
    ///     Returns the resulting vector after opposition interactions.
    /// </summary>
    /// <param name="primary">Primary magic vector</param>
    /// <param name="secondary">Secondary magic vector</param>
    /// <returns>Resolved magic vector after opposition effects</returns>
    public static MagicSignature ResolveElementalOpposition(MagicSignature primary, MagicSignature secondary)
    {
        var result = new float[8];

        for (var i = 0; i < 8; i++)
        {
            var primaryValue = primary[i];
            var secondaryValue = secondary[i];

            // If signs are opposite, they partially cancel
            if (MathF.Sign(primaryValue) != MathF.Sign(secondaryValue) && primaryValue != 0 && secondaryValue != 0)
            {
                var cancelAmount =
                    MathF.Min(MathF.Abs(primaryValue), MathF.Abs(secondaryValue)) * 0.6f; // 60% cancellation
                var primarySign = MathF.Sign(primaryValue);
                var secondarySign = MathF.Sign(secondaryValue);

                var primaryRemaining = primarySign * MathF.Max(0f, MathF.Abs(primaryValue) - cancelAmount);
                var secondaryRemaining = secondarySign * MathF.Max(0f, MathF.Abs(secondaryValue) - cancelAmount);

                result[i] = primaryRemaining + (secondaryRemaining * 0.3f); // Secondary has less influence
            }
            else
            {
                // Same sign - they reinforce each other
                result[i] = primaryValue + (secondaryValue * 0.4f); // Secondary has moderate effect
            }
        }

        return new MagicSignature(result);
    }

    /// <summary>
    ///     Gets all aspects present in a magic vector above a threshold.
    /// </summary>
    /// <param name="vector">The magic vector to analyze</param>
    /// <param name="threshold">Minimum strength threshold (default: 0.1)</param>
    /// <returns>Array of aspects with their strengths</returns>
    public static (EleAspects.Aspect aspect, float strength)[] GetActiveAspects(MagicSignature vector,
        float threshold = 0.1f)
    {
        var activeAspects = new List<(EleAspects.Aspect, float)>();

        foreach (EleAspects.Element element in Enum.GetValues<EleAspects.Element>())
        {
            var value = vector.GetComponent(element);
            var absValue = MathF.Abs(value);

            if (absValue >= threshold)
            {
                EleAspects.Aspect aspect = value >= 0 ? element.Positive() : element.Negative();
                activeAspects.Add((aspect, absValue));
            }
        }

        return activeAspects.ToArray();
    }

    /// <summary>
    ///     Determines if an aspect is the positive aspect of its element.
    /// </summary>
    /// <param name="element">The element</param>
    /// <param name="aspect">The aspect to check</param>
    /// <returns>True if positive aspect, false if negative</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsPositiveAspect(EleAspects.Element element, EleAspects.Aspect aspect)
    {
        return element.Positive() == aspect;
    }
}
