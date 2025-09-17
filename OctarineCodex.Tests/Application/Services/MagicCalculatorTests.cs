// OctarineCodex.Tests/Application/Services/MagicCalculatorTests.cs

using FluentAssertions;
using NSubstitute;
using OctarineCodex.Application.Services;
using OctarineCodex.Domain.Magic;
using OctarineCodex.Infrastructure.Logging;

namespace OctarineCodex.Tests.Application.Services;

/// <summary>
///     Unit tests for the MagicCalculator service.
/// </summary>
public class MagicCalculatorTests
{
    private readonly IMagicCalculator _magicCalculator;
    private readonly LoggingService _mockLogger;

    public MagicCalculatorTests()
    {
        _mockLogger = Substitute.For<LoggingService>();
        _magicCalculator = new MagicCalculator(_mockLogger);
    }

    [Fact]
    public void CalculateInteraction_WithCompatibleVectors_ReturnsPositiveInteraction()
    {
        // Arrange
        var caster = new MagicSignature(3f, 4f, 0f, 0f, 0f, 0f, 0f, 0f);
        var target = new MagicSignature(1f, 1.5f, 0f, 0f, 0f, 0f, 0f, 0f); // Similar direction

        // Act
        MagicSignature result = _magicCalculator.CalculateInteraction(caster, target);

        // Assert
        result.Magnitude.Should().BeGreaterThan(0f);

        // The result should be influenced by both resonance and interaction strength
        var affinity = AspectCalculator.CalculateAspectAffinity(caster, target);
        affinity.Should().BeGreaterThan(0.8f); // High affinity expected
    }

    [Fact]
    public void CalculateInteraction_WithOpposingVectors_ReturnsWeakerInteraction()
    {
        // Arrange
        var caster = new MagicSignature(5f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
        var target = new MagicSignature(-3f, 0f, 0f, 0f, 0f, 0f, 0f, 0f); // Opposing direction

        // Act
        MagicSignature result = _magicCalculator.CalculateInteraction(caster, target);

        // Assert
        result.Magnitude.Should().BeLessThan(caster.Magnitude);

        // Should still have some interaction due to shared element
        result.Magnitude.Should().BeGreaterThan(0f);
    }

    [Fact]
    public void ResolveAspects_WithValidElementAndAspect_ReturnsCorrectResult()
    {
        // Arrange
        var element = EleAspects.Element.Febris;
        var primaryAspect = EleAspects.Aspect.Ignis;

        // Act
        AspectResult result = _magicCalculator.ResolveAspects(element, primaryAspect);

        // Assert
        result.PrimaryAspect.Should().Be(primaryAspect);
        result.Intensity.Should().BeGreaterThan(0.5f); // Should have high intensity for primary aspect
        result.IsStable.Should().BeTrue();
        result.SecondaryAspects.Should().NotBeEmpty(); // Should generate some secondary aspects
    }

    [Fact]
    public void ResolveAspects_WithMismatchedElementAndAspect_ReturnsUnstableResult()
    {
        // Arrange
        var element = EleAspects.Element.Solidum;
        var invalidAspect = EleAspects.Aspect.Ignis; // Fire aspect doesn't belong to Solidum

        // Act
        AspectResult result = _magicCalculator.ResolveAspects(element, invalidAspect);

        // Assert
        result.PrimaryAspect.Should().Be(EleAspects.Aspect.Tellus); // Should default to positive aspect of element
        result.Intensity.Should().Be(0f);
        result.IsStable.Should().BeFalse();
        result.SecondaryAspects.Should().BeEmpty();
    }

    [Fact]
    public void CalculateMagicalDamage_WithHighAffinity_AmplifiersDamage()
    {
        // Arrange
        var attackVector = new MagicSignature(5f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
        var defenseVector = new MagicSignature(3f, 0f, 0f, 0f, 0f, 0f, 0f, 0f); // Same direction
        var baseDamage = 10f;

        // Act
        var finalDamage = _magicCalculator.CalculateMagicalDamage(attackVector, defenseVector, baseDamage);

        // Assert
        finalDamage.Should().BeGreaterThan(baseDamage); // High affinity should amplify damage
        finalDamage.Should().BeLessThan(baseDamage * 3f); // But not exceed reasonable bounds
    }

    [Fact]
    public void CalculateMagicalDamage_WithOpposingVectors_ReducesDamage()
    {
        // Arrange
        var attackVector = new MagicSignature(5f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
        var defenseVector = new MagicSignature(-3f, 0f, 0f, 0f, 0f, 0f, 0f, 0f); // Opposing direction
        var baseDamage = 10f;

        // Act
        var finalDamage = _magicCalculator.CalculateMagicalDamage(attackVector, defenseVector, baseDamage);

        // Assert
        finalDamage.Should().BeLessThan(baseDamage); // Opposition should reduce damage
        finalDamage.Should().BeGreaterThan(0f); // But still have some effect
    }

    [Fact]
    public void CalculateSpellEffectiveness_WithPerfectCasterAffinity_ReturnsHighEffectiveness()
    {
        // Arrange
        var casterVector = new MagicSignature(3f, 4f, 0f, 0f, 0f, 0f, 0f, 0f);
        var spellVector = new MagicSignature(6f, 8f, 0f, 0f, 0f, 0f, 0f, 0f); // Same direction
        MagicSignature environmentalVector = MagicSignature.Zero; // Neutral environment

        // Act
        var effectiveness =
            _magicCalculator.CalculateSpellEffectiveness(casterVector, spellVector, environmentalVector);

        // Assert
        effectiveness.Should().BeGreaterThan(0.8f); // High effectiveness for perfect alignment
        effectiveness.Should().BeLessThanOrEqualTo(2f); // Reasonable upper bound
    }

    [Fact]
    public void CalculateSpellEffectiveness_WithPoorCasterAffinity_ReturnsLowEffectiveness()
    {
        // Arrange
        var casterVector = new MagicSignature(1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
        var spellVector = new MagicSignature(-1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f); // Opposing direction
        MagicSignature environmentalVector = MagicSignature.Zero;

        // Act
        var effectiveness =
            _magicCalculator.CalculateSpellEffectiveness(casterVector, spellVector, environmentalVector);

        // Assert
        effectiveness.Should().BeLessThan(0.5f); // Low effectiveness for poor alignment
        effectiveness.Should().BeGreaterThanOrEqualTo(0f); // Never negative
    }

    [Fact]
    public void CalculateSpellEffectiveness_WithPositiveEnvironment_BoostsEffectiveness()
    {
        // Arrange
        var casterVector = new MagicSignature(1f, 1f, 0f, 0f, 0f, 0f, 0f, 0f);
        var spellVector = new MagicSignature(1f, 1f, 0f, 0f, 0f, 0f, 0f, 0f);
        var environmentalVector = new MagicSignature(2f, 2f, 0f, 0f, 0f, 0f, 0f, 0f); // Supportive environment

        // Act
        var effectiveness =
            _magicCalculator.CalculateSpellEffectiveness(casterVector, spellVector, environmentalVector);

        // Assert
        effectiveness.Should().BeGreaterThan(1f); // Environment should boost effectiveness
    }

    [Fact]
    public void CalculateAreaResonance_WithMultipleTargets_ReturnsCorrectResonanceArray()
    {
        // Arrange
        var epicenterVector = new MagicSignature(5f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
        var targetVectors = new[]
        {
            new MagicSignature(3f, 0f, 0f, 0f, 0f, 0f, 0f, 0f), // Close alignment
            new MagicSignature(-2f, 0f, 0f, 0f, 0f, 0f, 0f, 0f), // Opposing
            new MagicSignature(0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f) // Different element
        };
        var distances = new[] { 1f, 2f, 3f };

        // Act
        MagicSignature[] resonanceResults =
            _magicCalculator.CalculateAreaResonance(epicenterVector, targetVectors, distances);

        // Assert
        resonanceResults.Should().HaveCount(3);

        // Closer targets should have stronger resonance effects (all other things being equal)
        resonanceResults[0].Magnitude.Should().BeGreaterThan(resonanceResults[1].Magnitude);
        resonanceResults[1].Magnitude.Should().BeGreaterThan(resonanceResults[2].Magnitude);
    }

    [Fact]
    public void CalculateAreaResonance_WithMismatchedArrayLengths_ThrowsException()
    {
        // Arrange
        MagicSignature epicenterVector = MagicSignature.One;
        MagicSignature[] targetVectors = new[] { MagicSignature.Zero, MagicSignature.One };
        var distances = new[] { 1f }; // Mismatched length

        // Act & Assert
        Action act = () => _magicCalculator.CalculateAreaResonance(epicenterVector, targetVectors, distances);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Target vectors and distances arrays must have the same length");
    }

    [Fact]
    public void CalculateAreaResonance_WithZeroDistance_AppliesMinimumFalloff()
    {
        // Arrange
        var epicenterVector = new MagicSignature(1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
        var targetVectors = new[] { new MagicSignature(1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f) };
        var distances = new[] { 0f }; // Zero distance

        // Act
        MagicSignature[] resonanceResults =
            _magicCalculator.CalculateAreaResonance(epicenterVector, targetVectors, distances);

        // Assert
        resonanceResults[0].Magnitude.Should().BeGreaterThan(0f); // Should still have effect
        // The resonance should be strong since distance is zero (minimum falloff applies)
    }

    [Theory]
    [InlineData(1f, 2f, 3f)]
    [InlineData(0.5f, 1f, 1.5f)]
    [InlineData(10f, 20f, 30f)]
    public void CalculateMagicalDamage_WithVariousBaseDamages_ScalesCorrectly(float baseDamage, float expectedMin,
        float expectedMax)
    {
        // Arrange
        var attackVector = new MagicSignature(2f, 2f, 0f, 0f, 0f, 0f, 0f, 0f);
        var defenseVector = new MagicSignature(1f, 1f, 0f, 0f, 0f, 0f, 0f, 0f);

        // Act
        var finalDamage = _magicCalculator.CalculateMagicalDamage(attackVector, defenseVector, baseDamage);

        // Assert
        finalDamage.Should().BeInRange(expectedMin, expectedMax); // Should scale with base damage
        finalDamage.Should().BeGreaterThan(0f);
    }

    [Fact]
    public void CalculateInteraction_LogsDebugInformation()
    {
        // Arrange
        var caster = new MagicSignature(1f, 2f, 0f, 0f, 0f, 0f, 0f, 0f);
        var target = new MagicSignature(2f, 4f, 0f, 0f, 0f, 0f, 0f, 0f);

        // Act
        _magicCalculator.CalculateInteraction(caster, target);

        // Assert
        _mockLogger.Received().Debug(
            Arg.Is<string>(s => s.Contains("Calculating magic interaction")));

        _mockLogger.Received().Debug(
            Arg.Is<string>(s => s.Contains("Magic interaction result")));
    }
}
