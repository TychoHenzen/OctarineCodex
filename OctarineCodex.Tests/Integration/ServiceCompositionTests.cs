using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OctarineCodex.Input;
using OctarineCodex.Maps;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace OctarineCodex.Tests.Integration;

/// <summary>
/// Integration tests for service composition and core application functionality.
/// Verifies that dependency injection works correctly and core services can be instantiated
/// without requiring graphics or user interaction.
/// </summary>
public class ServiceCompositionTests
{
    [Fact]
    public void ServiceConfiguration_ShouldRegisterAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOctarineServices();
        using var serviceProvider = services.BuildServiceProvider();

        // Assert
        var inputService = serviceProvider.GetService<IInputService>();
        var mapService = serviceProvider.GetService<ILdtkMapService>();
        var mapRenderer = serviceProvider.GetService<ILdtkMapRenderer>();

        inputService.Should().NotBeNull();
        mapService.Should().NotBeNull();
        mapRenderer.Should().NotBeNull();
    }

    [Fact]
    public void ServiceConfiguration_ShouldCreateCorrectImplementations()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOctarineServices();
        using var serviceProvider = services.BuildServiceProvider();

        // Assert
        var inputService = serviceProvider.GetRequiredService<IInputService>();
        var mapService = serviceProvider.GetRequiredService<ILdtkMapService>();
        var mapRenderer = serviceProvider.GetRequiredService<ILdtkMapRenderer>();

        inputService.Should().BeOfType<CompositeInputService>();
        mapService.Should().BeOfType<LdtkMonoGameMapService>();
        mapRenderer.Should().BeOfType<LdtkMapRenderer>();
    }

    [Fact]
    public void ServiceConfiguration_ShouldSupportMultipleServiceProviderInstances()
    {
        // Arrange
        var services1 = new ServiceCollection();
        var services2 = new ServiceCollection();

        // Act
        services1.AddOctarineServices();
        services2.AddOctarineServices();
        
        using var serviceProvider1 = services1.BuildServiceProvider();
        using var serviceProvider2 = services2.BuildServiceProvider();

        // Assert
        var inputService1 = serviceProvider1.GetRequiredService<IInputService>();
        var inputService2 = serviceProvider2.GetRequiredService<IInputService>();

        inputService1.Should().NotBeNull();
        inputService2.Should().NotBeNull();
        inputService1.Should().NotBeSameAs(inputService2);
    }

    [Fact]
    public async Task CoreServices_ShouldFunctionWithoutGraphics()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOctarineServices();
        using var serviceProvider = services.BuildServiceProvider();

        var mapService = serviceProvider.GetRequiredService<ILdtkMapService>();

        // Act
        var project = await mapService.LoadProjectAsync("non_existent_file.ldtk");
        var levels = mapService.GetAllLevels();
        var specificLevel = mapService.GetLevel("NonExistent");
        var currentProject = mapService.GetCurrentProject();

        // Assert
        project.Should().BeNull();
        levels.Should().BeEmpty();
        specificLevel.Should().BeNull();
        currentProject.Should().BeNull();
        mapService.IsProjectLoaded.Should().BeFalse();
    }

    [Fact]
    public async Task LdtkMapService_ShouldLoadValidFile()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOctarineServices();
        using var serviceProvider = services.BuildServiceProvider();

        var mapService = serviceProvider.GetRequiredService<ILdtkMapService>();
        var testLdtkPath = Path.Combine("OctarineCodex", "Content", "test_level2.ldtk");

        // Act
        var project = await mapService.LoadProjectAsync(testLdtkPath);

        // Assert
        if (File.Exists(testLdtkPath))
        {
            project.Should().NotBeNull();
            mapService.IsProjectLoaded.Should().BeTrue();
            mapService.GetAllLevels().Should().NotBeEmpty();
            
            // Verify the specific level we expect
            var entranceLevel = mapService.GetLevel("Entrance");
            entranceLevel.Should().NotBeNull();
            entranceLevel!.Identifier.Should().Be("Entrance");
        }
        else
        {
            project.Should().BeNull();
            mapService.IsProjectLoaded.Should().BeFalse();
        }
    }

    [Fact]
    public void InputService_ShouldFunctionWithoutInput()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOctarineServices();
        using var serviceProvider = services.BuildServiceProvider();

        var inputService = serviceProvider.GetRequiredService<IInputService>();

        // Act & Assert - Should not throw and should provide default values
        var movement = inputService.GetMovementDirection();
        var exitPressed = inputService.IsExitPressed();
        var primaryAction = inputService.IsPrimaryActionPressed();
        var secondaryAction = inputService.IsSecondaryActionPressed();

        movement.Should().Be(Microsoft.Xna.Framework.Vector2.Zero);
        exitPressed.Should().BeFalse();
        primaryAction.Should().BeFalse();
        secondaryAction.Should().BeFalse();
    }

    [Fact]
    public void ServiceLifetimes_ShouldBeConfiguredCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOctarineServices();
        using var serviceProvider = services.BuildServiceProvider();

        // Act
        var inputService1 = serviceProvider.GetRequiredService<IInputService>();
        var inputService2 = serviceProvider.GetRequiredService<IInputService>();
        
        var mapService1 = serviceProvider.GetRequiredService<ILdtkMapService>();
        var mapService2 = serviceProvider.GetRequiredService<ILdtkMapService>();
        
        var mapRenderer1 = serviceProvider.GetRequiredService<ILdtkMapRenderer>();
        var mapRenderer2 = serviceProvider.GetRequiredService<ILdtkMapRenderer>();

        // Assert
        // Singletons should be the same instance
        inputService1.Should().BeSameAs(inputService2);
        mapService1.Should().BeSameAs(mapService2);
        
        // Transients should be different instances
        mapRenderer1.Should().NotBeSameAs(mapRenderer2);
    }
}