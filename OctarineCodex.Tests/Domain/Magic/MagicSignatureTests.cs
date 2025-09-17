// OctarineCodex.Tests/Domain/Magic/MagicSignatureTests.cs

using FluentAssertions;
using OctarineCodex.Domain.Magic;

namespace OctarineCodex.Tests.Domain.Magic;

/// <summary>
///     Unit tests for the MagicSignature struct.
///     Validates all mathematical operations and behavior.
/// </summary>
public class MagicSignatureTests
{
    [Fact]
    public void Constructor_WithEightValues_SetsComponentsCorrectly()
    {
        // Arrange & Act
        var vector = new MagicSignature(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f);

        // Assert
        vector[EleAspects.Element.Solidum].Should().Be(1f);
        vector[EleAspects.Element.Febris].Should().Be(2f);
        vector[EleAspects.Element.Ordinem].Should().Be(3f);
        vector[EleAspects.Element.Lumines].Should().Be(4f);
        vector[EleAspects.Element.Varias].Should().Be(5f);
        vector[EleAspects.Element.Inertiae].Should().Be(6f);
        vector[EleAspects.Element.Subsidium].Should().Be(7f);
        vector[EleAspects.Element.Spatium].Should().Be(8f);
    }

    [Fact]
    public void Constructor_WithArray_SetsComponentsCorrectly()
    {
        // Arrange
        var values = new[] { 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f };

        // Act
        var vector = new MagicSignature(values);

        // Assert
        for (var i = 0; i < 8; i++)
        {
            vector[i].Should().Be(values[i]);
        }
    }

    [Fact]
    public void Constructor_WithInvalidArrayLength_ThrowsException()
    {
        // Arrange
        var invalidValues = new[] { 1f, 2f, 3f }; // Only 3 values instead of 8

        // Act & Assert
        Action act = () => new MagicSignature(invalidValues);
        act.Should().Throw<ArgumentException>()
            .WithMessage("MagicSignature requires exactly 8 values*");
    }

    [Fact]
    public void Zero_ReturnsVectorWithAllZeroComponents()
    {
        // Act
        MagicSignature zero = MagicSignature.Zero;

        // Assert
        for (var i = 0; i < 8; i++)
        {
            zero[i].Should().Be(0f);
        }
    }

    [Fact]
    public void One_ReturnsVectorWithAllOneComponents()
    {
        // Act
        MagicSignature one = MagicSignature.One;

        // Assert
        for (var i = 0; i < 8; i++)
        {
            one[i].Should().Be(1f);
        }
    }

    [Fact]
    public void Magnitude_CalculatesCorrectly()
    {
        // Arrange
        var vector = new MagicSignature(3f, 4f, 0f, 0f, 0f, 0f, 0f, 0f); // 3-4-0... triangle

        // Act
        var magnitude = vector.Magnitude;

        // Assert
        magnitude.Should().BeApproximately(5f, 0.001f); // √(3² + 4²) = 5
    }

    [Fact]
    public void MagnitudeSquared_CalculatesCorrectly()
    {
        // Arrange
        var vector = new MagicSignature(3f, 4f, 0f, 0f, 0f, 0f, 0f, 0f);

        // Act
        var magnitudeSquared = vector.MagnitudeSquared;

        // Assert
        magnitudeSquared.Should().Be(25f); // 3² + 4² = 25
    }

    [Fact]
    public void Normalize_WithNonZeroVector_ReturnsUnitVector()
    {
        // Arrange
        var vector = new MagicSignature(3f, 4f, 0f, 0f, 0f, 0f, 0f, 0f);

        // Act
        MagicSignature normalized = vector.Normalize();

        // Assert
        normalized.Magnitude.Should().BeApproximately(1f, 0.001f);
        normalized[0].Should().BeApproximately(0.6f, 0.001f); // 3/5
        normalized[1].Should().BeApproximately(0.8f, 0.001f); // 4/5
    }

    [Fact]
    public void Normalize_WithZeroVector_ReturnsZero()
    {
        // Arrange
        MagicSignature vector = MagicSignature.Zero;

        // Act
        MagicSignature normalized = vector.Normalize();

        // Assert
        normalized.Should().Be(MagicSignature.Zero);
    }

    [Fact]
    public void Addition_CombinesVectorsCorrectly()
    {
        // Arrange
        var vector1 = new MagicSignature(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f);
        var vector2 = new MagicSignature(8f, 7f, 6f, 5f, 4f, 3f, 2f, 1f);

        // Act
        MagicSignature result = vector1 + vector2;

        // Assert
        for (var i = 0; i < 8; i++)
        {
            result[i].Should().Be(9f); // Each component should be 9
        }
    }

    [Fact]
    public void Subtraction_SubtractsVectorsCorrectly()
    {
        // Arrange
        var vector1 = new MagicSignature(8f, 7f, 6f, 5f, 4f, 3f, 2f, 1f);
        var vector2 = new MagicSignature(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f);

        // Act
        MagicSignature result = vector1 - vector2;

        // Assert
        result[0].Should().Be(7f);
        result[1].Should().Be(5f);
        result[2].Should().Be(3f);
        result[3].Should().Be(1f);
        result[4].Should().Be(-1f);
        result[5].Should().Be(-3f);
        result[6].Should().Be(-5f);
        result[7].Should().Be(-7f);
    }

    [Fact]
    public void ScalarMultiplication_ScalesVectorCorrectly()
    {
        // Arrange
        var vector = new MagicSignature(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f);
        var scalar = 2.5f;

        // Act
        MagicSignature result = vector * scalar;

        // Assert
        for (var i = 0; i < 8; i++)
        {
            result[i].Should().Be(vector[i] * scalar);
        }
    }

    [Fact]
    public void ScalarDivision_DividesVectorCorrectly()
    {
        // Arrange
        var vector = new MagicSignature(2f, 4f, 6f, 8f, 10f, 12f, 14f, 16f);
        var divisor = 2f;

        // Act
        MagicSignature result = vector / divisor;

        // Assert
        for (var i = 0; i < 8; i++)
        {
            result[i].Should().Be(vector[i] / divisor);
        }
    }

    [Fact]
    public void ScalarDivision_ByZero_ThrowsException()
    {
        // Arrange
        MagicSignature vector = MagicSignature.One;

        // Act & Assert
        Func<MagicSignature> act = () => vector / 0f;
        act.Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void DotProduct_CalculatesCorrectly()
    {
        // Arrange
        var vector1 = new MagicSignature(1f, 2f, 3f, 4f, 0f, 0f, 0f, 0f);
        var vector2 = new MagicSignature(4f, 3f, 2f, 1f, 0f, 0f, 0f, 0f);

        // Act
        var dotProduct = MagicSignature.Dot(vector1, vector2);

        // Assert
        dotProduct.Should().Be(20f); // 1*4 + 2*3 + 3*2 + 4*1 = 4 + 6 + 6 + 4 = 20
    }

    [Fact]
    public void Distance_CalculatesCorrectly()
    {
        // Arrange
        var vector1 = new MagicSignature(1f, 2f, 0f, 0f, 0f, 0f, 0f, 0f);
        var vector2 = new MagicSignature(4f, 6f, 0f, 0f, 0f, 0f, 0f, 0f);

        // Act
        var distance = MagicSignature.Distance(vector1, vector2);

        // Assert
        distance.Should().BeApproximately(5f, 0.001f); // √((4-1)² + (6-2)²) = √(9+16) = 5
    }

    [Fact]
    public void Lerp_InterpolatesCorrectly()
    {
        // Arrange
        var from = new MagicSignature(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
        var to = new MagicSignature(10f, 20f, 30f, 40f, 50f, 60f, 70f, 80f);

        // Act
        MagicSignature result = MagicSignature.Lerp(from, to, 0.5f);

        // Assert
        result[0].Should().Be(5f);
        result[1].Should().Be(10f);
        result[2].Should().Be(15f);
        result[3].Should().Be(20f);
        result[4].Should().Be(25f);
        result[5].Should().Be(30f);
        result[6].Should().Be(35f);
        result[7].Should().Be(40f);
    }

    [Fact]
    public void Clamp_ClampsComponentsCorrectly()
    {
        // Arrange
        var vector = new MagicSignature(-5f, -2f, 0f, 2f, 5f, 8f, 10f, 15f);

        // Act
        MagicSignature clamped = vector.Clamp(-1f, 3f);

        // Assert
        clamped[0].Should().Be(-1f); // Clamped from -5
        clamped[1].Should().Be(-1f); // Clamped from -2
        clamped[2].Should().Be(0f); // No change
        clamped[3].Should().Be(2f); // No change
        clamped[4].Should().Be(3f); // Clamped from 5
        clamped[5].Should().Be(3f); // Clamped from 8
        clamped[6].Should().Be(3f); // Clamped from 10
        clamped[7].Should().Be(3f); // Clamped from 15
    }

    [Fact]
    public void Project_ProjectsVectorCorrectly()
    {
        // Arrange
        var vector = new MagicSignature(3f, 4f, 0f, 0f, 0f, 0f, 0f, 0f);
        var onto = new MagicSignature(1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f); // Unit vector along first axis

        // Act
        MagicSignature projection = vector.Project(onto);

        // Assert
        projection[0].Should().BeApproximately(3f, 0.001f); // Projection of (3,4,0...) onto (1,0,0...) is (3,0,0...)
        projection[1].Should().BeApproximately(0f, 0.001f);
        for (var i = 2; i < 8; i++)
        {
            projection[i].Should().BeApproximately(0f, 0.001f);
        }
    }

    [Theory]
    [InlineData(0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f)]
    [InlineData(-1f, -2f, -3f, -4f, -5f, -6f, -7f, -8f)]
    public void Equality_WorksCorrectly(float v1, float v2, float v3, float v4, float v5, float v6, float v7, float v8)
    {
        // Arrange
        var vector1 = new MagicSignature(v1, v2, v3, v4, v5, v6, v7, v8);
        var vector2 = new MagicSignature(v1, v2, v3, v4, v5, v6, v7, v8);
        var vector3 = new MagicSignature(v1 + 1f, v2, v3, v4, v5, v6, v7, v8);

        // Act & Assert
        (vector1 == vector2).Should().BeTrue();
        (vector1 != vector3).Should().BeTrue();
        vector1.Equals(vector2).Should().BeTrue();
        vector1.Equals(vector3).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_ConsistentForEqualVectors()
    {
        // Arrange
        var vector1 = new MagicSignature(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f);
        var vector2 = new MagicSignature(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f);

        // Act & Assert
        vector1.GetHashCode().Should().Be(vector2.GetHashCode());
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        // Arrange
        var vector = new MagicSignature(1.5f, 2.7f, 3.14f, 4.0f, 5.25f, 6.6f, 7.77f, 8.88f);

        // Act
        var result = vector.ToString();

        // Assert
        result.Should().Contain("MagicSignature");
        result.Should().Contain("1.50");
        result.Should().Contain("2.70");
    }

    [Fact]
    public void ToElementString_FormatsWithElementNames()
    {
        // Arrange
        var vector = new MagicSignature(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f);

        // Act
        var result = vector.ToElementString();

        // Assert
        result.Should().Contain("Solidum:1.00");
        result.Should().Contain("Febris:2.00");
        result.Should().Contain("Spatium:8.00");
    }
}
