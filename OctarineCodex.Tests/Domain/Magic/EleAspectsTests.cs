// OctarineCodex.Tests/Domain/Magic/EleAspectsTests.cs

using FluentAssertions;
using OctarineCodex.Domain.Magic;

namespace OctarineCodex.Tests.Domain.Magic;

/// <summary>
///     Unit tests for the enhanced EleAspects static class.
///     Tests both existing functionality and new 8D vector integration.
/// </summary>
public class EleAspectsTests
{
    [Theory]
    [InlineData(EleAspects.Element.Solidum, EleAspects.Aspect.Tellus)]
    [InlineData(EleAspects.Element.Febris, EleAspects.Aspect.Ignis)]
    [InlineData(EleAspects.Element.Ordinem, EleAspects.Aspect.Vitrio)]
    [InlineData(EleAspects.Element.Lumines, EleAspects.Aspect.Luminus)]
    [InlineData(EleAspects.Element.Varias, EleAspects.Aspect.Spatius)]
    [InlineData(EleAspects.Element.Inertiae, EleAspects.Aspect.Gravitas)]
    [InlineData(EleAspects.Element.Subsidium, EleAspects.Aspect.Auxillus)]
    [InlineData(EleAspects.Element.Spatium, EleAspects.Aspect.Disis)]
    public void Positive_ReturnsCorrectPositiveAspect(EleAspects.Element element, EleAspects.Aspect expectedAspect)
    {
        // Act
        EleAspects.Aspect result = element.Positive();

        // Assert
        result.Should().Be(expectedAspect);
    }

    [Theory]
    [InlineData(EleAspects.Element.Solidum, EleAspects.Aspect.Aeolis)]
    [InlineData(EleAspects.Element.Febris, EleAspects.Aspect.Hydris)]
    [InlineData(EleAspects.Element.Ordinem, EleAspects.Aspect.Empyrus)]
    [InlineData(EleAspects.Element.Lumines, EleAspects.Aspect.Noctis)]
    [InlineData(EleAspects.Element.Varias, EleAspects.Aspect.Tempus)]
    [InlineData(EleAspects.Element.Inertiae, EleAspects.Aspect.Levitas)]
    [InlineData(EleAspects.Element.Subsidium, EleAspects.Aspect.Malus)]
    [InlineData(EleAspects.Element.Spatium, EleAspects.Aspect.Iuxta)]
    public void Negative_ReturnsCorrectNegativeAspect(EleAspects.Element element, EleAspects.Aspect expectedAspect)
    {
        // Act
        EleAspects.Aspect result = element.Negative();

        // Assert
        result.Should().Be(expectedAspect);
    }

    [Theory]
    [InlineData(EleAspects.Aspect.Tellus, EleAspects.Element.Solidum)]
    [InlineData(EleAspects.Aspect.Aeolis, EleAspects.Element.Solidum)]
    [InlineData(EleAspects.Aspect.Ignis, EleAspects.Element.Febris)]
    [InlineData(EleAspects.Aspect.Hydris, EleAspects.Element.Febris)]
    public void IsElement_ReturnsCorrectElement(EleAspects.Aspect aspect, EleAspects.Element expectedElement)
    {
        // Act
        EleAspects.Element result = aspect.IsElement();

        // Assert
        result.Should().Be(expectedElement);
    }

    // *** NEW: 8D Vector Integration Tests ***

    [Theory]
    [InlineData(EleAspects.Element.Solidum, 0)]
    [InlineData(EleAspects.Element.Febris, 1)]
    [InlineData(EleAspects.Element.Ordinem, 2)]
    [InlineData(EleAspects.Element.Lumines, 3)]
    [InlineData(EleAspects.Element.Varias, 4)]
    [InlineData(EleAspects.Element.Inertiae, 5)]
    [InlineData(EleAspects.Element.Subsidium, 6)]
    [InlineData(EleAspects.Element.Spatium, 7)]
    public void GetVectorIndex_ReturnsCorrectIndex(EleAspects.Element element, int expectedIndex)
    {
        // Act
        var result = element.GetVectorIndex();

        // Assert
        result.Should().Be(expectedIndex);
    }

    [Theory]
    [InlineData(0, EleAspects.Element.Solidum)]
    [InlineData(1, EleAspects.Element.Febris)]
    [InlineData(7, EleAspects.Element.Spatium)]
    public void GetElementFromIndex_ReturnsCorrectElement(int index, EleAspects.Element expectedElement)
    {
        // Act
        EleAspects.Element result = EleAspects.GetElementFromIndex(index);

        // Assert
        result.Should().Be(expectedElement);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(8)]
    [InlineData(100)]
    public void GetElementFromIndex_WithInvalidIndex_ThrowsException(int invalidIndex)
    {
        // Act & Assert
        Action act = () => EleAspects.GetElementFromIndex(invalidIndex);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("Element index must be 0-7*");
    }

    [Fact]
    public void ToMagicVector_Element_CreatesVectorWithSingleComponent()
    {
        // Arrange
        var element = EleAspects.Element.Febris;
        var value = 3.5f;

        // Act
        MagicSignature result = element.ToMagicVector(value);

        // Assert
        result[EleAspects.Element.Febris].Should().Be(value);

        // All other components should be zero
        foreach (EleAspects.Element otherElement in EleAspects.AllElements.Where(e => e != element))
        {
            result[otherElement].Should().Be(0f);
        }
    }

    [Fact]
    public void ToMagicVector_PositiveAspect_CreatesPositiveComponent()
    {
        // Arrange
        var aspect = EleAspects.Aspect.Ignis; // Positive aspect of Febris
        var strength = 2.5f;

        // Act
        MagicSignature result = aspect.ToMagicVector(strength);

        // Assert
        result[EleAspects.Element.Febris].Should().Be(strength);

        // All other components should be zero
        for (var i = 0; i < 8; i++)
        {
            if (i != (int)EleAspects.Element.Febris)
            {
                result[i].Should().Be(0f);
            }
        }
    }

    [Fact]
    public void ToMagicVector_NegativeAspect_CreatesNegativeComponent()
    {
        // Arrange
        var aspect = EleAspects.Aspect.Hydris; // Negative aspect of Febris
        var strength = 2.5f;

        // Act
        MagicSignature result = aspect.ToMagicVector(strength);

        // Assert
        result[EleAspects.Element.Febris].Should().Be(-strength);

        // All other components should be zero
        for (var i = 0; i < 8; i++)
        {
            if (i != (int)EleAspects.Element.Febris)
            {
                result[i].Should().Be(0f);
            }
        }
    }

    [Fact]
    public void AllElements_ContainsAllEightElements()
    {
        // Act
        EleAspects.Element[] allElements = EleAspects.AllElements;

        // Assert
        allElements.Should().HaveCount(8);
        allElements.Should().Contain(EleAspects.Element.Solidum);
        allElements.Should().Contain(EleAspects.Element.Febris);
        allElements.Should().Contain(EleAspects.Element.Ordinem);
        allElements.Should().Contain(EleAspects.Element.Lumines);
        allElements.Should().Contain(EleAspects.Element.Varias);
        allElements.Should().Contain(EleAspects.Element.Inertiae);
        allElements.Should().Contain(EleAspects.Element.Subsidium);
        allElements.Should().Contain(EleAspects.Element.Spatium);
    }

    [Fact]
    public void AllAspects_ContainsAllSixteenAspects()
    {
        // Act
        EleAspects.Aspect[] allAspects = EleAspects.AllAspects;

        // Assert
        allAspects.Should().HaveCount(16);

        // Should contain all positive and negative aspects
        foreach (EleAspects.Element element in EleAspects.AllElements)
        {
            allAspects.Should().Contain(element.Positive());
            allAspects.Should().Contain(element.Negative());
        }
    }

    [Theory]
    [InlineData(EleAspects.Aspect.Ignis, EleAspects.Aspect.Hydris)]
    [InlineData(EleAspects.Aspect.Tellus, EleAspects.Aspect.Aeolis)]
    [InlineData(EleAspects.Aspect.Luminus, EleAspects.Aspect.Noctis)]
    public void GetOpposite_ReturnsOpposingAspect(EleAspects.Aspect aspect, EleAspects.Aspect expectedOpposite)
    {
        // Act
        EleAspects.Aspect result = aspect.GetOpposite();

        // Assert
        result.Should().Be(expectedOpposite);
    }

    [Theory]
    [InlineData(EleAspects.Aspect.Ignis, EleAspects.Aspect.Hydris, true)]
    [InlineData(EleAspects.Aspect.Tellus, EleAspects.Aspect.Aeolis, true)]
    [InlineData(EleAspects.Aspect.Ignis, EleAspects.Aspect.Tellus, false)]
    [InlineData(EleAspects.Aspect.Ignis, EleAspects.Aspect.Ignis, false)]
    public void AreOpposing_ReturnsCorrectResult(EleAspects.Aspect aspect1, EleAspects.Aspect aspect2,
        bool expectedResult)
    {
        // Act
        var result = EleAspects.AreOpposing(aspect1, aspect2);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(EleAspects.Element.Solidum, "Solidity")]
    [InlineData(EleAspects.Element.Febris, "Temperature")]
    [InlineData(EleAspects.Element.Ordinem, "Order")]
    public void GetElementName_ReturnsReadableName(EleAspects.Element element, string expectedName)
    {
        // Act
        var result = element.GetElementName();

        // Assert
        result.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(EleAspects.Aspect.Ignis, "Fire (Temperature+)")]
    [InlineData(EleAspects.Aspect.Hydris, "Water (Temperature-)")]
    [InlineData(EleAspects.Aspect.Tellus, "Earth (Solidity+)")]
    public void GetAspectDescription_ReturnsDescriptiveText(EleAspects.Aspect aspect, string expectedDescription)
    {
        // Act
        var result = aspect.GetAspectDescription();

        // Assert
        result.Should().Be(expectedDescription);
    }

    [Fact]
    public void CreateRandomMagicVector_GeneratesValidVector()
    {
        // Arrange
        var random = new Random(42); // Fixed seed for reproducible tests
        var minValue = -2f;
        var maxValue = 3f;

        // Act
        MagicSignature result = EleAspects.CreateRandomMagicVector(random, minValue, maxValue);

        // Assert
        for (var i = 0; i < 8; i++)
        {
            result[i].Should().BeInRange(minValue, maxValue);
        }

        // Should not be zero vector (very unlikely with this range)
        result.Magnitude.Should().BeGreaterThan(0f);
    }

    [Fact]
    public void CreateRandomMagicVector_WithDefaultRange_GeneratesNormalizedRange()
    {
        // Arrange
        var random = new Random(42);

        // Act
        MagicSignature result = EleAspects.CreateRandomMagicVector(random);

        // Assert
        for (var i = 0; i < 8; i++)
        {
            result[i].Should().BeInRange(-1f, 1f);
        }
    }

    [Fact]
    public void CreateRandomMagicVector_WithDifferentSeeds_GeneratesDifferentVectors()
    {
        // Arrange
        var random1 = new Random(1);
        var random2 = new Random(2);

        // Act
        MagicSignature result1 = EleAspects.CreateRandomMagicVector(random1);
        MagicSignature result2 = EleAspects.CreateRandomMagicVector(random2);

        // Assert
        result1.Should().NotBe(result2);
    }
}
