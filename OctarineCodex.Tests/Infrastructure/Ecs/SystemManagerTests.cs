using FluentAssertions;
using Microsoft.Xna.Framework;
using NSubstitute;
using OctarineCodex.Application.Systems;
using OctarineCodex.Infrastructure.Ecs;
using OctarineCodex.Infrastructure.Logging;

namespace OctarineCodex.Tests.Infrastructure.Ecs;

public class SystemManagerTests : IDisposable
{
    private readonly ILoggingService _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly SystemManager _systemManager;

    public SystemManagerTests()
    {
        _logger = Substitute.For<ILoggingService>();
        _serviceProvider = Substitute.For<IServiceProvider>();
        _systemManager = new SystemManager(_serviceProvider, _logger);
    }

    public void Dispose()
    {
        _systemManager.Dispose();
    }

    [Fact]
    public void GetDiagnostics_ShouldReturnEmptyDiagnostics_WhenNoSystemsRegistered()
    {
        // Act
        SystemManagerDiagnostics diagnostics = _systemManager.GetDiagnostics();

        // Assert
        diagnostics.UpdateSystemCount.Should().Be(0);
        diagnostics.DrawSystemCount.Should().Be(0);
        diagnostics.UpdateSystemTypes.Should().BeEmpty();
        diagnostics.DrawSystemTypes.Should().BeEmpty();
    }

    [Fact]
    public void RegisterUpdateSystem_ShouldThrowException_WhenDisposed()
    {
        // Arrange
        _systemManager.Dispose();

        // Act & Assert
        _systemManager.Invoking(sm => sm.RegisterUpdateSystem<TestSystem>())
            .Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void Update_ShouldThrowException_WhenDisposed()
    {
        // Arrange
        _systemManager.Dispose();
        var gameTime = new GameTime();

        // Act & Assert
        _systemManager.Invoking(sm => sm.Update(gameTime))
            .Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void Draw_ShouldThrowException_WhenDisposed()
    {
        // Arrange
        _systemManager.Dispose();
        var gameTime = new GameTime();

        // Act & Assert
        _systemManager.Invoking(sm => sm.Draw(gameTime))
            .Should().Throw<ObjectDisposedException>();
    }

    // Test system for unit testing
    private class TestSystem : ISystem
    {
        public void Update(GameTime gameTime) { }
        public void Draw(GameTime gameTime) { }
    }
}
