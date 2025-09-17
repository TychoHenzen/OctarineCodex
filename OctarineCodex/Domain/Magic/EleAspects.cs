// OctarineCodex/Domain/Magic/EleAspects.cs (Enhanced Version)

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace OctarineCodex.Domain.Magic;

/// <summary>
///     Enhanced magic system definitions for Elements and Aspects with 8D vector integration.
///     Provides the foundation for the magical system's mathematical operations.
/// </summary>
public static class EleAspects
{
    public enum Aspect
    {
        Ignis, //fire
        Hydris, // water
        Tellus, // earth
        Aeolis, // air
        Empyrus, // chaos
        Vitrio, // order
        Luminus, // light
        Noctis, // dark
        Spatius, // Space
        Tempus, // Time
        Gravitas, // Heavy
        Levitas, // light
        Auxillus, // Helpful
        Malus, // Harmful
        Iuxta, // Nearby
        Disis // Distant
    }

    public enum Element
    {
        Solidum, // solidity (air-rock) - Index 0
        Febris, // temperature (water-fire) - Index 1
        Ordinem, // orderedness (entropy-order) - Index 2
        Lumines, // luminance (dark-light) - Index 3
        Varias, // Manifold (time-space) - Index 4
        Inertiae, // Density (heavy-light) - Index 5
        Subsidium, // Helpfulness (harmful-helpful) - Index 6
        Spatium // Distance (nearby-distant) - Index 7
    }

    /// <summary>
    ///     Gets all elements as an array for iteration.
    /// </summary>
    public static Element[] AllElements => new[]
    {
        Element.Solidum, Element.Febris, Element.Ordinem, Element.Lumines, Element.Varias, Element.Inertiae,
        Element.Subsidium, Element.Spatium
    };

    /// <summary>
    ///     Gets all aspects as an array for iteration.
    /// </summary>
    public static Aspect[] AllAspects => new[]
    {
        Aspect.Ignis, Aspect.Hydris, Aspect.Tellus, Aspect.Aeolis, Aspect.Empyrus, Aspect.Vitrio, Aspect.Luminus,
        Aspect.Noctis, Aspect.Spatius, Aspect.Tempus, Aspect.Gravitas, Aspect.Levitas, Aspect.Auxillus,
        Aspect.Malus, Aspect.Iuxta, Aspect.Disis
    };

    /// <summary>
    ///     Gets the positive aspect for an element.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Aspect Positive(this Element e)
    {
        var conversion = new Dictionary<Element, Aspect>
        {
            { Element.Solidum, Aspect.Tellus },
            { Element.Febris, Aspect.Ignis },
            { Element.Ordinem, Aspect.Vitrio },
            { Element.Lumines, Aspect.Luminus },
            { Element.Varias, Aspect.Spatius },
            { Element.Inertiae, Aspect.Gravitas },
            { Element.Subsidium, Aspect.Auxillus },
            { Element.Spatium, Aspect.Disis }
        };
        return conversion[e];
    }

    /// <summary>
    ///     Gets the negative aspect for an element.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Aspect Negative(this Element e)
    {
        var conversion = new Dictionary<Element, Aspect>
        {
            { Element.Solidum, Aspect.Aeolis },
            { Element.Febris, Aspect.Hydris },
            { Element.Ordinem, Aspect.Empyrus },
            { Element.Lumines, Aspect.Noctis },
            { Element.Varias, Aspect.Tempus },
            { Element.Inertiae, Aspect.Levitas },
            { Element.Subsidium, Aspect.Malus },
            { Element.Spatium, Aspect.Iuxta }
        };
        return conversion[e];
    }

    /// <summary>
    ///     Gets the element that an aspect belongs to.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Element IsElement(this Aspect a)
    {
        var conversion = new Dictionary<Aspect, Element>
        {
            { Aspect.Aeolis, Element.Solidum },
            { Aspect.Hydris, Element.Febris },
            { Aspect.Empyrus, Element.Ordinem },
            { Aspect.Noctis, Element.Lumines },
            { Aspect.Tempus, Element.Varias },
            { Aspect.Levitas, Element.Inertiae },
            { Aspect.Malus, Element.Subsidium },
            { Aspect.Iuxta, Element.Spatium },
            { Aspect.Tellus, Element.Solidum },
            { Aspect.Ignis, Element.Febris },
            { Aspect.Vitrio, Element.Ordinem },
            { Aspect.Luminus, Element.Lumines },
            { Aspect.Spatius, Element.Varias },
            { Aspect.Gravitas, Element.Inertiae },
            { Aspect.Auxillus, Element.Subsidium },
            { Aspect.Disis, Element.Spatium }
        };
        return conversion[a];
    }

    // *** NEW: 8D Vector Integration Utilities ***

    /// <summary>
    ///     Gets the index of an element in the 8D vector array.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetVectorIndex(this Element element)
    {
        return (int)element;
    }

    /// <summary>
    ///     Gets the element from a vector index.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Element GetElementFromIndex(int index)
    {
        if (index < 0 || index >= 8)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Element index must be 0-7");
        }

        return (Element)index;
    }

    /// <summary>
    ///     Creates a MagicSignature with only the specified element set to the given value.
    /// </summary>
    /// <param name="element">The element to set</param>
    /// <param name="value">The value to set (positive for positive aspect, negative for negative aspect)</param>
    /// <returns>A MagicSignature with the single element set</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MagicSignature ToMagicVector(this Element element, float value = 1f)
    {
        var values = new float[8];
        values[(int)element] = value;
        return new MagicSignature(values);
    }

    /// <summary>
    ///     Creates a MagicSignature with the specified aspect set to positive or negative based on the aspect type.
    /// </summary>
    /// <param name="aspect">The aspect to create a vector for</param>
    /// <param name="strength">The strength/magnitude of the aspect (default: 1.0)</param>
    /// <returns>A MagicSignature representing the aspect</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MagicSignature ToMagicVector(this Aspect aspect, float strength = 1f)
    {
        Element element = aspect.IsElement();
        var isPositive = element.Positive() == aspect;
        var value = isPositive ? strength : -strength;

        return element.ToMagicVector(value);
    }

    /// <summary>
    ///     Gets the opposing aspect for a given aspect (same element, opposite polarity).
    /// </summary>
    /// <param name="aspect">The aspect to get the opposite of</param>
    /// <returns>The opposing aspect</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Aspect GetOpposite(this Aspect aspect)
    {
        Element element = aspect.IsElement();
        var isPositive = element.Positive() == aspect;
        return isPositive ? element.Negative() : element.Positive();
    }

    /// <summary>
    ///     Determines if two aspects are opposing (same element, different polarity).
    /// </summary>
    /// <param name="aspect1">First aspect</param>
    /// <param name="aspect2">Second aspect</param>
    /// <returns>True if aspects are opposing</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AreOpposing(Aspect aspect1, Aspect aspect2)
    {
        return aspect1.IsElement() == aspect2.IsElement() && aspect1 != aspect2;
    }

    /// <summary>
    ///     Gets the element name as a human-readable string.
    /// </summary>
    public static string GetElementName(this Element element)
    {
        return element switch
        {
            Element.Solidum => "Solidity",
            Element.Febris => "Temperature",
            Element.Ordinem => "Order",
            Element.Lumines => "Luminance",
            Element.Varias => "Manifold",
            Element.Inertiae => "Density",
            Element.Subsidium => "Helpfulness",
            Element.Spatium => "Distance",
            _ => element.ToString()
        };
    }

    /// <summary>
    ///     Gets the aspect description with its elemental context.
    /// </summary>
    public static string GetAspectDescription(this Aspect aspect)
    {
        return aspect switch
        {
            Aspect.Ignis => "Fire (Temperature+)",
            Aspect.Hydris => "Water (Temperature-)",
            Aspect.Tellus => "Earth (Solidity+)",
            Aspect.Aeolis => "Air (Solidity-)",
            Aspect.Empyrus => "Chaos (Order-)",
            Aspect.Vitrio => "Order (Order+)",
            Aspect.Luminus => "Light (Luminance+)",
            Aspect.Noctis => "Dark (Luminance-)",
            Aspect.Spatius => "Space (Manifold+)",
            Aspect.Tempus => "Time (Manifold-)",
            Aspect.Gravitas => "Heavy (Density+)",
            Aspect.Levitas => "Light (Density-)",
            Aspect.Auxillus => "Helpful (Helpfulness+)",
            Aspect.Malus => "Harmful (Helpfulness-)",
            Aspect.Iuxta => "Nearby (Distance-)",
            Aspect.Disis => "Distant (Distance+)",
            _ => aspect.ToString()
        };
    }

    /// <summary>
    ///     Creates a random MagicSignature for testing or procedural generation.
    /// </summary>
    /// <param name="random">Random number generator</param>
    /// <param name="minValue">Minimum component value</param>
    /// <param name="maxValue">Maximum component value</param>
    /// <returns>A random MagicSignature</returns>
    public static MagicSignature CreateRandomMagicVector(Random random, float minValue = -1f, float maxValue = 1f)
    {
        var values = new float[8];
        for (var i = 0; i < 8; i++)
        {
            values[i] = minValue + ((maxValue - minValue) * (float)random.NextDouble());
        }

        return new MagicSignature(values);
    }
}
