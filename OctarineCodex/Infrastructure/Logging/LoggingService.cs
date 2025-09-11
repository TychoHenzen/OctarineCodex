using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace OctarineCodex.Logging;

/// <summary>
/// File-based logging service with automatic log rotation and caller information capture.
/// Maintains the last 5 log files and formats entries with origin site information.
/// </summary>
public class LoggingService : ILoggingService, IDisposable
{
    private readonly string _logDirectory;
    private readonly string _currentLogFile;
    private readonly object _lock = new();
    private bool _disposed;

    public LoggingService()
    {
        _logDirectory = Path.Combine(Environment.CurrentDirectory, "Logs");
        Directory.CreateDirectory(_logDirectory);
        
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
        _currentLogFile = Path.Combine(_logDirectory, $"octarine_{timestamp}.log");
        
        // Rotate old log files (keep last 5)
        RotateLogFiles();
        
        // Write session start marker
        WriteLogEntry("INFO", "Session started", "LoggingService", "ctor", 0);
    }

    public void Debug(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        WriteLogEntry("DEBUG", message, ExtractClassName(sourceFilePath), memberName, sourceLineNumber);
    }

    public void Info(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        WriteLogEntry("INFO", message, ExtractClassName(sourceFilePath), memberName, sourceLineNumber);
    }

    public void Warn(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        WriteLogEntry("WARN", message, ExtractClassName(sourceFilePath), memberName, sourceLineNumber);
    }

    public void Error(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        WriteLogEntry("ERROR", message, ExtractClassName(sourceFilePath), memberName, sourceLineNumber);
    }

    public void Exception(Exception exception, string? message = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        var fullMessage = string.IsNullOrEmpty(message) 
            ? $"{exception.GetType().Name}: {exception.Message}"
            : $"{message} | {exception.GetType().Name}: {exception.Message}";
            
        WriteLogEntry("EXCEPTION", fullMessage, ExtractClassName(sourceFilePath), memberName, sourceLineNumber);
        
        // Also log stack trace as additional info
        if (!string.IsNullOrEmpty(exception.StackTrace))
        {
            WriteLogEntry("EXCEPTION", $"Stack trace: {exception.StackTrace}", ExtractClassName(sourceFilePath), memberName, sourceLineNumber);
        }
    }

    private void WriteLogEntry(string level, string message, string className, string memberName, int lineNumber)
    {
        if (_disposed)
        {
            return;
        }

        lock (_lock)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                var origin = $"[{className}.{memberName}:{lineNumber}]";
                var logLine = $"{timestamp} {level,-9} {origin,-40} {message}";
                
                File.AppendAllText(_currentLogFile, logLine + Environment.NewLine, Encoding.UTF8);
                
                // Also write to console for immediate visibility during development
                Console.WriteLine(logLine);
            }
            catch
            {
                // Silent failure - logging shouldn't crash the application
            }
        }
    }

    private static string ExtractClassName(string sourceFilePath)
    {
        if (string.IsNullOrEmpty(sourceFilePath))
        {
            return "Unknown";
        }

        try
        {
            var fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
            return fileName;
        }
        catch
        {
            return "Unknown";
        }
    }

    private void RotateLogFiles()
    {
        try
        {
            var logFiles = Directory.GetFiles(_logDirectory, "octarine_*.log")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .ToArray();

            // Keep only the last 4 files (plus the new one we're about to create = 5 total)
            var filesToDelete = logFiles.Skip(4);
            foreach (var file in filesToDelete)
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                    // Ignore deletion failures
                }
            }
        }
        catch
        {
            // Ignore rotation failures - logging should continue
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        WriteLogEntry("INFO", "Session ended", "LoggingService", "Dispose", 0);
        _disposed = true;
    }
}