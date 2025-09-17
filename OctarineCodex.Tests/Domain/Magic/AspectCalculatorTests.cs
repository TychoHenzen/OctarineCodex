// OctarineCodex.Tests/Domain/Magic/AspectCalculatorTests.cs

using FluentAssertions;
using OctarineCodex.Domain.Magic;

namespace OctarineCodex.Tests.Domain.Magic;

/// <summary>
///     Unit tests for the AspectCalculator utility class.
/// </summary>
public class AspectCalculatorTests
{
    [Fact]
    public void GetDominantAspect_WithSinglePositiveElement_ReturnsCorrectAspect()
    {
        // Arrange
        var vector = new MagicSignature(0f, 5f, 0f, 0f, 0f, 0f, 0f, 0f); // Strong Febris (fire)

        // Act
        var (element, aspect, strength) = AspectCalculator.GetDominantAspect(vector);

        // Assert
        element.Should().Be(EleAspects.Element.Febris);
        aspect.Should().Be(EleAspects.Aspect.Ignis);
        strength.Should().Be(5f);
    }

    [Fact]
    public void GetDominantAspect_WithSingleNegativeElement_ReturnsCorrectAspect()
    {
        // Arrange
        var vector = new MagicSignature(0f, -3f, 0f, 0f, 0f, 0f, 0f, 0f); // Strong Febris (water)

        // Act
        var (element, aspect, strength) = AspectCalculator.GetDominantAspect(vector);

        // Assert
        element.Should().Be(EleAspects.Element.Febris);
        aspect.Should().Be(EleAspects.Aspect.Hydris);
        strength.Should().Be(3f);
    }

    [Fact]
    public void CreateFromAspect_WithValidAspect_CreatesCorrectVector()
    {
        // Arrange
        var element = EleAspects.Element.Solidum;
        var aspect = EleAspects.Aspect.Tellus; // Positive aspect of Solidum
        var strength = 2.5f;

        // Act
        MagicSignature vector = AspectCalculator.CreateFromAspect(element, aspect, strength);

        // Assert
        vector[EleAspects.Element.Solidum].Should().Be(2.5f);
        for (var i = 1; i < 8; i++)
        {
            vector[i].Should().Be(0f);
        }
    }

    [Fact]
    public void CreateFromAspect_WithNegativeAspect_CreatesCorrectVector()
    {
        // Arrange
        var element = EleAspects.Element.Solidum;
        var aspect = EleAspects.Aspect.Aeolis; // Negative aspect of Solidum
        var strength = 1.5f;

        // Act
        MagicSignature vector = AspectCalculator.CreateFromAspect(element, aspect, strength);

        // Assert
        vector[EleAspects.Element.Solidum].Should().Be(-1.5f);
        for (var i = 1; i < 8; i++)
        {
            vector[i].Should().Be(0f);
        }
    }

    [Fact]
    public void CreateFromAspect_WithMismatchedElementAndAspect_ThrowsException()
    {
        // Arrange
        var element = EleAspects.Element.Solidum;
        var aspect = EleAspects.Aspect.Ignis; // Fire aspect (belongs to Febris, not Solidum)

        // Act & Assert
        Action act = () => AspectCalculator.CreateFromAspect(element, aspect);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Aspect Ignis does not belong to element Solidum*");
    }

    [Fact]
    public void CreateFromMultipleAspects_CombinesAspectsCorrectly()
    {
        // Arrange
        (EleAspects.Element, EleAspects.Aspect, float)[] aspects = new[]
        {
            (EleAspects.Element.Solidum, EleAspects.Aspect.Tellus, 2f),
            (EleAspects.Element.Febris, EleAspects.Aspect.Ignis, 3f),
            (EleAspects.Element.Solidum, EleAspects.Aspect.Aeolis, 1f) // Opposing aspect, should subtract
        };

        // Act
        MagicSignature vector = AspectCalculator.CreateFromMultipleAspects(aspects);

        // Assert
        vector[EleAspects.Element.Solidum].Should().Be(1f); // 2f - 1f = 1f
        vector[EleAspects.Element.Febris].Should().Be(3f);
        for (var i = 2; i < 8; i++)
        {
            vector[i].Should().Be(0f);
        }
    }

    [Fact]
    public void CalculateAspectAffinity_WithSimilarVectors_ReturnsHighAffinity()
    {
        // Arrange
        var vector1 = new MagicSignature(1f, 2f, 3f, 0f, 0f, 0f, 0f, 0f);
        var vector2 = new MagicSignature(2f, 4f, 6f, 0f, 0f, 0f, 0f, 0f); // Same direction, double magnitude

        // Act
        var affinity = AspectCalculator.CalculateAspectAffinity(vector1, vector2);

        // Assert
        affinity.Should().BeApproximately(1f, 0.001f); // Perfect alignment
    }

    [Fact]
    public void CalculateAspectAffinity_WithOppositeVectors_ReturnsNegativeAffinity()
    {
        // Arrange
        var vector1 = new MagicSignature(1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
        var vector2 = new MagicSignature(-1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f); // Opposite direction

        // Act
        var affinity = AspectCalculator.CalculateAspectAffinity(vector1, vector2);

        // Assert
        affinity.Should().BeApproximately(-1f, 0.001f); // Perfect opposition
    }

    [Fact]
    public void CalculateAspectAffinity_WithZeroMagnitudeVector_ReturnsZero()
    {
        // Arrange
        var vector1 = new MagicSignature(1f, 2f, 3f, 0f, 0f, 0f, 0f, 0f);
        MagicSignature vector2 = MagicSignature.Zero;

        // Act
        var affinity = AspectCalculator.CalculateAspectAffinity(vector1, vector2);

        // Assert
        affinity.Should().Be(0f);
    }

    [Fact]
    public void CalculateInteractionStrength_WithHighAffinityVectors_ReturnsHighStrength()
    {
        // Arrange
        var caster = new MagicSignature(3f, 4f, 0f, 0f, 0f, 0f, 0f, 0f); // Magnitude = 5
        var target = new MagicSignature(6f, 8f, 0f, 0f, 0f, 0f, 0f, 0f); // Same direction, magnitude = 10

        // Act
        var strength = AspectCalculator.CalculateInteractionStrength(caster, target);

        // Assert
        strength.Should().BeGreaterThan(0f);
        strength.Should().BeApproximately(7.5f, 0.1f); // Combined magnitude * affinity factor
    }

    [Fact]
    public void ResolveElementalOpposition_WithOpposingElements_CancelsPartially()
    {
        // Arrange
        var primary = new MagicSignature(5f, 0f, 0f, 0f, 0f, 0f, 0f, 0f); // +5 Solidum
        var secondary = new MagicSignature(-3f, 0f, 0f, 0f, 0f, 0f, 0f, 0f); // -3 Solidum

        // Act
        MagicSignature result = AspectCalculator.ResolveElementalOpposition(primary, secondary);

        // Assert
        // Calculation: cancelAmount = Min(5,3)*0.6 = 1.8
        // primaryRemaining = 1 * Max(0, 5-1.8) = 3.2
        // secondaryRemaining = -1 * Max(0, 3-1.8) = -1.2
        // result = 3.2 + (-1.2 * 0.3) = 2.84
        result[EleAspects.Element.Solidum].Should().BeApproximately(2.84f, 0.01f); // Updated from 2f
        for (var i = 1; i < 8; i++)
        {
            result[i].Should().Be(0f);
        }
    }

    [Fact]
    public void ResolveElementalOpposition_WithSameSignElements_Reinforces()
    {
        // Arrange
        var primary = new MagicSignature(3f, 0f, 0f, 0f, 0f, 0f, 0f, 0f); // +3 Solidum
        var secondary = new MagicSignature(2f, 0f, 0f, 0f, 0f, 0f, 0f, 0f); // +2 Solidum

        // Act
        MagicSignature result = AspectCalculator.ResolveElementalOpposition(primary, secondary);

        // Assert
        // Calculation: result = 3 + (2 * 0.4) = 3.8
        result[EleAspects.Element.Solidum].Should().BeApproximately(3.8f, 0.1f); // Updated from 4f
        for (var i = 1; i < 8; i++)
        {
            result[i].Should().Be(0f);
        }
    }


    [Fact]
    public void GetActiveAspects_WithMultipleActiveComponents_ReturnsCorrectAspects()
    {
// Arrange
// Arrange
        var vector = new MagicSignature(2f, -1.5f, 0.05f, 0f, 3f, 0f, -0.8f, 0f);
        var threshold = 0.1f;

        // Act
        (EleAspects.Aspect aspect, float strength)[] activeAspects =
            AspectCalculator.GetActiveAspects(vector, threshold);

        // Assert
        activeAspects.Should().HaveCount(4); // Components above threshold: 2f, -1.5f, 3f, -0.8f

        var aspectsDict = new Dictionary<EleAspects.Aspect, float>();
        foreach (var (aspect, strength) in activeAspects)
        {
            aspectsDict[aspect] = strength;
        }

        aspectsDict[EleAspects.Aspect.Tellus].Should().Be(2f); // Positive Solidum
        aspectsDict[EleAspects.Aspect.Hydris].Should().Be(1.5f); // Negative Febris
        aspectsDict[EleAspects.Aspect.Spatius].Should().Be(3f); // Positive Varias
        aspectsDict[EleAspects.Aspect.Malus].Should().Be(0.8f); // Negative Subsidium
    }

    [Fact]
    public void GetActiveAspects_WithAllComponentsBelowThreshold_ReturnsEmpty()
    {
        // Arrange
        var vector = new MagicSignature(0.05f, -0.03f, 0.08f, 0f, 0.02f, 0f, 0f, 0.09f);
        var threshold = 0.1f;

        // Act
        (EleAspects.Aspect aspect, float strength)[] activeAspects =
            AspectCalculator.GetActiveAspects(vector, threshold);

        // Assert
        activeAspects.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(0.001f)]
    [InlineData(0.5f)]
    [InlineData(1f)]
    [InlineData(2f)]
    public void CreateFromAspect_WithVariousStrengths_CreatesCorrectMagnitude(float strength)
    {
        // Arrange
        var element = EleAspects.Element.Lumines;
        var aspect = EleAspects.Aspect.Luminus;

        // Act
        MagicSignature vector = AspectCalculator.CreateFromAspect(element, aspect, strength);

        // Assert
        vector[EleAspects.Element.Lumines].Should().Be(strength);
        vector.Magnitude.Should().BeApproximately(strength, 0.001f);
    }
}
