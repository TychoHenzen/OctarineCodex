// OctarineCodex/Domain/Magic/IMagicCalculator.cs

namespace OctarineCodex.Domain.Magic;

/// <summary>
///     Interface for magic calculation services.
///     Provides high-level magical interaction calculations using the 8D vector system.
/// </summary>
public interface IMagicCalculator
{
    /// <summary>
    ///     Calculates the magical interaction result between a caster and target.
    /// </summary>
    /// <param name="caster">The caster's magic vector</param>
    /// <param name="target">The target's magic vector</param>
    /// <returns>The resulting magic vector from the interaction</returns>
    MagicSignature CalculateInteraction(MagicSignature caster, MagicSignature target);

    /// <summary>
    ///     Resolves aspects for a given element and primary aspect.
    /// </summary>
    /// <param name="element">The base element</param>
    /// <param name="primaryAspect">The primary aspect to resolve</param>
    /// <returns>The aspect resolution result</returns>
    AspectResult ResolveAspects(EleAspects.Element element, EleAspects.Aspect primaryAspect);

    /// <summary>
    ///     Calculates damage or healing based on magical interaction.
    /// </summary>
    /// <param name="attackVector">Attacker's magic vector</param>
    /// <param name="defenseVector">Defender's magic vector</param>
    /// <param name="baseDamage">Base damage before magical modifiers</param>
    /// <returns>Final damage/healing amount (negative values = healing)</returns>
    float CalculateMagicalDamage(MagicSignature attackVector, MagicSignature defenseVector, float baseDamage);

    /// <summary>
    ///     Determines spell effectiveness based on caster affinity and environmental factors.
    /// </summary>
    /// <param name="casterVector">Caster's magic vector</param>
    /// <param name="spellVector">Spell's required magic vector</param>
    /// <param name="environmentalVector">Environmental magic influences</param>
    /// <returns>Effectiveness multiplier (0.0 to 2.0+)</returns>
    float CalculateSpellEffectiveness(MagicSignature casterVector, MagicSignature spellVector,
        MagicSignature environmentalVector);

    /// <summary>
    ///     Calculates magical resonance for area-of-effect spells.
    /// </summary>
    /// <param name="epicenterVector">The spell's epicenter magic vector</param>
    /// <param name="targetVectors">All targets in the area</param>
    /// <param name="distance">Distance from epicenter for each target</param>
    /// <returns>Resonance effects for each target</returns>
    MagicSignature[] CalculateAreaResonance(MagicSignature epicenterVector, MagicSignature[] targetVectors,
        float[] distance);
}

/// <summary>
///     Result of aspect resolution calculations.
/// </summary>
/// <param name="PrimaryAspect">The resolved primary aspect</param>
/// <param name="SecondaryAspects">Any secondary aspects that emerged</param>
/// <param name="Intensity">The overall intensity of the resolution</param>
/// <param name="IsStable">Whether the aspect resolution is stable</param>
public readonly record struct AspectResult(
    EleAspects.Aspect PrimaryAspect,
    EleAspects.Aspect[] SecondaryAspects,
    float Intensity,
    bool IsStable
);
