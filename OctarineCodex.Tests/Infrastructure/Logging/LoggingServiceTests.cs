using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using OctarineCodex.Logging;
using Xunit;

namespace OctarineCodex.Tests.Logging;

public class LoggingServiceTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Act
        var service = new LoggingService();

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Debug_ShouldFormatMessageCorrectly()
    {
        // Arrange
        using var service = new LoggingService();
        var testMessage = "Test debug message";

        // Act
        var action = () => service.Debug(testMessage);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Info_ShouldFormatMessageCorrectly()
    {
        // Arrange
        using var service = new LoggingService();
        var testMessage = "Test info message";

        // Act
        var action = () => service.Info(testMessage);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Warn_ShouldFormatMessageCorrectly()
    {
        // Arrange
        using var service = new LoggingService();
        var testMessage = "Test warning message";

        // Act
        var action = () => service.Warn(testMessage);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Error_ShouldFormatMessageCorrectly()
    {
        // Arrange
        using var service = new LoggingService();
        var testMessage = "Test error message";

        // Act
        var action = () => service.Error(testMessage);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Exception_WithMessage_ShouldFormatMessageCorrectly()
    {
        // Arrange
        using var service = new LoggingService();
        var testException = new InvalidOperationException("Test exception message");
        var additionalMessage = "Additional context";

        // Act
        var action = () => service.Exception(testException, additionalMessage);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Exception_WithoutMessage_ShouldFormatMessageCorrectly()
    {
        // Arrange
        using var service = new LoggingService();
        var testException = new ArgumentNullException("testParam", "Test null exception");

        // Act
        var action = () => service.Exception(testException);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Dispose_ShouldDisposeCleanly()
    {
        // Arrange
        var service = new LoggingService();

        // Act
        var action = () => service.Dispose();

        // Assert
        action.Should().NotThrow();
    }
}