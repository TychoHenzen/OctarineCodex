using FluentAssertions;
using NSubstitute;
using OctarineCodex.Infrastructure.Ecs;
using OctarineCodex.Infrastructure.Logging;

namespace OctarineCodex.Tests.Infrastructure.Ecs;

public class ComponentRegistryTests
{
    private readonly ILoggingService _logger;
    private readonly ComponentRegistry _registry;

    public ComponentRegistryTests()
    {
        _logger = Substitute.For<ILoggingService>();
        _registry = new ComponentRegistry(_logger);
    }

    [Fact]
    public void RegisterComponent_ShouldRegisterComponent_WhenValidStructProvided()
    {
        // Act
        _registry.RegisterComponent<TestComponent>();

        // Assert
        _registry.IsRegistered<TestComponent>().Should().BeTrue();
        _registry.RegisteredComponents.Should().Contain(typeof(TestComponent));
    }

    [Fact]
    public void RegisterComponent_ShouldThrowException_WhenClassProvided()
    {
        // Act & Assert
        _registry.Invoking(r => r.RegisterComponent(typeof(TestClass)))
            .Should().Throw<ArgumentException>()
            .WithMessage("*must be a struct*");
    }

    [Fact]
    public void GetComponentType_ShouldReturnType_WhenComponentRegistered()
    {
        // Arrange
        _registry.RegisterComponent<TestComponent>();

        // Act
        Type? componentType = _registry.GetComponentType("TestComponent");

        // Assert
        componentType.Should().Be(typeof(TestComponent));
    }

    [Fact]
    public void GetComponentType_ShouldReturnNull_WhenComponentNotRegistered()
    {
        // Act
        Type? componentType = _registry.GetComponentType("NonExistentComponent");

        // Assert
        componentType.Should().BeNull();
    }

    [Fact]
    public void ValidateComponents_ShouldReturnValidResult_WhenNoComponentsRegistered()
    {
        // Act
        ComponentRegistryValidationResult result = _registry.ValidateComponents();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void DiscoverComponents_ShouldFindComponentsInNamespace()
    {
        // Act
        _registry.DiscoverComponents();

        // Assert - Should find at least the components we created
        _registry.RegisteredComponents.Should().NotBeEmpty();
    }

    // Test component for unit testing
    private struct TestComponent
    {
        public int Value;
        public float Speed;
    }

    // Test class for negative testing
    private class TestClass
    {
        public int Value { get; set; }
    }
}
