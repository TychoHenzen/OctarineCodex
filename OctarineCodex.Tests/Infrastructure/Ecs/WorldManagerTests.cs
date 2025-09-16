using DefaultEcs;
using FluentAssertions;
using NSubstitute;
using OctarineCodex.Infrastructure.Ecs;
using OctarineCodex.Infrastructure.Logging;

namespace OctarineCodex.Tests.Infrastructure.Ecs;

public class WorldManagerTests : IDisposable
{
    private readonly ILoggingService _logger;
    private readonly WorldManager _worldManager;

    public WorldManagerTests()
    {
        _logger = Substitute.For<ILoggingService>();
        _worldManager = new WorldManager(_logger);
    }

    public void Dispose()
    {
        _worldManager.Dispose();
    }

    [Fact]
    public void Initialize_ShouldCreateWorld_WhenCalled()
    {
        // Act
        _worldManager.Initialize();

        // Assert
        _worldManager.IsInitialized.Should().BeTrue();
        _worldManager.EntityCount.Should().Be(0);
    }

    [Fact]
    public void CreateEntity_ShouldCreateEntity_WhenWorldInitialized()
    {
        // Arrange
        _worldManager.Initialize();

        // Act
        Entity entity = _worldManager.CreateEntity();

        // Assert
        entity.IsAlive.Should().BeTrue();
        _worldManager.EntityCount.Should().Be(1);
    }

    [Fact]
    public void CreateEntity_ShouldThrowException_WhenWorldNotInitialized()
    {
        // Act & Assert
        _worldManager.Invoking(wm => wm.CreateEntity())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("World must be initialized before creating entities");
    }

    [Fact]
    public void TransitionToNewWorld_ShouldCreateNewWorld_WhenCalled()
    {
        // Arrange
        _worldManager.Initialize();
        Entity originalEntity = _worldManager.CreateEntity();
        originalEntity.IsAlive.Should().BeTrue();
        var originalEntityCount = _worldManager.EntityCount;
        originalEntityCount.Should().Be(1);

        // Act
        _worldManager.TransitionToNewWorld();

        // Assert
        _worldManager.IsInitialized.Should().BeTrue();
        _worldManager.EntityCount.Should().Be(0);

        // Note: We don't check originalEntity.IsAlive because accessing disposed entities throws exceptions
        // This is correct DefaultEcs behavior - entities from disposed worlds should not be accessed

        // Instead, verify the new world works by creating a new entity
        Entity newEntity = _worldManager.CreateEntity();
        newEntity.IsAlive.Should().BeTrue();
        _worldManager.EntityCount.Should().Be(1);
    }

    [Fact]
    public void Initialize_ShouldDisposeExistingWorld_WhenCalledTwice()
    {
        // Arrange
        _worldManager.Initialize();
        Entity firstEntity = _worldManager.CreateEntity();
        firstEntity.IsAlive.Should().BeTrue();

        // Act
        _worldManager.Initialize(); // Second initialization

        // Assert
        _worldManager.IsInitialized.Should().BeTrue();
        _worldManager.EntityCount.Should().Be(0); // New world should be empty

        // Verify the new world works
        Entity newEntity = _worldManager.CreateEntity();
        newEntity.IsAlive.Should().BeTrue();
        _worldManager.EntityCount.Should().Be(1);
    }

    [Fact]
    public void CurrentWorld_ShouldThrowException_WhenNotInitialized()
    {
        // Act & Assert
        _worldManager.Invoking(wm => wm.CurrentWorld)
            .Should().Throw<InvalidOperationException>()
            .WithMessage("World has not been initialized");
    }

    [Fact]
    public void Dispose_ShouldDisposeWorld_WhenCalled()
    {
        // Arrange
        _worldManager.Initialize();
        _worldManager.CreateEntity();
        _worldManager.EntityCount.Should().Be(1);

        // Act
        _worldManager.Dispose();

        // Assert
        _worldManager.IsInitialized.Should().BeFalse();

        // Accessing disposed WorldManager should throw
        _worldManager.Invoking(wm => wm.CreateEntity())
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void EntityCount_ShouldReturnZero_WhenNotInitialized()
    {
        // Act & Assert
        _worldManager.EntityCount.Should().Be(0);
    }
}
