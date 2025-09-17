// OctarineCodex/Application/Services/MagicCalculator.cs

using System;
using System.Collections.Generic;
using OctarineCodex.Domain.Magic;
using OctarineCodex.Infrastructure.Logging;

namespace OctarineCodex.Application.Services;

/// <summary>
///     Core implementation of magical calculations using the 8D vector system.
///     Handles all high-level magical interactions, damage calculations, and spell effectiveness.
/// </summary>
[Service<IMagicCalculator>]
public class MagicCalculator : IMagicCalculator
{
    private readonly ILoggingService _logger;

    /// <summary>
    ///     Initializes the magic calculator service.
    /// </summary>
    public MagicCalculator(ILoggingService logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Calculates the magical interaction result between a caster and target.
    ///     Uses vector projection and elemental resonance principles.
    /// </summary>
    public MagicSignature CalculateInteraction(MagicSignature caster, MagicSignature target)
    {
        _logger.Debug(
            $"Calculating magic interaction - Caster: {caster.ToElementString()}, Target: {target.ToElementString()}");

        // Calculate the interaction using resonance and opposition
        MagicSignature resonanceVector = CalculateResonance(caster, target);
        MagicSignature oppositionResult = AspectCalculator.ResolveElementalOpposition(caster, target);

        // For opposing vectors, ensure some interaction remains
        var affinity = AspectCalculator.CalculateAspectAffinity(caster, target);
        MagicSignature interactionResult;

        if (affinity >= 0)
        {
            // Compatible vectors - combine effects
            interactionResult = (resonanceVector + oppositionResult) * 0.5f;
        }
        else
        {
            // Opposing vectors - weaken but don't eliminate
            MagicSignature weakenedCaster = caster * (1f + (affinity * 0.3f)); // Reduce by up to 30%
            MagicSignature weakenedTarget = target * (1f + (affinity * 0.3f));
            interactionResult = (weakenedCaster + weakenedTarget) * 0.3f; // Weaker overall interaction
        }

        // Apply interaction strength modifier but ensure non-zero result for opposing vectors
        var strength = AspectCalculator.CalculateInteractionStrength(caster, target);
        var strengthMultiplier = affinity >= 0
            ? strength / (strength + 1f)
            : // Normal normalization for compatible
            MathF.Max(0.1f, strength / (strength + 2f)); // Ensure minimum for opposing

        MagicSignature finalResult = interactionResult * strengthMultiplier;

        _logger.Debug($"Magic interaction result: {finalResult.ToElementString()} (Strength: {strength:F2})");

        return finalResult;
    }

    /// <summary>
    ///     Resolves aspects for a given element and primary aspect.
    /// </summary>
    public AspectResult ResolveAspects(EleAspects.Element element, EleAspects.Aspect primaryAspect)
    {
        // Validate that the aspect belongs to the element
        if (primaryAspect.IsElement() != element)
        {
            _logger.Warn($"Aspect resolution mismatch - Element: {element}, Aspect: {primaryAspect}");

            return new AspectResult(
                element.Positive(), // Default to positive aspect
                Array.Empty<EleAspects.Aspect>(),
                0f,
                false
            );
        }

        // Calculate secondary aspects that might emerge from neighboring elements
        EleAspects.Aspect[] secondaryAspects = CalculateSecondaryAspects(element, primaryAspect);

        // Determine intensity based on aspect purity and stability
        var intensity = CalculateAspectIntensity(element, primaryAspect);
        var isStable = intensity > 0.5f && secondaryAspects.Length <= 2;

        _logger.Debug(
            $"Aspect resolution - Element: {element}, Primary: {primaryAspect}, Intensity: {intensity:F2}, Stable: {isStable}");

        return new AspectResult(
            primaryAspect,
            secondaryAspects,
            intensity,
            isStable
        );
    }

    /// <summary>
    ///     Calculates damage or healing based on magical interaction.
    /// </summary>
    public float CalculateMagicalDamage(MagicSignature attackVector, MagicSignature defenseVector, float baseDamage)
    {
        var affinity = AspectCalculator.CalculateAspectAffinity(attackVector, defenseVector);
        var interactionStrength = AspectCalculator.CalculateInteractionStrength(attackVector, defenseVector);

        // Calculate base amplification from affinity
        var affinityModifier = 1f + (affinity * 0.5f); // 0.5x to 1.5x based on affinity
        // Calculate strength bonus based on magical complexity (number of active elements)
        var attackComplexity = CalculateVectorComplexity(attackVector);
        var defenseComplexity = CalculateVectorComplexity(defenseVector);
        var complexityBonus = (attackComplexity + defenseComplexity) * 0.25f; // Up to 2x bonus for full 8D vectors
        var strengthModifier = 1f + MathF.Min(complexityBonus, interactionStrength / 3f);

        // For opposing vectors (negative affinity), reduce damage
        if (affinity < 0)
        {
            affinityModifier = MathF.Max(0.5f, 1f + (affinity * 0.5f)); // Minimum 0.5x damage
        }

        // Apply multiplicative scaling
        var finalDamage = baseDamage * affinityModifier * strengthModifier;

        // Apply smart bounds based on vector complexity
        var maxAmplification = attackComplexity > 1.5f ? 3f : 2f; // Higher cap for multi-element vectors
        var maxDamage = baseDamage * maxAmplification;
        finalDamage = MathF.Min(finalDamage, maxDamage);

        _logger.Debug(
            $"Magical damage calculation - Base: {baseDamage}, Affinity: {affinityModifier:F2}, Strength: {strengthModifier:F2}, Complexity: {attackComplexity:F2}, Final: {finalDamage:F2}");

        return finalDamage;
    }

    /// <summary>
    ///     Determines spell effectiveness based on caster affinity and environmental factors.
    /// </summary>
    public float CalculateSpellEffectiveness(MagicSignature casterVector, MagicSignature spellVector,
        MagicSignature environmentalVector)
    {
        var casterAffinity = AspectCalculator.CalculateAspectAffinity(casterVector, spellVector);
        var environmentalAffinity = AspectCalculator.CalculateAspectAffinity(spellVector, environmentalVector);

        // Base effectiveness from caster-spell affinity
        var baseEffectiveness = MathF.Max(0.1f, (casterAffinity + 1f) * 0.5f); // 0.1 to 1.0

        // Environmental bonus/penalty
        var environmentalModifier = 1f + (environmentalAffinity * 0.3f); // ±30% from environment

        var finalEffectiveness = baseEffectiveness * environmentalModifier;

        _logger.Debug(
            $"Spell effectiveness - Caster Affinity: {casterAffinity:F2}, Env Modifier: {environmentalModifier:F2}, Final: {finalEffectiveness:F2}");

        return MathF.Max(0f, finalEffectiveness);
    }

    /// <summary>
    ///     Calculates magical resonance for area-of-effect spells.
    /// </summary>
    public MagicSignature[] CalculateAreaResonance(MagicSignature epicenterVector, MagicSignature[] targetVectors,
        float[] distances)
    {
        if (targetVectors.Length != distances.Length)
        {
            throw new ArgumentException("Target vectors and distances arrays must have the same length");
        }

        var results = new MagicSignature[targetVectors.Length];

        for (var i = 0; i < targetVectors.Length; i++)
        {
            MagicSignature target = targetVectors[i];
            var distance = distances[i];

            // Calculate distance falloff (inverse square law with minimum effect)
            var distanceFalloff = MathF.Max(0.1f, 1f / (1f + (distance * distance * 0.1f)));

            // Calculate resonance interaction
            MagicSignature resonance = CalculateResonance(epicenterVector, target);

            // Apply distance falloff
            results[i] = resonance * distanceFalloff;

            // Using Debug level instead of LogTrace
            _logger.Debug($"Area resonance - Target {i}: Distance {distance:F2}, Falloff {distanceFalloff:F2}");
        }

        return results;
    }

    /// <summary>
    ///     Calculates the complexity of a magic vector based on the number and strength of active elements.
    /// </summary>
    private static float CalculateVectorComplexity(MagicSignature vector)
    {
        var activeElements = 0f;
        var totalMagnitude = 0f;

        // Count elements with significant contribution (> 5% of total magnitude)
        var threshold = vector.Magnitude * 0.05f;

        for (var i = 0; i < 8; i++)
        {
            var elementStrength = MathF.Abs(vector[i]);
            if (elementStrength > threshold)
            {
                activeElements += 1f;
                totalMagnitude += elementStrength;
            }
        }

        // Complexity is based on number of active elements and their balance
        var balance = activeElements > 0 ? totalMagnitude / activeElements / vector.Magnitude : 0f;
        return activeElements * balance;
    }

    /// <summary>
    ///     Calculates magical resonance between two vectors.
    /// </summary>
    private static MagicSignature CalculateResonance(MagicSignature source, MagicSignature target)
    {
        var affinity = AspectCalculator.CalculateAspectAffinity(source, target);

        // Positive affinity creates amplifying resonance
        if (affinity > 0)
        {
            return MagicSignature.Lerp(target, source, affinity * 0.3f); // Up to 30% influence
        }

        // Negative affinity creates dampening resonance
        return target * (1f + (affinity * 0.2f)); // Up to 20% reduction
    }

    /// <summary>
    ///     Calculates secondary aspects that might emerge from element interactions.
    /// </summary>
    private static EleAspects.Aspect[] CalculateSecondaryAspects(EleAspects.Element element,
        EleAspects.Aspect primaryAspect)
    {
        var secondaryAspects = new List<EleAspects.Aspect>();

        // Get adjacent elements that might create secondary aspects
        // This is a simplified version - could be expanded with more complex adjacency rules
        var elementIndex = (int)element;
        var nextElement = (EleAspects.Element)((elementIndex + 1) % 8);
        var prevElement = (EleAspects.Element)((elementIndex + 7) % 8);

        // Add potential secondary aspects from adjacent elements
        var isPositive = element.Positive() == primaryAspect;
        if (isPositive)
        {
            secondaryAspects.Add(nextElement.Positive());
        }
        else
        {
            secondaryAspects.Add(prevElement.Negative());
        }

        return secondaryAspects.ToArray();
    }

    /// <summary>
    ///     Calculates the intensity of an aspect resolution.
    /// </summary>
    private static float CalculateAspectIntensity(EleAspects.Element element, EleAspects.Aspect aspect)
    {
        // Base intensity depends on aspect purity (how well it matches the element)
        var isPrimaryAspect = element.Positive() == aspect || element.Negative() == aspect;

        if (!isPrimaryAspect)
        {
            return 0f; // Invalid aspect for this element
        }

        // Primary aspects of their own element have high intensity
        return 0.8f; // High but not perfect - room for modifiers
    }
}
