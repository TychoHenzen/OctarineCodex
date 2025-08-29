using FluentAssertions;
using OctarineCodex.Logging;

namespace OctarineCodex.Tests.Logging;

public class LoggingServiceTests : IDisposable
{
    private readonly LoggingService _loggingService;
    private readonly string _testLogDirectory;

    public LoggingServiceTests()
    {
        _loggingService = new LoggingService();
        _testLogDirectory = Path.Combine(Environment.CurrentDirectory, "Logs");
    }

    [Fact]
    public void Constructor_ShouldCreateLogsDirectory()
    {
        // Assert
        Directory.Exists(_testLogDirectory).Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateLogFileWithTimestamp()
    {
        // Arrange & Act - constructor already called
        
        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "octarine_*.log");
        logFiles.Should().NotBeEmpty();
        logFiles.Should().Contain(file => File.GetCreationTime(file) >= DateTime.Now.AddMinutes(-1));
    }

    [Fact]
    public void Debug_ShouldWriteToLogFileWithCallerInfo()
    {
        // Arrange
        var testMessage = "Test debug message";
        
        // Act
        _loggingService.Debug(testMessage); // This line number is important for the test
        
        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "octarine_*.log");
        var latestLogFile = logFiles.OrderByDescending(File.GetCreationTime).First();
        var logContent = File.ReadAllText(latestLogFile);
        
        logContent.Should().Contain("DEBUG");
        logContent.Should().Contain(testMessage);
        logContent.Should().Contain("[LoggingServiceTests.Debug_ShouldWriteToLogFileWithCallerInfo:");
        logContent.Should().Contain("Test debug message");
    }

    [Fact]
    public void Info_ShouldWriteToLogFileWithCorrectLevel()
    {
        // Arrange
        var testMessage = "Test info message";
        
        // Act
        _loggingService.Info(testMessage);
        
        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "octarine_*.log");
        var latestLogFile = logFiles.OrderByDescending(File.GetCreationTime).First();
        var logContent = File.ReadAllText(latestLogFile);
        
        logContent.Should().Contain("INFO");
        logContent.Should().Contain(testMessage);
        logContent.Should().Contain("[LoggingServiceTests.Info_ShouldWriteToLogFileWithCorrectLevel:");
    }

    [Fact]
    public void Warn_ShouldWriteToLogFileWithCorrectLevel()
    {
        // Arrange
        var testMessage = "Test warning message";
        
        // Act
        _loggingService.Warn(testMessage);
        
        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "octarine_*.log");
        var latestLogFile = logFiles.OrderByDescending(File.GetCreationTime).First();
        var logContent = File.ReadAllText(latestLogFile);
        
        logContent.Should().Contain("WARN");
        logContent.Should().Contain(testMessage);
        logContent.Should().Contain("[LoggingServiceTests.Warn_ShouldWriteToLogFileWithCorrectLevel:");
    }

    [Fact]
    public void Error_ShouldWriteToLogFileWithCorrectLevel()
    {
        // Arrange
        var testMessage = "Test error message";
        
        // Act
        _loggingService.Error(testMessage);
        
        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "octarine_*.log");
        var latestLogFile = logFiles.OrderByDescending(File.GetCreationTime).First();
        var logContent = File.ReadAllText(latestLogFile);
        
        logContent.Should().Contain("ERROR");
        logContent.Should().Contain(testMessage);
        logContent.Should().Contain("[LoggingServiceTests.Error_ShouldWriteToLogFileWithCorrectLevel:");
    }

    [Fact]
    public void Exception_ShouldLogExceptionDetailsWithStackTrace()
    {
        // Arrange
        var testException = new InvalidOperationException("Test exception message");
        var additionalMessage = "Additional context";
        
        // Act
        _loggingService.Exception(testException, additionalMessage);
        
        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "octarine_*.log");
        var latestLogFile = logFiles.OrderByDescending(File.GetCreationTime).First();
        var logContent = File.ReadAllText(latestLogFile);
        
        logContent.Should().Contain("EXCEPTION");
        logContent.Should().Contain("InvalidOperationException");
        logContent.Should().Contain("Test exception message");
        logContent.Should().Contain(additionalMessage);
        logContent.Should().Contain("[LoggingServiceTests.Exception_ShouldLogExceptionDetailsWithStackTrace:");
    }

    [Fact]
    public void Exception_WithoutAdditionalMessage_ShouldLogExceptionOnly()
    {
        // Arrange
        var testException = new ArgumentNullException("testParam", "Test null exception");
        
        // Act
        _loggingService.Exception(testException);
        
        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "octarine_*.log");
        var latestLogFile = logFiles.OrderByDescending(File.GetCreationTime).First();
        var logContent = File.ReadAllText(latestLogFile);
        
        logContent.Should().Contain("EXCEPTION");
        logContent.Should().Contain("ArgumentNullException");
        logContent.Should().Contain("Test null exception");
        logContent.Should().NotContain(" | "); // Should not have the separator since no additional message
    }

    [Fact]
    public void LogEntries_ShouldHaveConsistentFormat()
    {
        // Arrange & Act
        _loggingService.Debug("Debug test");
        _loggingService.Info("Info test");
        _loggingService.Warn("Warn test");
        _loggingService.Error("Error test");
        
        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "octarine_*.log");
        var latestLogFile = logFiles.OrderByDescending(File.GetCreationTime).First();
        var logLines = File.ReadAllLines(latestLogFile);
        
        foreach (var line in logLines.Where(l => l.Contains("test")))
        {
            // Each line should have: timestamp, level, [caller info], message
            line.Should().MatchRegex(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} \w+\s+\[.*\]\s+.*test.*$");
        }
    }

    [Fact]
    public void Dispose_ShouldWriteSessionEndedMessage()
    {
        // Arrange
        using var service = new LoggingService();
        
        // Act
        service.Dispose();
        
        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "octarine_*.log");
        var latestLogFile = logFiles.OrderByDescending(File.GetCreationTime).First();
        var logContent = File.ReadAllText(latestLogFile);
        
        logContent.Should().Contain("Session ended");
        logContent.Should().Contain("[LoggingService.Dispose:0]");
    }

    public void Dispose()
    {
        _loggingService?.Dispose();
        
        // Clean up test log files
        try
        {
            if (Directory.Exists(_testLogDirectory))
            {
                var testLogFiles = Directory.GetFiles(_testLogDirectory, "octarine_*.log");
                foreach (var file in testLogFiles)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // Ignore cleanup failures
                    }
                }
            }
        }
        catch
        {
            // Ignore cleanup failures
        }
    }
}